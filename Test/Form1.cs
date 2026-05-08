using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Test.Services;
using Test.UI.Forms;

namespace Test
{
    public partial class Form1 : MaterialForm
    {
        private readonly MaterialSkinManager _skinManager;
        private readonly ScaleService _scaleService;
        private readonly DatabaseService _databaseService;
        private ScaleData? _lastData;

        public Form1()
        {
            InitializeComponent();

            // Setup MaterialSkin
            _skinManager = MaterialSkinManager.Instance;
            _skinManager.AddFormToManage(this);
            _skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            _skinManager.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue200, Accent.LightBlue200,
                TextShade.WHITE
            );

            // Initialize Services
            _scaleService = new ScaleService();
            _databaseService = new DatabaseService();

            historyTab1.SetDatabaseService(_databaseService);

            WireEvents();
        }

        private void WireEvents()
        {
            // Scale Events
            _scaleService.OnDataReceived += data => {
                // Task 2.3 — Thread-safe: PushSample uses ConcurrentQueue, no lock needed.
                // Decoupled from UI thread — TCP speed is preserved.
                analyticsTab1.PushSample((double)data.Weight);

                // Dashboard update — marshaled to UI thread
                this.BeginInvoke(new Action(() => {
                    _lastData = data;
                    dashboardTab1.UpdateWeight(data.Weight, data.Unit, data.IsStable, data.Status);
                }));
            };

            _scaleService.OnStatusChanged += (connected, reconnecting) => {
                this.BeginInvoke(new Action(() => {
                    dashboardTab1.SetConnectionStatus(connected, reconnecting);
                }));
            };

            // UI Events from DashboardTab
            dashboardTab1.OnConnectRequest += async (s, e) => {
                try {
                    await _scaleService.ConnectAsync(e.Ip, e.Port);
                } catch (Exception ex) {
                    MessageBox.Show($"Kết nối thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            dashboardTab1.OnSaveRequest += (s, e) => SaveData();
        }

        private void SaveData()
        {
            if (!_lastData.HasValue) {
                MessageBox.Show("Chưa có dữ liệu từ cân.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var record = new ScaleRecord {
                Weight = _lastData.Value.Weight,
                Unit = _lastData.Value.Unit,
                NatCode = dashboardTab1.tboNat.Text.Trim(),
                Batch = dashboardTab1.tboBatch.Text.Trim(),
                SampleName = dashboardTab1.tboSample.Text.Trim(),
                Location = dashboardTab1.tboLocation.Text.Trim()
            };

            _databaseService.InsertRecord(record);
            historyTab1.RefreshData(); // Cập nhật grid lịch sử
            System.Media.SystemSounds.Beep.Play();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _scaleService.Dispose();
            base.OnFormClosing(e);
        }
    }
}
