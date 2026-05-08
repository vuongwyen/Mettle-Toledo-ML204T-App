namespace Test.UI.Forms
{
    partial class DashboardTab
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            cardConnection = new MaterialSkin.Controls.MaterialCard();
            ledStatus = new Test.UI.Controls.ConnectionLed();
            lbStatusText = new MaterialSkin.Controls.MaterialLabel();
            tboIp = new MaterialSkin.Controls.MaterialTextBox();
            tboPort = new MaterialSkin.Controls.MaterialTextBox();
            btnConnect = new MaterialSkin.Controls.MaterialButton();
            
            cardWeight = new MaterialSkin.Controls.MaterialCard();
            lbLiveWeight = new Label();
            progCapacity = new MaterialSkin.Controls.MaterialProgressBar();
            lbCapacityText = new MaterialSkin.Controls.MaterialLabel();
            btnSave = new MaterialSkin.Controls.MaterialButton();
            
            tboNat = new MaterialSkin.Controls.MaterialTextBox();
            tboBatch = new MaterialSkin.Controls.MaterialTextBox();
            tboSample = new MaterialSkin.Controls.MaterialTextBox();
            tboLocation = new MaterialSkin.Controls.MaterialTextBox();

            cardConnection.SuspendLayout();
            cardWeight.SuspendLayout();
            SuspendLayout();

            // cardConnection
            cardConnection.Controls.Add(ledStatus);
            cardConnection.Controls.Add(lbStatusText);
            cardConnection.Controls.Add(tboIp);
            cardConnection.Controls.Add(tboPort);
            cardConnection.Controls.Add(btnConnect);
            cardConnection.Location = new System.Drawing.Point(15, 15);
            cardConnection.Size = new System.Drawing.Size(400, 300);

            ledStatus.Location = new System.Drawing.Point(20, 20);
            
            lbStatusText.Location = new System.Drawing.Point(50, 20);
            lbStatusText.Text = "DISCONNECTED";
            lbStatusText.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;

            tboIp.Location = new System.Drawing.Point(20, 60);
            tboIp.Size = new System.Drawing.Size(360, 50);
            tboIp.Hint = "IP Address";

            tboPort.Location = new System.Drawing.Point(20, 120);
            tboPort.Size = new System.Drawing.Size(360, 50);
            tboPort.Hint = "TCP Port";

            btnConnect.Location = new System.Drawing.Point(20, 200);
            btnConnect.Size = new System.Drawing.Size(360, 40);
            btnConnect.Text = "CONNECT TO SCALE";
            btnConnect.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;

            // cardWeight
            cardWeight.Controls.Add(lbLiveWeight);
            cardWeight.Controls.Add(progCapacity);
            cardWeight.Controls.Add(lbCapacityText);
            cardWeight.Controls.Add(btnSave);
            cardWeight.Location = new System.Drawing.Point(430, 15);
            cardWeight.Size = new System.Drawing.Size(940, 770);

            lbLiveWeight.Dock = DockStyle.Top;
            lbLiveWeight.Height = 400;
            lbLiveWeight.Font = new Font("Bahnschrift Condensed", 96F);
            lbLiveWeight.Text = "0.0000 g";
            lbLiveWeight.TextAlign = ContentAlignment.MiddleCenter;

            // Inputs
            tboNat.Location = new Point(40, 420);
            tboNat.Size = new Size(420, 50);
            tboNat.Hint = "Mã NAT";

            tboBatch.Location = new Point(480, 420);
            tboBatch.Size = new Size(420, 50);
            tboBatch.Hint = "Lô hàng (Batch)";

            tboSample.Location = new Point(40, 490);
            tboSample.Size = new Size(860, 50);
            tboSample.Hint = "Tên mẫu (Sample Name)";

            tboLocation.Location = new Point(40, 560);
            tboLocation.Size = new Size(860, 50);
            tboLocation.Hint = "Vị trí (Location)";

            progCapacity.Location = new System.Drawing.Point(40, 640);
            progCapacity.Size = new System.Drawing.Size(860, 10);

            lbCapacityText.Location = new System.Drawing.Point(40, 660);
            lbCapacityText.Text = "Current Load (% of Max Capacity)";

            btnSave.Location = new System.Drawing.Point(40, 700);
            btnSave.Size = new System.Drawing.Size(860, 50);
            btnSave.Text = "PULL & SAVE DATA";
            btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnSave.UseAccentColor = true;

            // DashboardTab
            cardWeight.Controls.Add(tboNat);
            cardWeight.Controls.Add(tboBatch);
            cardWeight.Controls.Add(tboSample);
            cardWeight.Controls.Add(tboLocation);
            Controls.Add(cardConnection);
            Controls.Add(cardWeight);
            Size = new System.Drawing.Size(1395, 807);
            
            cardConnection.ResumeLayout(false);
            cardWeight.ResumeLayout(false);
            ResumeLayout(false);
        }

        private MaterialSkin.Controls.MaterialCard cardConnection;
        private Test.UI.Controls.ConnectionLed ledStatus;
        private MaterialSkin.Controls.MaterialLabel lbStatusText;
        private MaterialSkin.Controls.MaterialTextBox tboIp;
        private MaterialSkin.Controls.MaterialTextBox tboPort;
        private MaterialSkin.Controls.MaterialButton btnConnect;
        
        private MaterialSkin.Controls.MaterialCard cardWeight;
        private Label lbLiveWeight;
        private MaterialSkin.Controls.MaterialProgressBar progCapacity;
        private MaterialSkin.Controls.MaterialLabel lbCapacityText;
        private MaterialSkin.Controls.MaterialButton btnSave;
        public MaterialSkin.Controls.MaterialTextBox tboNat;
        public MaterialSkin.Controls.MaterialTextBox tboBatch;
        public MaterialSkin.Controls.MaterialTextBox tboSample;
        public MaterialSkin.Controls.MaterialTextBox tboLocation;
    }
}
