using System;
using System.Drawing;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Data;
using DataGridViewAutoFilter;
using Test.Helpers;

namespace Test
{
    public partial class Form1 : Form
    {
        private ConnectionManager _connectionManager;
        private DataRepository _repository;
        private DatabaseService _dbService;
        private CsvExportService _csvExportService;
        private ExcelExportService _excelExportService;
        private Test.Services.CsvImportService _csvImportService;
        private Test.Services.ExcelImportService _excelImportService;
        private ScaleData? _lastScaleData;
        private System.Collections.Generic.List<ScaleRecord> _allRecords = new System.Collections.Generic.List<ScaleRecord>();

        // Analytics
        private PlotModel _plotModel = null!;
        private LineSeries _weightSeries = null!;
        private decimal? _sessionMin;
        private decimal? _sessionMax;

        // Auto Scaling
        private Size _originalFormSize;
        private System.Collections.Generic.Dictionary<Control, Rectangle> _originalControlRects = new System.Collections.Generic.Dictionary<Control, Rectangle>();
        private System.Collections.Generic.Dictionary<Control, float> _originalFonts = new System.Collections.Generic.Dictionary<Control, float>();
        private const int MaxChartPoints = 300;


        // [R-01] Producer-Consumer: Background thread enqueues, WinForms Timer dequeues in batch.
        // ConcurrentQueue<T> là lock-free — an toàn cho write từ background và read từ UI thread.
        private readonly System.Collections.Concurrent.ConcurrentQueue<ScaleData> _dataQueue
            = new System.Collections.Concurrent.ConcurrentQueue<ScaleData>();
        private System.Windows.Forms.Timer _uiTimer = null!;
        private System.Windows.Forms.Timer _autoBackupTimer = null!;
        private BindingSource _bindingSource = new BindingSource();

        // Roles & Auth
        private bool _isAdmin = false;
        private Button _btnLogin = null!;
        private TabPage? _tpSettings;

        // Server status indicator
        private Label _lblServerStatus = null!;
        private System.Windows.Forms.Timer _serverPingTimer = null!;

        // In-memory error log
        private readonly System.Collections.Generic.List<string> _errorLog
            = new System.Collections.Generic.List<string>();
        private TextBox? _txtErrorLog;

        private void LogError(string context, Exception ex)
            => LogError($"[{context}] {ex.GetType().Name}: {ex.Message}");

        private void LogError(string message)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _errorLog.Add(entry);
            if (_txtErrorLog != null && !_txtErrorLog.IsDisposed)
            {
                if (_txtErrorLog.InvokeRequired)
                    _txtErrorLog.Invoke(() => { _txtErrorLog.AppendText(entry + Environment.NewLine); });
                else
                    _txtErrorLog.AppendText(entry + Environment.NewLine);
            }
        }

        private enum AutoPollingState
        {
            WaitingForZero,
            ReadyToWeigh,
            WeightCaptured
        }
        private AutoPollingState _autoPollingState = AutoPollingState.WaitingForZero;
        private const decimal ZeroThreshold = 0.05m;

        public Form1()
        {
            InitializeComponent();
            _connectionManager = new ConnectionManager();
            _connectionManager.OnStateChanged += ConnectionManager_OnStateChanged;
            _connectionManager.OnDataReceived += ConnectionManager_OnDataReceived;

            _repository = new DataRepository();
            var config = AppConfig.Load();
            _dbService = new DatabaseService(string.IsNullOrWhiteSpace(config.DeviceId) ? System.Environment.MachineName : config.DeviceId);
            _csvExportService = new CsvExportService();
            _excelExportService = new ExcelExportService();
            _csvImportService = new Test.Services.CsvImportService();
            _excelImportService = new Test.Services.ExcelImportService();

            btnConnectIpadd.Click += btnConnectIpadd_Click;
            btnPolling.Click += btnPolling_Click;
            btnExportdata.Click += btnExportdata_Click;
            btnImportData.Click += btnImportData_Click;
            tboSearch.TextChanged += tboSearch_TextChanged;

            tboNat.KeyDown += Tbo_KeyDown;
            tboBatch.KeyDown += Tbo_KeyDown;
            tboSamplename.KeyDown += Tbo_KeyDown;
            tboLocation.KeyDown += Tbo_KeyDown;
            tboTester.KeyDown += Tbo_KeyDown;

            trayIcon.Icon = this.Icon;
            trayIcon.MouseDoubleClick += (s, e) => RestoreFromTray();

            // Tray Menu context is now native, removing manual override
            trayIcon.ContextMenuStrip = trayContextMenu;
            trayMenuOpen.Click += (s, e) => RestoreFromTray();
            trayMenuExit.Click += (s, e) => this.Close();

            // Initialize new Settings Tab
            InitializeSettingsTab();

            // Setup Auto Backup Timer
            int intervalHours = AppConfig.Load().AutoBackupIntervalHours;
            if (intervalHours <= 0) intervalHours = 4;
            _autoBackupTimer = new System.Windows.Forms.Timer { Interval = intervalHours * 60 * 60 * 1000 };
            _autoBackupTimer.Tick += (s, e) => _ = PerformAutoBackupAsync();
            _autoBackupTimer.Start();

            // Wire up background sync errors to the UI error log
            Test.Services.NetworkService.Instance.OnError += msg => LogError(msg);

            this.FormClosing += Form1_FormClosing;

            InitializeDataGridView();
            InitializeChart();
            LoadDataToGrid();
            UpdateStats();

            // [R-01] UI refresh timer: thay thế BeginInvoke flooding bằng batch 100ms
            _uiTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();

            // Custom tab rendering
            tcDashboard.DrawItem += TcDashboard_DrawItem;
            tcDashboard.SelectedIndexChanged += (s, e) => tcDashboard.Invalidate();

            // Hook for scaling
            this.Load += Form1_Load;
        }

        private async void btnConnectIpadd_Click(object? sender, EventArgs e)
        {
            // Dùng state thật thay vì so sánh text nút (tránh bug khi text thay đổi)
            if (_connectionManager.IsConnected || _connectionManager.IsReconnecting)
            {
                _connectionManager.Disconnect();
            }
            else
            {
                string ip = tboIpadd.Text.Trim();
                if (string.IsNullOrEmpty(ip) || !System.Net.IPAddress.TryParse(ip, out _))
                {
                    MessageBox.Show("Vui lòng nhập IP hợp lệ (VD: 192.168.1.100).", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(tboTcpport.Text.Trim(), out int port) || port < 1 || port > 65535)
                {
                    MessageBox.Show("Vui lòng nhập Port hợp lệ (1 - 65535).", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnConnectIpadd.Enabled = false;
                await _connectionManager.ConnectAsync(ip, port);
            }
        }

        private void ConnectionManager_OnStateChanged(bool isConnected, bool isReconnecting)
        {
            // [FIX B6] Guard: tránh crash ObjectDisposedException khi Form đã đóng
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => ConnectionManager_OnStateChanged(isConnected, isReconnecting))); }
                catch (ObjectDisposedException) { }
                return;
            }

