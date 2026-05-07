using System;
using System.Drawing;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        private ConnectionManager _connectionManager;

        public Form1()
        {
            InitializeComponent();
            _connectionManager = new ConnectionManager();
            _connectionManager.OnStateChanged += ConnectionManager_OnStateChanged;
            _connectionManager.OnDataReceived += ConnectionManager_OnDataReceived;

            btnConnectIpadd.Click += btnConnectIpadd_Click;
            this.FormClosing += Form1_FormClosing;
        }

        private async void btnConnectIpadd_Click(object sender, EventArgs e)
        {
            if (_connectionManager.IsConnected)
            {
                _connectionManager.Disconnect();
            }
            else
            {
                string ip = tboIpadd.Text.Trim();
                if (string.IsNullOrEmpty(ip) || !int.TryParse(tboTcpport.Text.Trim(), out int port))
                {
                    MessageBox.Show("Vui lòng nhập IP và Port hợp lệ.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    btnConnectIpadd.Enabled = false;
                    await _connectionManager.ConnectAsync(ip, port);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnConnectIpadd.Enabled = true;
                }
            }
        }

        private void ConnectionManager_OnStateChanged(bool isConnected)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ConnectionManager_OnStateChanged(isConnected)));
                return;
            }

            if (isConnected)
            {
                lbStatusconnection.Text = "Status : CONNECTED";
                lbStatusconnection.BackColor = Color.LimeGreen;
                btnConnectIpadd.Text = "Ngắt kết nối (Disconnect)";
            }
            else
            {
                lbStatusconnection.Text = "Status : DISCONNECTED";
                lbStatusconnection.BackColor = Color.Silver;
                btnConnectIpadd.Text = "Kết nối (Connect)";
                panel1.BackColor = SystemColors.WindowFrame;
                lbLiveweight.ForeColor = Color.LimeGreen;
            }
        }

        private void ConnectionManager_OnDataReceived(string rawData)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ConnectionManager_OnDataReceived(rawData)));
                return;
            }

            var scaleData = MtSicsParser.Parse(rawData);
            if (scaleData.HasValue)
            {
                lbLiveweight.Text = $"{scaleData.Value.Weight} {scaleData.Value.Unit}";
                if (scaleData.Value.IsStable)
                {
                    panel1.BackColor = Color.LimeGreen;
                    lbLiveweight.ForeColor = Color.Black;
                }
                else
                {
                    panel1.BackColor = Color.Orange;
                    lbLiveweight.ForeColor = Color.Black;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _connectionManager?.Dispose();
        }
    }
}
