using System;
using System.Drawing;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Test
{
    public partial class Form1 : Form
    {
        private ConnectionManager _connectionManager;
        private DataRepository _repository;
        private CsvExportService _csvExportService;
        private ExcelExportService _excelExportService;
        private Test.Services.CsvImportService _csvImportService;
        private Test.Services.ExcelImportService _excelImportService;
        private ScaleData? _lastScaleData;
        private System.Collections.Generic.List<ScaleRecord> _allRecords = new System.Collections.Generic.List<ScaleRecord>();

        // Analytics
        private PlotModel   _plotModel   = null!;
        private LineSeries  _weightSeries = null!;
        private decimal?    _sessionMin;
        private decimal?    _sessionMax;
        private const int   MaxChartPoints = 300;

        // [R-01] Producer-Consumer: Background thread enqueues, WinForms Timer dequeues in batch.
        // ConcurrentQueue<T> là lock-free — an toàn cho write từ background và read từ UI thread.
        private readonly System.Collections.Concurrent.ConcurrentQueue<ScaleData> _dataQueue
            = new System.Collections.Concurrent.ConcurrentQueue<ScaleData>();
        private System.Windows.Forms.Timer _uiTimer = null!;

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

            _repository       = new DataRepository();
            _csvExportService  = new CsvExportService();
            _excelExportService = new ExcelExportService();
            _csvImportService = new Test.Services.CsvImportService();
            _excelImportService = new Test.Services.ExcelImportService();

            btnConnectIpadd.Click += btnConnectIpadd_Click;
            btnPolling.Click      += btnPolling_Click;
            btnExportdata.Click   += btnExportdata_Click;
            btnImportData.Click   += btnImportData_Click;
            tboSearch.TextChanged += tboSearch_TextChanged;

            tboNat.KeyDown        += Tbo_KeyDown;
            tboBatch.KeyDown      += Tbo_KeyDown;
            tboSamplename.KeyDown += Tbo_KeyDown;
            tboLocation.KeyDown   += Tbo_KeyDown;

            trayIcon.Icon = this.Icon;
            trayIcon.MouseDoubleClick += (s, e) => RestoreFromTray();

            this.FormClosing += Form1_FormClosing;

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
                lbStatusconnection.Text      = "⬤  CONNECTED";
                lbStatusconnection.BackColor = AppColors.StatusConnectedBg;
                lbStatusconnection.ForeColor = AppColors.StatusConnected;
                btnConnectIpadd.Text      = "⏹  Ngắt kết nối";
                btnConnectIpadd.BackColor = AppColors.BrandRed;
                btnConnectIpadd.FlatAppearance.MouseOverBackColor = AppColors.AccentRedHover;
                btnConnectIpadd.Enabled   = true;

                // Cập nhật thông tin nhận diện cân
                var info = _connectionManager.ConnectedScale;
                if (info != null)
                {
                    lbScaleModel.Text = $"Model: {info.Model}";
                    lbScaleSN.Text    = $"S/N: {info.SerialNumber}";
                    lbScaleModel.Visible = true;
                    lbScaleSN.Visible    = true;
                }
            }
            else if (isReconnecting)
            {
                // Lấy lý do lỗi nếu có (vd: sai thiết bị, timeout)
                string reason = _connectionManager.LastError ?? "Mất kết nối";
                bool isWrongDevice = reason.Contains("MT-SICS");

                lbStatusconnection.Text      = isWrongDevice
                    ? "⚠  SAI THIẼT BỊ"
                    : "⬤  RECONNECTING...";
                lbStatusconnection.BackColor = AppColors.StatusWarningBg;
                lbStatusconnection.ForeColor = AppColors.StatusWarning;
                btnConnectIpadd.Text      = "❌  Hủy Reconnect";
                btnConnectIpadd.BackColor = AppColors.BrandRed;
                btnConnectIpadd.Enabled   = true;
                panel1.BackColor          = AppColors.PanelIdle;
                lbLiveweight.ForeColor    = AppColors.StatusWarning;
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
                lbStatusconnection.Text      = "⬤  DISCONNECTED";
                lbStatusconnection.BackColor = AppColors.StatusIdle;
                lbStatusconnection.ForeColor = AppColors.StatusIdleText;
                btnConnectIpadd.Text      = "🔌  Kết nối (Connect)";
                btnConnectIpadd.BackColor = AppColors.BrandBlue;
                btnConnectIpadd.FlatAppearance.MouseOverBackColor = AppColors.BrandBlueDark;
                btnConnectIpadd.Enabled   = true;
                panel1.BackColor          = AppColors.PanelIdle;
                lbLiveweight.ForeColor    = AppColors.TextMuted;

                lbScaleModel.Text = "Model: -";
                lbScaleSN.Text    = "S/N: -";
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
                lbLiveweight.Text      = "OVERLOAD";
                panel1.BackColor       = AppColors.StatusWarningBg;
                lbLiveweight.ForeColor = AppColors.BrandRed;
            }
            else if (latest.Status == ScaleStatus.Underload)
            {
                lbLiveweight.Text      = "UNDERLOAD";
                panel1.BackColor       = AppColors.StatusWarningBg;
                lbLiveweight.ForeColor = AppColors.BrandRed;
            }
            else if (latest.Status == ScaleStatus.Invalid)
            {
                lbLiveweight.Text      = "ERR / BUSY";
                panel1.BackColor       = AppColors.StatusWarningBg;
                lbLiveweight.ForeColor = AppColors.StatusWarning;
            }
            else
            {
                lbLiveweight.Text      = $"{latest.Weight:F4} {latest.Unit}";
                panel1.BackColor       = latest.IsStable ? AppColors.PanelStable   : AppColors.PanelUnstable;
                lbLiveweight.ForeColor = latest.IsStable ? AppColors.WeightStable  : AppColors.WeightUnstable;
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
            float fontSize  = isSelected ? 10.5F : 10F;
            var fontStyle   = isSelected ? FontStyle.Bold : FontStyle.Regular;

            using var font      = new Font("Segoe UI", fontSize, fontStyle);
            using var textBrush = new SolidBrush(textColor);
            // [FIX B7] using để tránh GDI+ resource leak
            using var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
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
                Background           = OxyColor.FromRgb(255, 255, 255),
                PlotAreaBackground   = OxyColor.FromRgb(248, 250, 252),
                TextColor            = OxyColor.FromRgb(30, 41, 59),
                PlotAreaBorderColor  = OxyColor.FromRgb(226, 232, 240),
                TitleFontSize        = 14
            };

            _plotModel.Axes.Add(new DateTimeAxis
            {
                Position           = AxisPosition.Bottom,
                StringFormat       = "HH:mm:ss",
                Title              = "Thời gian",
                TextColor          = OxyColor.FromRgb(100, 116, 139),
                TicklineColor      = OxyColor.FromRgb(226, 232, 240),
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(120, 226, 232, 240),
                IntervalType       = DateTimeIntervalType.Seconds
            });

            _plotModel.Axes.Add(new LinearAxis
            {
                Position           = AxisPosition.Left,
                Title              = "Khối lượng",
                TextColor          = OxyColor.FromRgb(100, 116, 139),
                TicklineColor      = OxyColor.FromRgb(226, 232, 240),
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(120, 226, 232, 240)
            });

            _weightSeries = new LineSeries
            {
                Title           = "Khối lượng",
                Color           = OxyColor.FromRgb(0, 159, 227),  // BrandBlue
                StrokeThickness = 2.5,
                MarkerType      = MarkerType.None
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
                int todayCount      = _repository.GetTodayCount();
                decimal batchTotal  = _repository.GetBatchTotal(tboBatch.Text.Trim());

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
                    // Nếu ở ô cuối cùng, có thể focus ngược lại ô đầu hoặc chốt số?
                    // Ở đây tôi chọn quay lại ô đầu để chuẩn bị cho lô tiếp theo
                    tboNat.Focus();
                }
            }
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
                    Location = tboLocation.Text.Trim()
                };

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

        private void LoadDataToGrid()
        {
            try
            {
                _allRecords = _repository.GetAll();
                ApplyFilter();
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            string keyword = tboSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                dgvWeightsheet.DataSource = _allRecords;
            }
            else
            {
                var filtered = new System.Collections.Generic.List<ScaleRecord>();
                foreach (var record in _allRecords)
                {
                    if ((record.NatCode != null && record.NatCode.ToLower().Contains(keyword)) ||
                        (record.Batch != null && record.Batch.ToLower().Contains(keyword)) ||
                        (record.SampleName != null && record.SampleName.ToLower().Contains(keyword)) ||
                        (record.Location != null && record.Location.ToLower().Contains(keyword)) ||
                        (record.Unit != null && record.Unit.ToLower().Contains(keyword)))
                    {
                        filtered.Add(record);
                    }
                }
                dgvWeightsheet.DataSource = filtered;
            }
        }

        private void tboSearch_TextChanged(object? sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void btnExportdata_Click(object? sender, EventArgs e)
        {
            try
            {
                var data = _repository.GetAll();
                if (data.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dlg = new SaveFileDialog())
                {
                    dlg.Title    = "Xuất dữ liệu cân";
                    dlg.Filter   = "Excel files (*.xlsx)|*.xlsx|CSV files (*.csv)|*.csv";
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

        // [FIX N2] Đã xóa Form1_Resize handler rỗng

        private void RestoreFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            trayIcon.Visible = false;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // [R-01] Dừng timer trước khi dispose form — tránh Tick chạy sau khi handle bị hủy
            _uiTimer?.Stop();
            _uiTimer?.Dispose();
            _connectionManager?.Dispose();
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }
    }
}