            if (isConnected)
            {
                lbStatusconnection.Text = "⬤  CONNECTED";
                lbStatusconnection.BackColor = AppColors.StatusConnectedBg;
                lbStatusconnection.ForeColor = AppColors.StatusConnected;
                btnConnectIpadd.Text = "⏹  Ngắt kết nối";
                btnConnectIpadd.BackColor = AppColors.BrandRed;
                btnConnectIpadd.FlatAppearance.MouseOverBackColor = AppColors.AccentRedHover;
                btnConnectIpadd.Enabled = true;

                // Cập nhật thông tin nhận diện cân
                var info = _connectionManager.ConnectedScale;
                if (info != null)
                {
                    lbScaleModel.Text = $"Model: {info.Model}";
                    lbScaleSN.Text = $"S/N: {info.SerialNumber}";
                    lbScaleModel.Visible = true;
                    lbScaleSN.Visible = true;
                }
            }
            else if (isReconnecting)
            {
                // Lấy lý do lỗi nếu có (vd: sai thiết bị, timeout)
                string reason = _connectionManager.LastError ?? "Mất kết nối";
                bool isWrongDevice = reason.Contains("MT-SICS");

                lbStatusconnection.Text = isWrongDevice
                    ? "⚠  SAI THIẼT BỊ"
                    : "⬤  RECONNECTING...";
                lbStatusconnection.BackColor = AppColors.StatusWarningBg;
                lbStatusconnection.ForeColor = AppColors.StatusWarning;
                btnConnectIpadd.Text = "❌  Hủy Reconnect";
                btnConnectIpadd.BackColor = AppColors.BrandRed;
                btnConnectIpadd.Enabled = true;
                panel1.BackColor = AppColors.PanelIdle;
                lbLiveweight.ForeColor = AppColors.StatusWarning;
                System.Media.SystemSounds.Exclamation.Play();

                // Hiển thị balloon tip với lý do cụ thể
                trayIcon.Visible = true;
                trayIcon.ShowBalloonTip(4000,
                    isWrongDevice ? "Lỗi xác thực thiết bị" : "Mất kết nối",
                    reason,
                    isWrongDevice ? ToolTipIcon.Error : ToolTipIcon.Warning);
                trayIcon.Visible = false;
            }
            else
            {
                lbStatusconnection.Text = "⬤  DISCONNECTED";
                lbStatusconnection.BackColor = AppColors.StatusIdle;
                lbStatusconnection.ForeColor = AppColors.StatusIdleText;
                btnConnectIpadd.Text = "🔌  Kết nối (Connect)";
                btnConnectIpadd.BackColor = AppColors.BrandBlue;
                btnConnectIpadd.FlatAppearance.MouseOverBackColor = AppColors.BrandBlueDark;
                btnConnectIpadd.Enabled = true;
                panel1.BackColor = AppColors.PanelIdle;
                lbLiveweight.ForeColor = AppColors.TextMuted;

                lbScaleModel.Text = "Model: -";
                lbScaleSN.Text = "S/N: -";
            }
        }

        private void ConnectionManager_OnDataReceived(string rawData)
        {
            // [R-01] Parse trên Background Thread, enqueue vào ConcurrentQueue.
            // Không cần InvokeRequired/BeginInvoke — không trực tiếp chạm UI control.
            var scaleData = MtSicsParser.Parse(rawData);
            if (scaleData.HasValue)
                _dataQueue.Enqueue(scaleData.Value);
        }

        /// <summary>
        /// [R-01] Batch UI update chạy mỗi 100ms trên UI Thread.
        /// Dequeue toàn bộ items từ ConcurrentQueue và xử lý gộp thành một render pass duy nhất.
        /// </summary>
        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            var batch = new System.Collections.Generic.List<ScaleData>();
            while (_dataQueue.TryDequeue(out var item))
                batch.Add(item);

            if (batch.Count == 0) return;

            var latest = batch[^1];
            _lastScaleData = latest;

            // Cập nhật hiển thị từ item mới nhất trong batch
            if (latest.Status == ScaleStatus.Overload)
            {
                lbLiveweight.Text = "OVERLOAD";
                panel1.BackColor = AppColors.StatusWarningBg;
                lbLiveweight.ForeColor = AppColors.BrandRed;
            }
            else if (latest.Status == ScaleStatus.Underload)
            {
                lbLiveweight.Text = "UNDERLOAD";
                panel1.BackColor = AppColors.StatusWarningBg;
                lbLiveweight.ForeColor = AppColors.BrandRed;
            }
            else if (latest.Status == ScaleStatus.Invalid)
            {
                lbLiveweight.Text = "ERR / BUSY";
                panel1.BackColor = AppColors.StatusWarningBg;
                lbLiveweight.ForeColor = AppColors.StatusWarning;
            }
            else
            {
                lbLiveweight.Text = $"{latest.Weight:F4} {latest.Unit}";
                panel1.BackColor = latest.IsStable ? AppColors.PanelStable : AppColors.PanelUnstable;
                lbLiveweight.ForeColor = latest.IsStable ? AppColors.WeightStable : AppColors.WeightUnstable;
            }

            // Batch chart: thêm tất cả điểm trong một lượt, render một lần duy nhất
            bool hasChartData = false;
            foreach (var data in batch)
            {
                if (!data.IsError)
                {
                    UpdateChart(data, triggerRender: false);
                    hasChartData = true;
                }
            }
            if (hasChartData)
                _plotModel.InvalidatePlot(true);

            // Auto-Polling: xử lý tuần tự để giữ đúng state machine zero-detect
            if (chkAutoPolling.Checked)
            {
                foreach (var data in batch)
                    if (!data.IsError) ProcessAutoPolling(data);
            }
        }

        // ── Custom Tab Rendering ──────────────────────────────────
        private void TcDashboard_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tc) return;
            TabPage page = tc.TabPages[e.Index];
            bool isSelected = tc.SelectedIndex == e.Index;

            // ── Background ────────────────────────────────────────────
            Color bgColor = isSelected ? AppColors.Surface : AppColors.Background;
            using var bgBrush = new SolidBrush(bgColor);
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

            // ── Red accent underline for active tab ───────────────────
            if (isSelected)
            {
                using var accentBrush = new SolidBrush(AppColors.BrandRed);
                var accentRect = new Rectangle(e.Bounds.Left, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
                e.Graphics.FillRectangle(accentBrush, accentRect);
            }
            else
            {
                // Subtle bottom border for inactive tabs
                using var borderPen = new Pen(AppColors.Border);
                e.Graphics.DrawLine(borderPen,
                    e.Bounds.Left, e.Bounds.Bottom - 1,
                    e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            // ── Tab text ──────────────────────────────────────────────
            Color textColor = isSelected ? AppColors.BrandRed : AppColors.TextSecondary;
            float fontSize = isSelected ? 10.5F : 10F;
            var fontStyle = isSelected ? FontStyle.Bold : FontStyle.Regular;

            using var font = new Font("Segoe UI", fontSize, fontStyle);
            using var textBrush = new SolidBrush(textColor);
            // [FIX B7] using để tránh GDI+ resource leak
            using var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            // Offset text up slightly to avoid overlapping accent bar
            var textRect = new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height - 3);
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(page.Text, font, textBrush, textRect, sf);
        }

        private void InitializeChart()
        {
            _plotModel = new PlotModel
            {
                Background = OxyColor.FromRgb(255, 255, 255),
                PlotAreaBackground = OxyColor.FromRgb(248, 250, 252),
                TextColor = OxyColor.FromRgb(30, 41, 59),
                PlotAreaBorderColor = OxyColor.FromRgb(226, 232, 240),
                TitleFontSize = 14
            };

            _plotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss",
                Title = "Thời gian",
                TextColor = OxyColor.FromRgb(100, 116, 139),
                TicklineColor = OxyColor.FromRgb(226, 232, 240),
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(120, 226, 232, 240),
                IntervalType = DateTimeIntervalType.Seconds
            });

            _plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Khối lượng",
                TextColor = OxyColor.FromRgb(100, 116, 139),
                TicklineColor = OxyColor.FromRgb(226, 232, 240),
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(120, 226, 232, 240)
            });

            _weightSeries = new LineSeries
            {
                Title = "Khối lượng",
                Color = OxyColor.FromRgb(0, 159, 227),  // BrandBlue
                StrokeThickness = 2.5,
                MarkerType = MarkerType.None
            };

            _plotModel.Series.Add(_weightSeries);
            plotViewLiveChart.Model = _plotModel;
        }

        private void UpdateChart(ScaleData data, bool triggerRender = true)
        {
            if (data.IsError) return; // Không vẽ khi quá tải/dưới tải

            double t = DateTimeAxis.ToDouble(DateTime.Now);
            _weightSeries.Points.Add(new DataPoint(t, (double)data.Weight));

            if (_weightSeries.Points.Count > MaxChartPoints)
                _weightSeries.Points.RemoveAt(0);

            // Track session min/max (stable readings only)
            if (data.IsStable && data.Weight > 0.05m)
            {
                _sessionMin = _sessionMin.HasValue ? Math.Min(_sessionMin.Value, data.Weight) : data.Weight;
                _sessionMax = _sessionMax.HasValue ? Math.Max(_sessionMax.Value, data.Weight) : data.Weight;

                // [FIX B8] Dùng unit thực tế từ scale data thay vì hardcoded "g"
                string unit = _lastScaleData?.Unit ?? "g";
                lbStatMinValue.Text = $"{_sessionMin:F4} {unit}";
                lbStatMaxValue.Text = $"{_sessionMax:F4} {unit}";
            }

            // [R-01] Chỉ gọi InvalidatePlot khi được yêu cầu — batch mode dùng false
            if (triggerRender)
                _plotModel.InvalidatePlot(true);
        }

        private void UpdateStats()
        {
            try
            {
                int todayCount = _repository.GetTodayCount();
                decimal batchTotal = _repository.GetBatchTotal(tboBatch.Text.Trim());

                lbStatTodayValue.Text = todayCount.ToString();
                lbStatBatchValue.Text = $"{batchTotal:F4} g";
            }
            catch { /* stats are non-critical, suppress silently */ }
        }

        private void ProcessAutoPolling(ScaleData data)
        {
            // Nếu khối lượng <= ZeroThreshold (kể cả chưa ổn định), ta coi như cân đã được làm trống và sẵn sàng cho lần cân tiếp theo.
            if (data.Weight <= ZeroThreshold)
            {
                _autoPollingState = AutoPollingState.ReadyToWeigh;
                return;
            }

            // Nếu đang ở trạng thái sẵn sàng, khối lượng lớn hơn ZeroThreshold và đã ổn định -> Chốt số
            if (_autoPollingState == AutoPollingState.ReadyToWeigh && data.IsStable)
            {
                SaveCurrentWeight(true);
                _autoPollingState = AutoPollingState.WeightCaptured;
            }
        }

        private void btnPolling_Click(object? sender, EventArgs e)
        {
            SaveCurrentWeight(false);
        }

        private void Tbo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Chặn tiếng bíp mặc định của Windows

                if (sender == tboNat)
                {
                    tboBatch.Focus();
                }
                else if (sender == tboBatch)
                {
                    tboSamplename.Focus();
                }
                else if (sender == tboSamplename)
                {
                    tboLocation.Focus();
                }
                else if (sender == tboLocation)
                {
                    tboTester.Focus();
                }
                else if (sender == tboTester)
                {
                    tboNat.Focus();
                }
            }
        }

        private void InitializeSettingsTab()
        {
            _tpSettings = new TabPage("⚙️ Cài đặt");
            _tpSettings.BackColor = Color.FromArgb(244, 246, 249);
            _tpSettings.Padding = new Padding(20);

            // GroupBox Backup & Restore
            var gbBackup = new GroupBox
            {
                Text = "An toàn Dữ liệu (Backup & Restore)",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(227, 6, 19),
                Location = new Point(20, 20),
                Size = new Size(600, 310),
                BackColor = Color.White
            };

            // --- Row 1: Manual Backup + Import ---
            var btnManualBackup = new Button
            {
                Text = "💾 Sao lưu thủ công",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(30, 40),
                Size = new Size(260, 40)
            };
            btnManualBackup.FlatAppearance.BorderSize = 0;
            btnManualBackup.Click += (s, e) => PerformManualBackup();

            var btnImportDB = new Button
            {
                Text = "📂 Phục hồi dữ liệu (Import)",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(310, 40),
                Size = new Size(260, 40)
            };
            btnImportDB.FlatAppearance.BorderSize = 0;
            btnImportDB.Click += async (s, e) => await ImportDatabaseAsync();

            // --- Row 2: Auto backup interval (ComboBox) ---
            var lbBackupInterval = new Label
            {
                Text = "Chu kỳ sao lưu tự động:",
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(30, 100),
                AutoSize = true
            };

            // Preset options: label → hours
            var intervalOptions = new (string Label, int Hours)[]
            {
                ("3 giờ",   3),
                ("24 giờ",  24),
                ("3 ngày",  72),
                ("7 ngày",  168),
                ("30 ngày", 720)
            };

            var cboInterval = new ComboBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(220, 97),
                Size = new Size(150, 32),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var opt in intervalOptions)
                cboInterval.Items.Add(opt.Label);

            // Pre-select current setting
            int currentHours = AppConfig.Load().AutoBackupIntervalHours;
            int selectedIndex = 0;
            for (int i = 0; i < intervalOptions.Length; i++)
            {
                if (intervalOptions[i].Hours == currentHours) { selectedIndex = i; break; }
            }
            cboInterval.SelectedIndex = selectedIndex;

            var btnSaveBackupInterval = new Button
            {
                Text = "Lưu chu kỳ",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 159, 227),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(382, 97),
                Size = new Size(110, 32)
            };
            btnSaveBackupInterval.FlatAppearance.BorderSize = 0;
            btnSaveBackupInterval.Click += (s, e) =>
            {
                int idx = cboInterval.SelectedIndex;
                if (idx < 0) return;
                int hours = intervalOptions[idx].Hours;

                var config = AppConfig.Load();
                config.AutoBackupIntervalHours = hours;
                config.Save();

                if (_autoBackupTimer != null)
                    _autoBackupTimer.Interval = hours * 60 * 60 * 1000;

                MessageBox.Show(
                    $"Đã lưu chu kỳ tự động sao lưu: {intervalOptions[idx].Label}",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // --- Row 3: Dropbox folder path (pushed down to avoid overlap with interval row) ---
            var lbDropbox = new Label
            {
                Text = "📦 Thư mục Dropbox (để sao lưu tự động):",
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(30, 158),
                AutoSize = true
            };

            var tboDropboxPath = new TextBox
            {
                Text = AppConfig.Load().DropboxFolderPath,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(30, 185),
                Size = new Size(440, 28),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Để trống = lưu vào thư mục Backups/ mặc định"
            };

            var btnBrowseDropbox = new Button
            {
                Text = "📁 Chọn...",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(480, 184),
                Size = new Size(90, 30)
            };
            btnBrowseDropbox.FlatAppearance.BorderSize = 0;
            btnBrowseDropbox.Click += (s, e) =>
            {
                using var dlg = new FolderBrowserDialog
                {
                    Description = "Chọn thư mục Dropbox để lưu file backup",
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = true,
                    SelectedPath = tboDropboxPath.Text
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                    tboDropboxPath.Text = dlg.SelectedPath;
            };

            var btnSaveDropbox = new Button
            {
                Text = "💾 Lưu đường dẫn",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(30, 228),
                Size = new Size(160, 32)
            };
            btnSaveDropbox.FlatAppearance.BorderSize = 0;
            btnSaveDropbox.Click += (s, e) =>
            {
                var config = AppConfig.Load();
                config.DropboxFolderPath = tboDropboxPath.Text.Trim();
                config.Save();
                MessageBox.Show(
                    "Đã lưu đường dẫn Dropbox!\nCác bản sao lưu tự động tiếp theo sẽ được lưu vào thư mục này.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var lbBackupInfo = new Label
            {
                Text = "Dropbox app sẽ tự đồng bộ file backup lên mây sau khi lưu.",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(200, 235),
                AutoSize = true
            };

            gbBackup.Controls.Add(btnManualBackup);
            gbBackup.Controls.Add(btnImportDB);
            gbBackup.Controls.Add(lbBackupInterval);
            gbBackup.Controls.Add(cboInterval);
            gbBackup.Controls.Add(btnSaveBackupInterval);
            gbBackup.Controls.Add(lbDropbox);
            gbBackup.Controls.Add(tboDropboxPath);
            gbBackup.Controls.Add(btnBrowseDropbox);
            gbBackup.Controls.Add(btnSaveDropbox);
            gbBackup.Controls.Add(lbBackupInfo);

            // GroupBox API (Cấu hình Hệ thống)
            var gbApi = new GroupBox
            {
                Text = "Cấu hình Hệ thống",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(227, 6, 19),
                Location = new Point(20, 350),
                Size = new Size(600, 240),
                BackColor = Color.White
            };

            var lbApiUrl = new Label
            {
                Text = "Địa chỉ Máy chủ Trung tâm (API URL):",
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(30, 30),
                AutoSize = true
            };

            var tboApiUrl = new TextBox
            {
                Text = AppConfig.Load().ApiServerUrl,
                Font = new Font("Segoe UI", 11F),
                Location = new Point(30, 58),
                Size = new Size(540, 32),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lbDeviceId = new Label
            {
                Text = "Tên máy trạm (Device ID):",
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(30, 100),
                AutoSize = true
            };

            var tboDeviceId = new TextBox
            {
                Text = AppConfig.Load().DeviceId,
                Font = new Font("Segoe UI", 11F),
                Location = new Point(30, 128),
                Size = new Size(310, 32),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblConnStatus = new Label
            {
                Text = "Chưa kiểm tra",
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(360, 133),
                AutoSize = true
            };

            var btnTestConn = new Button
            {
                Text = "🔌 Kiểm tra kết nối",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(30, 178),
                Size = new Size(180, 36)
            };
            btnTestConn.FlatAppearance.BorderSize = 0;
            btnTestConn.Click += async (s, e) =>
            {
                string urlToTest = tboApiUrl.Text.Trim();
                if (string.IsNullOrWhiteSpace(urlToTest))
                {
                    lblConnStatus.Text = "⚠️ Chưa nhập URL";
                    lblConnStatus.ForeColor = Color.Orange;
                    return;
                }

                btnTestConn.Enabled = false;
                lblConnStatus.Text = "Đang kiểm tra...";
                lblConnStatus.ForeColor = Color.Gray;

                // Dùng static method — test URL mới ngay mà không cần restart App
                bool ok = await Test.Services.NetworkService.CheckUrlAsync(urlToTest);

                lblConnStatus.Text = ok ? "✅ Kết nối thành công" : "❌ Không kết nối được";
                lblConnStatus.ForeColor = ok ? Color.FromArgb(34, 197, 94) : Color.Red;

                if (!ok)
                    LogError($"Kiểm tra kết nối thất bại: {urlToTest}/api/health");

                btnTestConn.Enabled = true;
            };

            var btnSaveApi = new Button
            {
                Text = "Lưu cấu hình",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 159, 227),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(230, 178),
                Size = new Size(150, 36)
            };
            btnSaveApi.FlatAppearance.BorderSize = 0;
            btnSaveApi.Click += (s, e) =>
            {
                var config = AppConfig.Load();
                config.ApiServerUrl = tboApiUrl.Text.Trim();
                config.DeviceId = string.IsNullOrWhiteSpace(tboDeviceId.Text)
                    ? System.Environment.MachineName
                    : tboDeviceId.Text.Trim();
                config.Save();

                MessageBox.Show(
                    "Đã lưu cấu hình thành công!\nỨng dụng sẽ tự động khởi động lại để áp dụng cài đặt mới.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Application.Restart();
                Environment.Exit(0);
            };

            gbApi.Controls.Add(lbApiUrl);
            gbApi.Controls.Add(tboApiUrl);
            gbApi.Controls.Add(lbDeviceId);
            gbApi.Controls.Add(tboDeviceId);
            gbApi.Controls.Add(lblConnStatus);
            gbApi.Controls.Add(btnTestConn);
            gbApi.Controls.Add(btnSaveApi);

            // ── GroupBox Đổi mật khẩu Admin ─────────────────────────────────
            var gbPassword = new GroupBox
            {
                Text = "🔐 Đổi mật khẩu Admin (Cho App Cân)",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(227, 6, 19),
                Location = new Point(20, 610),
                Size = new Size(600, 220),
                BackColor = Color.White
            };

            // Helper tạo password row (label + textbox + toggle show)
            TextBox MakePasswordBox(string labelText, int y, out CheckBox chkShow)
            {
                var lbl = new Label { Text = labelText, Font = new Font("Segoe UI", 10F), Location = new Point(30, y), AutoSize = true, ForeColor = Color.FromArgb(30, 41, 59) };
                var tbo = new TextBox { Font = new Font("Segoe UI", 11F), Location = new Point(30, y + 24), Size = new Size(430, 30), PasswordChar = '•', BorderStyle = BorderStyle.FixedSingle };
                var chk = new CheckBox { Text = "Hiện", Font = new Font("Segoe UI", 9F), Location = new Point(470, y + 27), AutoSize = true };
                chk.CheckedChanged += (s, e) => tbo.PasswordChar = chk.Checked ? '\0' : '•';
                gbPassword.Controls.Add(lbl);
                gbPassword.Controls.Add(tbo);
                gbPassword.Controls.Add(chk);
                chkShow = chk;
                return tbo;
            }

            var tboOldPass = MakePasswordBox("Mật khẩu hiện tại:", 28, out _);
            var tboNewPass = MakePasswordBox("Mật khẩu mới:", 90, out _);
            var tboConfPass = MakePasswordBox("Xác nhận mật khẩu mới:", 152, out _);

            var btnChangePass = new Button
            {
                Text = "Đổi mật khẩu",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(30, 185),
                Size = new Size(150, 34)
            };
            btnChangePass.FlatAppearance.BorderSize = 0;
            btnChangePass.Click += (s, e) =>
            {
                string oldPass = tboOldPass.Text;
                string newPass = tboNewPass.Text;
                string confPass = tboConfPass.Text;

                var config = AppConfig.Load();
                if (oldPass != config.AdminPassword)
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(newPass) || newPass.Length < 6)
                {
                    MessageBox.Show("Mật khẩu mới phải có ít nhất 6 ký tự!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (newPass != confPass)
                {
                    MessageBox.Show("Mật khẩu mới và xác nhận không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                config.AdminPassword = newPass;
                config.Save();
                tboOldPass.Clear(); tboNewPass.Clear(); tboConfPass.Clear();
                MessageBox.Show("Đã đổi mật khẩu Admin thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            gbPassword.Controls.Add(btnChangePass);

            // ── GroupBox API Key ──────────────────────────────────────────
            var gbApiKey = new GroupBox
            {
                Text = "🔑 Mã khoá kết nối máy chủ (API Key)",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(227, 6, 19),
                Location = new Point(20, 850),
                Size = new Size(600, 160),
                BackColor = Color.White
            };

            var lblApiKey = new Label { Text = "API Key:", Font = new Font("Segoe UI", 10F), Location = new Point(30, 40), AutoSize = true, ForeColor = Color.FromArgb(30, 41, 59) };
            var tboApiKey = new TextBox { Font = new Font("Segoe UI", 11F), Location = new Point(30, 65), Size = new Size(430, 30), PasswordChar = ' ', BorderStyle = BorderStyle.FixedSingle };
            var chkShowApiKey = new CheckBox { Text = "Hiện", Font = new Font("Segoe UI", 9F), Location = new Point(470, 68), AutoSize = true };
            chkShowApiKey.CheckedChanged += (s, e) => tboApiKey.PasswordChar = chkShowApiKey.Checked ? '\0' : ' ';
            tboApiKey.Text = AppConfig.Load().ApiKey;

            var btnSaveApiKey = new Button
            {
                Text = "Lưu API Key",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(30, 110),
                Size = new Size(150, 34)
            };
            btnSaveApiKey.FlatAppearance.BorderSize = 0;
            btnSaveApiKey.Click += (s, e) =>
            {
                var config = AppConfig.Load();
                config.ApiKey = tboApiKey.Text.Trim();
                config.Save();
                MessageBox.Show("Đã lưu API Key thành công!\nỨng dụng sẽ tự động khởi động lại để áp dụng cài đặt mới.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Restart();
                Environment.Exit(0);
            };

            gbApiKey.Controls.Add(lblApiKey);
            gbApiKey.Controls.Add(tboApiKey);
            gbApiKey.Controls.Add(chkShowApiKey);
            gbApiKey.Controls.Add(btnSaveApiKey);

            // ── GroupBox Error Log ───────────────────────────────────────────
            var gbLog = new GroupBox
            {
                Text = "📋 Nhật ký lỗi (Error Log)",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(227, 6, 19),
                Location = new Point(20, 850),
                Size = new Size(600, 250),
                BackColor = Color.White
            };

            _txtErrorLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.FromArgb(134, 239, 172),
                Location = new Point(10, 35),
                Size = new Size(578, 160),
                BorderStyle = BorderStyle.None
            };
            // Điền lại các lỗi đã có từ trước
            if (_errorLog.Count > 0)
                _txtErrorLog.Text = string.Join(Environment.NewLine, _errorLog);

            var btnClearLog = new Button
            {
                Text = "🗑 Xóa log",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(10, 205),
                Size = new Size(100, 28)
            };
            btnClearLog.FlatAppearance.BorderSize = 0;
            btnClearLog.Click += (s, e) => { _errorLog.Clear(); _txtErrorLog.Clear(); };

            var btnCopyLog = new Button
            {
                Text = "📋 Copy log",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(120, 205),
                Size = new Size(110, 28)
            };
            btnCopyLog.FlatAppearance.BorderSize = 0;
            btnCopyLog.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(_txtErrorLog.Text))
                    Clipboard.SetText(_txtErrorLog.Text);
            };

            gbLog.Controls.Add(_txtErrorLog);
            gbLog.Controls.Add(btnClearLog);
            gbLog.Controls.Add(btnCopyLog);

            // Cho phép cuộn trong tab Cài đặt (nội dung dài)
            _tpSettings.AutoScroll = true;

            // Nút tạo dữ liệu ảo để test
            var btnFakeData = new Button
            {
                Text = "Tạo dữ liệu Cân Ảo (Test)",
                Location = new Point(gbLog.Left, gbLog.Bottom + 10),
                Size = new Size(gbLog.Width, 40),
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnFakeData.FlatAppearance.BorderSize = 0;
            btnFakeData.Click += btnTestFakeData_Click;

            _tpSettings.Controls.Add(gbBackup);
            _tpSettings.Controls.Add(gbApi);
            _tpSettings.Controls.Add(gbPassword);
            _tpSettings.Controls.Add(gbApiKey);
            _tpSettings.Controls.Add(gbLog);
            _tpSettings.Controls.Add(btnFakeData);

            // Removed tcDashboard.TabPages.Add(_tpSettings) here since UpdateRoleUI manages it.
        }

        private void SaveCurrentWeight(bool isAuto)
        {
            if (!_connectionManager.IsConnected)
            {
                if (!isAuto) MessageBox.Show("Cân chưa được kết nối.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_lastScaleData.HasValue)
            {
                if (!isAuto) MessageBox.Show("Chưa nhận được dữ liệu từ cân.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!isAuto && !_lastScaleData.Value.IsStable)
            {
                var result = MessageBox.Show("Cân chưa ổn định. Bạn có chắc chắn muốn chốt số liệu hiện tại?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No) return;
            }

            try
            {
                var record = new ScaleRecord
                {
                    Timestamp = DateTime.Now,
                    Weight = _lastScaleData.Value.Weight,
                    Unit = _lastScaleData.Value.Unit,
                    NatCode = tboNat.Text.Trim(),
                    Batch = tboBatch.Text.Trim(),
                    SampleName = tboSamplename.Text.Trim(),
                    Location = tboLocation.Text.Trim(),
                    Tester = tboTester.Text.Trim()
                };

                // Lưu dữ liệu qua facade đồng bộ mạng
                _ = _dbService.SaveRecordAsync(record);
                // Vẫn lưu local qua _repository.Insert cho UI grid
                _repository.Insert(record);

                // Audio feedback
                System.Media.SystemSounds.Beep.Play();

                if (!isAuto)
                {
                    MessageBox.Show("Đã lưu số liệu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadDataToGrid(); // UpdateStats() is called inside LoadDataToGrid
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- Nút Ẩn: Dùng để Test đẩy dữ liệu mà không cần Cân thật ---
        private void btnTestFakeData_Click(object? sender, EventArgs e)
        {
            var r = new Random();
            var records = new System.Collections.Generic.List<ScaleRecord>();
            for (int i = 1; i <= 5; i++)
            {
                var record = new ScaleRecord
                {
                    Timestamp = DateTime.Now.AddMinutes(-i * 5),
                    Weight = (decimal)r.Next(100, 5000) / 10m,
                    Unit = "g",
                    NatCode = tboNat.Text.Trim() == "" ? $"NAT-{r.Next(1000, 9999)}" : tboNat.Text.Trim(),
                    Batch = tboBatch.Text.Trim() == "" ? $"BATCH-{DateTime.Now:MMdd}-{i}" : tboBatch.Text.Trim(),
                    SampleName = $"Mẫu kiểm thử {i}",
                    Location = "Bàn Test 1",
                    Tester = "Admin Test"
                };
                records.Add(record);
                _ = _dbService.SaveRecordAsync(record); // Sync to server if possible
            }
            
            _repository.InsertBatch(records);

            System.Media.SystemSounds.Beep.Play();
            LoadDataToGrid();
            MessageBox.Show($"Đã tạo và lưu thành công 5 dòng dữ liệu mẫu!", "Tạo dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadDataToGrid()
        {
            try
            {
                _allRecords = _repository.GetAll();
                var dt = _allRecords.ToDataTable();
                _bindingSource.DataSource = dt;
                
                if (dgvWeightsheet.DataSource == null)
                {
                    dgvWeightsheet.DataSource = _bindingSource;
                }
                
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tboSearch_TextChanged(object? sender, EventArgs e)
        {
            string keyword = tboSearch.Text.Trim().Replace("'", "''");
            if (string.IsNullOrEmpty(keyword))
            {
                _bindingSource.Filter = null;
            }
            else
            {
                _bindingSource.Filter = $"Convert(Id, 'System.String') LIKE '%{keyword}%' OR NatCode LIKE '%{keyword}%' OR Batch LIKE '%{keyword}%' OR SampleName LIKE '%{keyword}%' OR Location LIKE '%{keyword}%' OR Unit LIKE '%{keyword}%'";
            }
        }

        private void btnExportdata_Click(object? sender, EventArgs e)
        {
            try
            {
                var data = new System.Collections.Generic.List<ScaleRecord>();
                foreach (DataGridViewRow row in dgvWeightsheet.Rows)
                {
                    if (row.DataBoundItem is DataRowView drv)
                    {
                        data.Add(new ScaleRecord
                        {
                            Id = Convert.ToInt64(drv["Id"]),
                            Timestamp = Convert.ToDateTime(drv["Timestamp"]),
                            Weight = Convert.ToDecimal(drv["Weight"]),
                            Unit = drv["Unit"]?.ToString() ?? "g",
                            NatCode = drv["NatCode"] == DBNull.Value ? null : drv["NatCode"].ToString(),
                            Batch = drv["Batch"] == DBNull.Value ? null : drv["Batch"].ToString(),
                            SampleName = drv["SampleName"] == DBNull.Value ? null : drv["SampleName"].ToString(),
                            Location = drv["Location"] == DBNull.Value ? null : drv["Location"].ToString(),
                            Tester = drv["Tester"] == DBNull.Value ? null : drv["Tester"].ToString(),
                            IsSynced = Convert.ToBoolean(drv["IsSynced"]),
                            IsSelected = drv["IsSelected"] != DBNull.Value && Convert.ToBoolean(drv["IsSelected"])
                        });
                    }
                }

                if (data.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dlg = new SaveFileDialog())
                {
                    dlg.Title = "Xuất dữ liệu cân";
                    dlg.Filter = "Excel files (*.xlsx)|*.xlsx|CSV files (*.csv)|*.csv";
                    dlg.FileName = $"ScaleData_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string ext = System.IO.Path.GetExtension(dlg.FileName).ToLower();
                        if (ext == ".xlsx")
                            _excelExportService.Export(dlg.FileName, data);
                        else
                            _csvExportService.Export(dlg.FileName, data);

                        MessageBox.Show("Xuất dữ liệu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImportData_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Title = "Nhập dữ liệu cân";
                    dlg.Filter = "Excel & CSV files (*.xlsx;*.csv)|*.xlsx;*.csv|Excel files (*.xlsx)|*.xlsx|CSV files (*.csv)|*.csv";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string ext = System.IO.Path.GetExtension(dlg.FileName).ToLower();
                        System.Collections.Generic.List<ScaleRecord> records;

                        if (ext == ".xlsx")
                            records = _excelImportService.Import(dlg.FileName);
                        else
                            records = _csvImportService.Import(dlg.FileName);

                        if (records.Count > 0)
                        {
                            _repository.InsertBatch(records);
                            LoadDataToGrid();
                            MessageBox.Show($"Đã nhập thành công {records.Count} dòng dữ liệu!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy dữ liệu hợp lệ trong file.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nhập dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            trayIcon.Visible = false;
        }

        private async System.Threading.Tasks.Task PerformAutoBackupAsync()
        {
            try
            {
                var config = AppConfig.Load();

                // Ưu tiên lưu vào thư mục Dropbox nếu đã cấu hình, ngược lại dùng Backups/ mặc định
                string dropboxPath = config.DropboxFolderPath?.Trim() ?? "";
                string backupDir = (!string.IsNullOrEmpty(dropboxPath) && System.IO.Directory.Exists(dropboxPath))
                    ? System.IO.Path.Combine(dropboxPath, "ScaleData_Backups")
                    : System.IO.Path.Combine(Application.StartupPath, "Backups");

                if (!System.IO.Directory.Exists(backupDir))
                    System.IO.Directory.CreateDirectory(backupDir);

                string fileName = $"ScaleData_AutoBackup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                string destPath = System.IO.Path.Combine(backupDir, fileName);

                await DatabaseHelper.BackupDatabaseAsync(destPath);

                // Dọn file cũ: giữ tương đương 5 chu kỳ gần nhất
                int keepDays = Math.Max(1, (config.AutoBackupIntervalHours * 5) / 24);
                var oldFiles = System.IO.Directory.GetFiles(backupDir, "ScaleData_AutoBackup_*.db")
                    .Select(f => new System.IO.FileInfo(f))
                    .Where(f => f.CreationTime < DateTime.Now.AddDays(-keepDays));
                foreach (var file in oldFiles)
                {
                    try { file.Delete(); } catch { }
                }

                System.Diagnostics.Debug.WriteLine($"[AutoBackup] Thành công: {fileName} → {backupDir}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AutoBackup] Lỗi: {ex.Message}");
            }
        }

        private async void PerformManualBackup()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Chọn nơi lưu bản sao lưu (Backup)";
                dlg.Filter = "SQLite Database (*.db)|*.db";
                dlg.FileName = $"ScaleData_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await DatabaseHelper.BackupDatabaseAsync(dlg.FileName);
                        MessageBox.Show("Đã sao lưu cơ sở dữ liệu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi sao lưu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task ImportDatabaseAsync()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Chọn file dữ liệu để phục hồi (Import)";
                dlg.Filter = "SQLite Database (*.db)|*.db";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var confirmResult = MessageBox.Show(
                        "CẢNH BÁO: Việc phục hồi dữ liệu sẽ XÓA TOÀN BỘ dữ liệu hiện tại và thay thế bằng dữ liệu từ file bạn chọn.\n\nBạn có chắc chắn muốn tiếp tục?",
                        "Xác nhận Phục hồi",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            // Đảm bảo không ai đang ghi vào DB
                            await DatabaseHelper.DbAccessLock.WaitAsync();

                            // Dừng các timer và connection
                            _uiTimer?.Stop();
                            _autoBackupTimer?.Stop();
                            if (_connectionManager.IsConnected) _connectionManager.Disconnect();

                            // Copy đè file
                            string currentDbPath = System.IO.Path.Combine(Application.StartupPath, "ScaleData.db");
                            System.IO.File.Copy(dlg.FileName, currentDbPath, true);

                            MessageBox.Show("Đã phục hồi dữ liệu thành công! Ứng dụng sẽ tải lại dữ liệu.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi phục hồi dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            DatabaseHelper.DbAccessLock.Release();

                            // Khởi động lại
                            _uiTimer?.Start();
                            _autoBackupTimer?.Start();
                            LoadDataToGrid();
                            UpdateStats();
                        }
                    }
                }
            }
        }

        private async void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                int unsyncedCount = _repository.GetUnsyncedCount();
                string msg = unsyncedCount > 0
                    ? $"CẢNH BÁO: Còn {unsyncedCount} dòng dữ liệu chưa được đồng bộ lên máy chủ.\nNếu thoát, dữ liệu sẽ lưu trữ nội bộ và tự động gửi vào lần mở ứng dụng tiếp theo.\n\nBạn có chắc chắn muốn thoát?"
                    : "Bạn có chắc chắn muốn thoát ứng dụng không?\nHệ thống sẽ tự động tạo một bản sao lưu dữ liệu trước khi đóng.";

                var confirmResult = MessageBox.Show(msg, "Xác nhận thoát", MessageBoxButtons.YesNo, unsyncedCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Question);
                if (confirmResult == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                e.Cancel = true;
                this.FormClosing -= Form1_FormClosing; // Prevent recursion

                try
                {
                    await PerformAutoBackupAsync();
                }
                finally
                {
                    _autoBackupTimer?.Stop();
                    _autoBackupTimer?.Dispose();
                    _uiTimer?.Stop();
                    _uiTimer?.Dispose();
                    _connectionManager?.Dispose();
                    trayIcon.Visible = false;
                    trayIcon.Dispose();

                    this.Close();
                }
            }
        }

        #region Auto Scaling Logic

        private void Form1_Load(object? sender, EventArgs e)
        {
            _originalFormSize = this.ClientSize;
            SaveOriginalBounds(this);
            this.Resize += Form1_Resize;

            // Đảm bảo DataGridView phóng to columns
            dgvWeightsheet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Create Login/Logout button
            _btnLogin = new Button
            {
                Text = "Đăng nhập Admin",
                Location = new Point(this.ClientSize.Width - 160, 10),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(0, 159, 227),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(_btnLogin);
            _btnLogin.BringToFront();

            UpdateRoleUI();
            // Server status label — hiển thị ở góc dưới bên trái, luôn thấy dù là User hay Admin
            _lblServerStatus = new Label
            {
                Text = "🔴 Server: Chưa kết nối",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Red,
                Location = new Point(8, this.ClientSize.Height - 24),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            this.Controls.Add(_lblServerStatus);
            _lblServerStatus.BringToFront();

            // Ping server định kỳ mỗi 30 giây để cập nhật trạng thái
            _serverPingTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _serverPingTimer.Tick += async (s, e) => await PingServerStatusAsync();
            _serverPingTimer.Start();

            // Ping ngay lúc khởi động
            _ = PingServerStatusAsync();
        }

        private void InitializeDataGridView()
        {
            // Set grid features
            dgvWeightsheet.MultiSelect = true;
            dgvWeightsheet.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvWeightsheet.ReadOnly = false; // Enable inline edit

            // Configure columns after data binding in LoadDataToGrid
            dgvWeightsheet.DataBindingComplete += (s, e) =>
            {
                foreach (DataGridViewColumn col in dgvWeightsheet.Columns)
                {
                    if (col.Name == "Id" || col.Name == "Timestamp" || col.Name == "Weight" || col.Name == "Unit" || col.Name == "IsSynced" || col.Name == "IsSelected")
                    {
                        col.ReadOnly = true;
                        col.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                    }
                    else
                    {
                        // Enable AutoFilter for editable text columns
                        if (!(col.HeaderCell is DataGridViewAutoFilterColumnHeaderCell))
                        {
                            col.HeaderCell = new DataGridViewAutoFilterColumnHeaderCell(col.HeaderCell);
                        }
                    }
                }

                if (dgvWeightsheet.Columns.Contains("IsSelected"))
                {
                    var chkCol = dgvWeightsheet.Columns["IsSelected"];
                    chkCol.HeaderText = "Chọn";
                    chkCol.DisplayIndex = 0;
                    chkCol.Width = 50;
                    chkCol.ReadOnly = false;
                }
            };

            // Bulk Delete context menu
            var ctxMenu = new ContextMenuStrip();
            var delItem = new ToolStripMenuItem("Xóa các dòng đã chọn", null, (s, e) =>
            {
                var idsToDelete = new System.Collections.Generic.List<long>();
                foreach (DataGridViewRow row in dgvWeightsheet.Rows)
                {
                    if (row.DataBoundItem is DataRowView drv)
                    {
                        var isSelected = drv["IsSelected"] != DBNull.Value && Convert.ToBoolean(drv["IsSelected"]);
                        if (isSelected || row.Selected)
                        {
                            var id = Convert.ToInt64(drv["Id"]);
                            if (!idsToDelete.Contains(id))
                            {
                                idsToDelete.Add(id);
                            }
                        }
                    }
                }

                if (idsToDelete.Count > 0)
                {
                    var res = MessageBox.Show($"Bạn có chắc muốn xóa {idsToDelete.Count} dòng dữ liệu này khỏi máy tính?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        _repository.DeleteBatch(idsToDelete);
                        LoadDataToGrid(); // Refresh
                        MessageBox.Show("Đã xóa dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một dòng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
            ctxMenu.Items.Add(delItem);
            dgvWeightsheet.ContextMenuStrip = ctxMenu;

            // Add explicit Bulk Delete button on UI
            var btnBulkDelete = new Button
            {
                Text = "🗑 Xóa mục đã chọn",
                Size = new Size(180, 28),
                Location = new Point(17, 80),
                BackColor = Color.White,
                ForeColor = Color.Red,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnBulkDelete.FlatAppearance.BorderColor = Color.Red;
            btnBulkDelete.Click += (s, e) => delItem.PerformClick();
            tpDatasheet.Controls.Add(btnBulkDelete);

            // Auto-save inline edits
            dgvWeightsheet.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var row = dgvWeightsheet.Rows[e.RowIndex];
                    if (row.DataBoundItem is DataRowView drv)
                    {
                        var record = new ScaleRecord
                        {
                            Id = Convert.ToInt64(drv["Id"]),
                            Timestamp = Convert.ToDateTime(drv["Timestamp"]),
                            Weight = Convert.ToDecimal(drv["Weight"]),
                            Unit = drv["Unit"]?.ToString() ?? "g",
                            NatCode = drv["NatCode"] == DBNull.Value ? null : drv["NatCode"].ToString(),
                            Batch = drv["Batch"] == DBNull.Value ? null : drv["Batch"].ToString(),
                            SampleName = drv["SampleName"] == DBNull.Value ? null : drv["SampleName"].ToString(),
                            Location = drv["Location"] == DBNull.Value ? null : drv["Location"].ToString(),
                            Tester = drv["Tester"] == DBNull.Value ? null : drv["Tester"].ToString(),
                            IsSynced = Convert.ToBoolean(drv["IsSynced"]),
                            IsSelected = drv["IsSelected"] != DBNull.Value && Convert.ToBoolean(drv["IsSelected"])
                        };
                        _repository.Update(record);
                    }
                }
            };

            // Adjust DataGridView location
            dgvWeightsheet.Location = new Point(17, 125);
            dgvWeightsheet.Size = new Size(1358, 650);
        }

        private async System.Threading.Tasks.Task PingServerStatusAsync()
        {
            bool ok = await Test.Services.NetworkService.Instance.CheckConnectionAsync();
            if (_lblServerStatus.IsDisposed) return;
            _lblServerStatus.Text = ok ? "🟢 Server: Đã kết nối" : "🔴 Server: Chưa kết nối";
            _lblServerStatus.ForeColor = ok ? Color.FromArgb(22, 163, 74) : Color.Red;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (_isAdmin)
            {
                // Logout
                _isAdmin = false;
                UpdateRoleUI();
                MessageBox.Show("Đã đăng xuất quyền Admin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Login
                using (var frm = new LoginForm())
                {
                    if (frm.ShowDialog() == DialogResult.OK && frm.IsAuthenticated)
                    {
                        _isAdmin = true;
                        UpdateRoleUI();
                        MessageBox.Show("Đăng nhập Admin thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void UpdateRoleUI()
        {
            if (_isAdmin)
            {
                _btnLogin.Text = "Đăng xuất";
                _btnLogin.BackColor = Color.Gray;
                btnImportData.Enabled = true;
                btnImportData.BackColor = Color.FromArgb(46, 204, 113); // Green

                if (_tpSettings != null && !tcDashboard.TabPages.Contains(_tpSettings))
                {
                    tcDashboard.TabPages.Add(_tpSettings);
                }
            }
            else
            {
                _btnLogin.Text = "Đăng nhập Admin";
                _btnLogin.BackColor = Color.FromArgb(0, 159, 227);
                btnImportData.Enabled = false;
                btnImportData.BackColor = Color.LightGray;

                if (_tpSettings != null && tcDashboard.TabPages.Contains(_tpSettings))
                {
                    // If on settings tab when logged out, switch to dashboard
                    if (tcDashboard.SelectedTab == _tpSettings)
                    {
                        tcDashboard.SelectedTab = tpDashboard;
                    }
                    tcDashboard.TabPages.Remove(_tpSettings);
                }
            }
        }

        private void SaveOriginalBounds(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                _originalControlRects[c] = c.Bounds;
                _originalFonts[c] = c.Font.Size;
                if (c.HasChildren) SaveOriginalBounds(c);
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (_originalFormSize.Width == 0 || this.WindowState == FormWindowState.Minimized) return;

            float ratioX = (float)this.ClientSize.Width / _originalFormSize.Width;
            float ratioY = (float)this.ClientSize.Height / _originalFormSize.Height;
            float ratioFont = Math.Min(ratioX, ratioY);

            this.SuspendLayout();
            ScaleControls(this, ratioX, ratioY, ratioFont);
            this.ResumeLayout();
        }

        private void ScaleControls(Control parent, float ratioX, float ratioY, float ratioFont)
        {
            foreach (Control c in parent.Controls)
            {
                if (_originalControlRects.TryGetValue(c, out Rectangle rect))
                {
                    c.Bounds = new Rectangle(
                        (int)(rect.X * ratioX),
                        (int)(rect.Y * ratioY),
                        (int)(rect.Width * ratioX),
                        (int)(rect.Height * ratioY)
                    );
                    if (_originalFonts.TryGetValue(c, out float fontSize))
                    {
                        c.Font = new Font(c.Font.FontFamily, fontSize * ratioFont, c.Font.Style);
                    }
                }
                if (c.HasChildren) ScaleControls(c, ratioX, ratioY, ratioFont);
            }
        }

        #endregion

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}
