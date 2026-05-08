using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Test.UI.Controls;

namespace Test.UI.Forms
{
    public partial class DashboardTab : UserControl
    {
        public event EventHandler<ConnectionEventArgs>? OnConnectRequest;
        public event EventHandler? OnSaveRequest;

        public DashboardTab()
        {
            InitializeComponent();
            SetupStyles();
            WireInternalEvents();
        }

        private void WireInternalEvents()
        {
            btnConnect.Click += (s, e) => {
                OnConnectRequest?.Invoke(this, new ConnectionEventArgs { Ip = tboIp.Text, Port = int.Parse(tboPort.Text) });
            };
            btnSave.Click += (s, e) => OnSaveRequest?.Invoke(this, EventArgs.Empty);
        }

        private void SetupStyles()
        {
            this.BackColor = AppColors.Background;
            lbLiveWeight.ForeColor = AppColors.TextMuted;
        }

        public void UpdateWeight(decimal weight, string unit, bool isStable, ScaleStatus status)
        {
            if (status == ScaleStatus.Overload)
            {
                lbLiveWeight.Text = "OVERLOAD";
                lbLiveWeight.ForeColor = AppColors.BrandRed;
                progCapacity.Value = 100;
            }
            else if (status == ScaleStatus.Underload)
            {
                lbLiveWeight.Text = "UNDERLOAD";
                lbLiveWeight.ForeColor = AppColors.BrandRed;
                progCapacity.Value = 0;
            }
            else
            {
                lbLiveWeight.Text = $"{weight:F4} {unit}";
                lbLiveWeight.ForeColor = isStable ? AppColors.WeightStable : AppColors.WeightUnstable;
                
                // Giả định Max là 220g cho ML204T
                int pct = (int)((weight / 220m) * 100);
                progCapacity.Value = Math.Min(100, Math.Max(0, pct));
            }
        }

        public void SetConnectionStatus(bool connected, bool transferring)
        {
            ledStatus.IsConnected = connected;
            ledStatus.IsTransferring = transferring;
            lbStatusText.Text = connected ? "CONNECTED" : "DISCONNECTED";
            lbStatusText.ForeColor = connected ? AppColors.StatusConnected : AppColors.StatusIdleText;
        }
    }

    public class ConnectionEventArgs : EventArgs
    {
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 9100;
    }
}
