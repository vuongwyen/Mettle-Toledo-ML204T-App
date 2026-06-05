namespace Test
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tcDashboard = new TabControl();
            tpDashboard = new TabPage();
            groupBox3 = new GroupBox();
            chkAutoPolling = new CheckBox();
            btnPolling = new Button();
            label11 = new Label();
            panel1 = new Panel();
            lbLiveweight = new Label();
            groupBox2 = new GroupBox();
            tboLocation = new TextBox();
            label10 = new Label();
            tboTester = new TextBox();
            lblTester = new Label();
            tboSamplename = new TextBox();
            label9 = new Label();
            tboBatch = new TextBox();
            label8 = new Label();
            tboNat = new TextBox();
            label7 = new Label();
            label6 = new Label();
            groupBox1 = new GroupBox();
            lbScaleSN = new Label();
            lbScaleModel = new Label();
            lbStatusconnection = new Label();
            btnConnectIpadd = new Button();
            tboTcpport = new TextBox();
            label3 = new Label();
            tboIpadd = new TextBox();
            label2 = new Label();
            label1 = new Label();
            tpAnalytics = new TabPage();
            pnlStats = new Panel();
            lbStatTodayCaption = new Label();
            lbStatTodayValue = new Label();
            lbStatBatchCaption = new Label();
            lbStatBatchValue = new Label();
            lbStatMinCaption = new Label();
            lbStatMinValue = new Label();
            lbStatMaxCaption = new Label();
            lbStatMaxValue = new Label();
            plotViewLiveChart = new OxyPlot.WindowsForms.PlotView();
            tpDatasheet = new TabPage();
            tboSearch = new TextBox();
            lbSearch = new Label();
            btnImportData = new Button();
            btnExportdata = new Button();
            label12 = new Label();
            dgvWeightsheet = new DataGridView();
            trayIcon = new NotifyIcon(components);
            trayContextMenu = new ContextMenuStrip(components);
            trayMenuOpen = new ToolStripMenuItem();
            trayMenuSep = new ToolStripSeparator();
            trayMenuExit = new ToolStripMenuItem();
            sqliteConnection1 = new Microsoft.Data.Sqlite.SqliteConnection();
            sqliteConnection2 = new Microsoft.Data.Sqlite.SqliteConnection();
            tcDashboard.SuspendLayout();
            tpDashboard.SuspendLayout();
            groupBox3.SuspendLayout();
            panel1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            tpAnalytics.SuspendLayout();
            pnlStats.SuspendLayout();
            tpDatasheet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvWeightsheet).BeginInit();
            trayContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // tcDashboard
            // 
            tcDashboard.Controls.Add(tpDashboard);
            tcDashboard.Controls.Add(tpAnalytics);
            tcDashboard.Controls.Add(tpDatasheet);
            tcDashboard.Dock = DockStyle.Fill;
            tcDashboard.DrawMode = TabDrawMode.OwnerDrawFixed;
            tcDashboard.ItemSize = new Size(180, 40);
            tcDashboard.Location = new Point(0, 0);
            tcDashboard.Name = "tcDashboard";
            tcDashboard.Padding = new Point(20, 8);
            tcDashboard.SelectedIndex = 0;
            tcDashboard.Size = new Size(1403, 840);
            tcDashboard.SizeMode = TabSizeMode.Fixed;
            tcDashboard.TabIndex = 0;
            // 
            // tpDashboard
            // 
            tpDashboard.BackColor = Color.FromArgb(244, 246, 249);
            tpDashboard.Controls.Add(groupBox3);
            tpDashboard.Controls.Add(groupBox2);
            tpDashboard.Controls.Add(groupBox1);
            tpDashboard.Location = new Point(4, 44);
            tpDashboard.Name = "tpDashboard";
            tpDashboard.Padding = new Padding(3);
            tpDashboard.Size = new Size(1395, 792);
            tpDashboard.TabIndex = 0;
            tpDashboard.Text = "Dashboard";
            // 
            // groupBox3
            // 
            groupBox3.BackColor = Color.White;
            groupBox3.Controls.Add(chkAutoPolling);
            groupBox3.Controls.Add(btnPolling);
            groupBox3.Controls.Add(label11);
            groupBox3.Controls.Add(panel1);
            groupBox3.ForeColor = Color.FromArgb(30, 41, 59);
            groupBox3.Location = new Point(447, 6);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(943, 793);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            // 
            // chkAutoPolling
            // 
            chkAutoPolling.AutoSize = true;
            chkAutoPolling.BackColor = Color.Transparent;
            chkAutoPolling.Font = new Font("Segoe UI", 12F);
            chkAutoPolling.ForeColor = Color.FromArgb(0, 159, 227);
            chkAutoPolling.Location = new Point(21, 612);
            chkAutoPolling.Name = "chkAutoPolling";
            chkAutoPolling.Size = new Size(301, 32);
            chkAutoPolling.TabIndex = 7;
            chkAutoPolling.Text = "⚡  Kích hoạt Chốt số tự động";
            chkAutoPolling.UseVisualStyleBackColor = false;
            // 
            // btnPolling
            // 
            btnPolling.BackColor = Color.FromArgb(227, 6, 19);
            btnPolling.FlatAppearance.BorderSize = 0;
            btnPolling.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 5, 15);
            btnPolling.FlatStyle = FlatStyle.Flat;
            btnPolling.Font = new Font("Segoe UI", 30F, FontStyle.Bold);
            btnPolling.ForeColor = Color.White;
            btnPolling.Location = new Point(21, 652);
            btnPolling.Name = "btnPolling";
            btnPolling.Size = new Size(907, 115);
            btnPolling.TabIndex = 6;
            btnPolling.Text = "⬇  Chốt số liệu (Polling)";
            btnPolling.UseVisualStyleBackColor = false;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.BackColor = Color.Transparent;
            label11.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            label11.ForeColor = Color.FromArgb(227, 6, 19);
            label11.Location = new Point(18, 20);
            label11.Name = "label11";
            label11.Size = new Size(152, 30);
            label11.TabIndex = 1;
            label11.Text = "3. Số cân Live";
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(lbLiveweight);
            panel1.Location = new Point(21, 52);
            panel1.Name = "panel1";
            panel1.Size = new Size(907, 545);
            panel1.TabIndex = 0;
            // 
            // lbLiveweight
            // 
            lbLiveweight.Dock = DockStyle.Fill;
            lbLiveweight.Font = new Font("Bahnschrift Condensed", 96F);
            lbLiveweight.ForeColor = Color.FromArgb(148, 163, 184);
            lbLiveweight.Location = new Point(0, 0);
            lbLiveweight.Name = "lbLiveweight";
            lbLiveweight.Size = new Size(907, 545);
            lbLiveweight.TabIndex = 0;
            lbLiveweight.Text = "0.0000 g";
            lbLiveweight.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            groupBox2.BackColor = Color.White;
            groupBox2.Controls.Add(tboLocation);
            groupBox2.Controls.Add(label10);
            groupBox2.Controls.Add(tboTester);
            groupBox2.Controls.Add(lblTester);
            groupBox2.Controls.Add(tboSamplename);
            groupBox2.Controls.Add(label9);
            groupBox2.Controls.Add(tboBatch);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(tboNat);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label6);
            groupBox2.ForeColor = Color.FromArgb(30, 41, 59);
            groupBox2.Location = new Point(8, 357);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(433, 442);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            // 
            // tboLocation
            // 
            tboLocation.BackColor = Color.White;
            tboLocation.BorderStyle = BorderStyle.FixedSingle;
            tboLocation.Font = new Font("Segoe UI", 10.8F);
            tboLocation.ForeColor = Color.FromArgb(30, 41, 59);
            tboLocation.Location = new Point(10, 225);
            tboLocation.Name = "tboLocation";
            tboLocation.Size = new Size(200, 31);
            tboLocation.TabIndex = 10;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.BackColor = Color.Transparent;
            label10.Font = new Font("Segoe UI", 10.8F);
            label10.ForeColor = Color.FromArgb(100, 116, 139);
            label10.Location = new Point(0, 196);
            label10.Name = "label10";
            label10.Size = new Size(130, 25);
            label10.TabIndex = 9;
            label10.Text = "Vị trí (Location)";
            // 
            // tboTester
            // 
            tboTester.BackColor = Color.White;
            tboTester.BorderStyle = BorderStyle.FixedSingle;
            tboTester.Font = new Font("Segoe UI", 10.8F);
            tboTester.ForeColor = Color.FromArgb(30, 41, 59);
            tboTester.Location = new Point(223, 225);
            tboTester.Name = "tboTester";
            tboTester.Size = new Size(200, 31);
            tboTester.TabIndex = 11;
            // 
            // lblTester
            // 
            lblTester.AutoSize = true;
            lblTester.BackColor = Color.Transparent;
            lblTester.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblTester.ForeColor = Color.FromArgb(71, 85, 105);
            lblTester.Location = new Point(223, 200);
            lblTester.Name = "lblTester";
            lblTester.Size = new Size(134, 20);
            lblTester.TabIndex = 12;
            lblTester.Text = "Người test (Tester)";
            // 
            // tboSamplename
            // 
            tboSamplename.BackColor = Color.White;
            tboSamplename.BorderStyle = BorderStyle.FixedSingle;
            tboSamplename.Font = new Font("Segoe UI", 10.8F);
            tboSamplename.ForeColor = Color.FromArgb(30, 41, 59);
            tboSamplename.Location = new Point(223, 155);
            tboSamplename.Name = "tboSamplename";
            tboSamplename.Size = new Size(200, 31);
            tboSamplename.TabIndex = 8;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.BackColor = Color.Transparent;
            label9.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            label9.ForeColor = Color.FromArgb(71, 85, 105);
            label9.Location = new Point(223, 130);
            label9.Name = "label9";
            label9.Size = new Size(176, 20);
            label9.TabIndex = 7;
            label9.Text = "Tên mẫu (Sample Name)";
            // 
            // tboBatch
            // 
            tboBatch.BackColor = Color.White;
            tboBatch.BorderStyle = BorderStyle.FixedSingle;
            tboBatch.Font = new Font("Segoe UI", 10.8F);
            tboBatch.ForeColor = Color.FromArgb(30, 41, 59);
            tboBatch.Location = new Point(10, 155);
            tboBatch.Name = "tboBatch";
            tboBatch.Size = new Size(200, 31);
            tboBatch.TabIndex = 6;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.BackColor = Color.Transparent;
            label8.Font = new Font("Segoe UI", 10.8F);
            label8.ForeColor = Color.FromArgb(100, 116, 139);
            label8.Location = new Point(0, 130);
            label8.Name = "label8";
            label8.Size = new Size(134, 25);
            label8.TabIndex = 5;
            label8.Text = "Lô hàng (Batch)";
            // 
            // tboNat
            // 
            tboNat.BackColor = Color.White;
            tboNat.BorderStyle = BorderStyle.FixedSingle;
            tboNat.Font = new Font("Segoe UI", 10.8F);
            tboNat.ForeColor = Color.FromArgb(30, 41, 59);
            tboNat.Location = new Point(10, 85);
            tboNat.Name = "tboNat";
            tboNat.Size = new Size(413, 31);
            tboNat.TabIndex = 4;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = Color.Transparent;
            label7.Font = new Font("Segoe UI", 10.8F);
            label7.ForeColor = Color.FromArgb(100, 116, 139);
            label7.Location = new Point(0, 60);
            label7.Name = "label7";
            label7.Size = new Size(86, 25);
            label7.TabIndex = 3;
            label7.Text = "Mã NART";
            label7.Click += label7_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = Color.Transparent;
            label6.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            label6.ForeColor = Color.FromArgb(227, 6, 19);
            label6.Location = new Point(6, 23);
            label6.Name = "label6";
            label6.Size = new Size(259, 30);
            label6.TabIndex = 1;
            label6.Text = "2. Thông tin tham chiếu";
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.White;
            groupBox1.Controls.Add(lbScaleSN);
            groupBox1.Controls.Add(lbScaleModel);
            groupBox1.Controls.Add(lbStatusconnection);
            groupBox1.Controls.Add(btnConnectIpadd);
            groupBox1.Controls.Add(tboTcpport);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(tboIpadd);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.ForeColor = Color.FromArgb(30, 41, 59);
            groupBox1.Location = new Point(8, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(433, 345);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // lbScaleSN
            // 
            lbScaleSN.AutoSize = true;
            lbScaleSN.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lbScaleSN.ForeColor = Color.FromArgb(100, 116, 139);
            lbScaleSN.Location = new Point(230, 316);
            lbScaleSN.Name = "lbScaleSN";
            lbScaleSN.Size = new Size(46, 20);
            lbScaleSN.TabIndex = 8;
            lbScaleSN.Text = "S/N: -";
            // 
            // lbScaleModel
            // 
            lbScaleModel.AutoSize = true;
            lbScaleModel.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lbScaleModel.ForeColor = Color.FromArgb(100, 116, 139);
            lbScaleModel.Location = new Point(10, 316);
            lbScaleModel.Name = "lbScaleModel";
            lbScaleModel.Size = new Size(61, 20);
            lbScaleModel.TabIndex = 7;
            lbScaleModel.Text = "Model: -";
            // 
            // lbStatusconnection
            // 
            lbStatusconnection.BackColor = Color.FromArgb(226, 232, 240);
            lbStatusconnection.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lbStatusconnection.ForeColor = Color.FromArgb(100, 116, 139);
            lbStatusconnection.Location = new Point(6, 277);
            lbStatusconnection.Name = "lbStatusconnection";
            lbStatusconnection.Size = new Size(421, 36);
            lbStatusconnection.TabIndex = 6;
            lbStatusconnection.Text = "⬤  DISCONNECTED";
            lbStatusconnection.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnConnectIpadd
            // 
            btnConnectIpadd.BackColor = Color.FromArgb(0, 159, 227);
            btnConnectIpadd.FlatAppearance.BorderSize = 0;
            btnConnectIpadd.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 128, 192);
            btnConnectIpadd.FlatStyle = FlatStyle.Flat;
            btnConnectIpadd.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            btnConnectIpadd.ForeColor = Color.White;
            btnConnectIpadd.Location = new Point(6, 226);
            btnConnectIpadd.Name = "btnConnectIpadd";
            btnConnectIpadd.Size = new Size(421, 40);
            btnConnectIpadd.TabIndex = 5;
            btnConnectIpadd.Text = "🔌  Kết nối (Connect)";
            btnConnectIpadd.UseVisualStyleBackColor = false;
            // 
            // tboTcpport
            // 
            tboTcpport.BackColor = Color.White;
            tboTcpport.BorderStyle = BorderStyle.FixedSingle;
            tboTcpport.Font = new Font("Segoe UI", 10.8F);
            tboTcpport.ForeColor = Color.FromArgb(30, 41, 59);
            tboTcpport.Location = new Point(6, 173);
            tboTcpport.Name = "tboTcpport";
            tboTcpport.Size = new Size(421, 31);
            tboTcpport.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Segoe UI", 10.8F);
            label3.ForeColor = Color.FromArgb(100, 116, 139);
            label3.Location = new Point(6, 145);
            label3.Name = "label3";
            label3.Size = new Size(78, 25);
            label3.TabIndex = 3;
            label3.Text = "TCP Port";
            // 
            // tboIpadd
            // 
            tboIpadd.BackColor = Color.White;
            tboIpadd.BorderStyle = BorderStyle.FixedSingle;
            tboIpadd.Font = new Font("Segoe UI", 10.8F);
            tboIpadd.ForeColor = Color.FromArgb(30, 41, 59);
            tboIpadd.Location = new Point(6, 91);
            tboIpadd.Name = "tboIpadd";
            tboIpadd.Size = new Size(421, 31);
            tboIpadd.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 10.8F);
            label2.ForeColor = Color.FromArgb(100, 116, 139);
            label2.Location = new Point(6, 63);
            label2.Name = "label2";
            label2.Size = new Size(97, 25);
            label2.TabIndex = 1;
            label2.Text = "IP Address";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(227, 6, 19);
            label1.Location = new Point(0, 12);
            label1.Name = "label1";
            label1.Size = new Size(287, 30);
            label1.TabIndex = 0;
            label1.Text = "1. Cấu hình Mạng (TCP/IP)";
            // 
            // tpAnalytics
            // 
            tpAnalytics.BackColor = Color.FromArgb(244, 246, 249);
            tpAnalytics.Controls.Add(pnlStats);
            tpAnalytics.Controls.Add(plotViewLiveChart);
            tpAnalytics.Location = new Point(4, 44);
            tpAnalytics.Name = "tpAnalytics";
            tpAnalytics.Padding = new Padding(3);
            tpAnalytics.Size = new Size(1395, 792);
            tpAnalytics.TabIndex = 2;
            tpAnalytics.Text = "📈 Analytics";
            // 
            // pnlStats
            // 
            pnlStats.BackColor = Color.White;
            pnlStats.Controls.Add(lbStatTodayCaption);
            pnlStats.Controls.Add(lbStatTodayValue);
            pnlStats.Controls.Add(lbStatBatchCaption);
            pnlStats.Controls.Add(lbStatBatchValue);
            pnlStats.Controls.Add(lbStatMinCaption);
            pnlStats.Controls.Add(lbStatMinValue);
            pnlStats.Controls.Add(lbStatMaxCaption);
            pnlStats.Controls.Add(lbStatMaxValue);
            pnlStats.Location = new Point(6, 6);
            pnlStats.Name = "pnlStats";
            pnlStats.Size = new Size(1383, 115);
            pnlStats.TabIndex = 0;
            // 
            // lbStatTodayCaption
            // 
            lbStatTodayCaption.BackColor = Color.Transparent;
            lbStatTodayCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbStatTodayCaption.ForeColor = Color.FromArgb(100, 116, 139);
            lbStatTodayCaption.Location = new Point(10, 10);
            lbStatTodayCaption.Name = "lbStatTodayCaption";
            lbStatTodayCaption.Size = new Size(330, 22);
            lbStatTodayCaption.TabIndex = 0;
            lbStatTodayCaption.Text = "MẪU HÔM NAY";
            // 
            // lbStatTodayValue
            // 
            lbStatTodayValue.BackColor = Color.Transparent;
            lbStatTodayValue.Font = new Font("Segoe UI", 38F, FontStyle.Bold);
            lbStatTodayValue.ForeColor = Color.FromArgb(227, 6, 19);
            lbStatTodayValue.Location = new Point(10, 35);
            lbStatTodayValue.Name = "lbStatTodayValue";
            lbStatTodayValue.Size = new Size(330, 70);
            lbStatTodayValue.TabIndex = 1;
            lbStatTodayValue.Text = "0";
            // 
            // lbStatBatchCaption
            // 
            lbStatBatchCaption.BackColor = Color.Transparent;
            lbStatBatchCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbStatBatchCaption.ForeColor = Color.FromArgb(100, 116, 139);
            lbStatBatchCaption.Location = new Point(355, 10);
            lbStatBatchCaption.Name = "lbStatBatchCaption";
            lbStatBatchCaption.Size = new Size(330, 22);
            lbStatBatchCaption.TabIndex = 2;
            lbStatBatchCaption.Text = "TỔNG LÔ HIỆN TẠI";
            // 
            // lbStatBatchValue
            // 
            lbStatBatchValue.BackColor = Color.Transparent;
            lbStatBatchValue.Font = new Font("Segoe UI", 30F, FontStyle.Bold);
            lbStatBatchValue.ForeColor = Color.FromArgb(0, 159, 227);
            lbStatBatchValue.Location = new Point(355, 35);
            lbStatBatchValue.Name = "lbStatBatchValue";
            lbStatBatchValue.Size = new Size(330, 70);
            lbStatBatchValue.TabIndex = 3;
            lbStatBatchValue.Text = "0.0000 g";
            // 
            // lbStatMinCaption
            // 
            lbStatMinCaption.BackColor = Color.Transparent;
            lbStatMinCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbStatMinCaption.ForeColor = Color.FromArgb(100, 116, 139);
            lbStatMinCaption.Location = new Point(700, 10);
            lbStatMinCaption.Name = "lbStatMinCaption";
            lbStatMinCaption.Size = new Size(330, 22);
            lbStatMinCaption.TabIndex = 4;
            lbStatMinCaption.Text = "MIN (PHIÊN)";
            // 
            // lbStatMinValue
            // 
            lbStatMinValue.BackColor = Color.Transparent;
            lbStatMinValue.Font = new Font("Segoe UI", 38F, FontStyle.Bold);
            lbStatMinValue.ForeColor = Color.FromArgb(217, 119, 6);
            lbStatMinValue.Location = new Point(700, 35);
            lbStatMinValue.Name = "lbStatMinValue";
            lbStatMinValue.Size = new Size(330, 70);
            lbStatMinValue.TabIndex = 5;
            lbStatMinValue.Text = "---";
            // 
            // lbStatMaxCaption
            // 
            lbStatMaxCaption.BackColor = Color.Transparent;
            lbStatMaxCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbStatMaxCaption.ForeColor = Color.FromArgb(100, 116, 139);
            lbStatMaxCaption.Location = new Point(1046, 10);
            lbStatMaxCaption.Name = "lbStatMaxCaption";
            lbStatMaxCaption.Size = new Size(330, 22);
            lbStatMaxCaption.TabIndex = 6;
            lbStatMaxCaption.Text = "MAX (PHIÊN)";
            // 
            // lbStatMaxValue
            // 
            lbStatMaxValue.BackColor = Color.Transparent;
            lbStatMaxValue.Font = new Font("Segoe UI", 38F, FontStyle.Bold);
            lbStatMaxValue.ForeColor = Color.FromArgb(227, 6, 19);
            lbStatMaxValue.Location = new Point(1046, 35);
            lbStatMaxValue.Name = "lbStatMaxValue";
            lbStatMaxValue.Size = new Size(330, 70);
            lbStatMaxValue.TabIndex = 7;
            lbStatMaxValue.Text = "---";
            // 
            // plotViewLiveChart
            // 
            plotViewLiveChart.BackColor = Color.White;
            plotViewLiveChart.Location = new Point(6, 127);
            plotViewLiveChart.Name = "plotViewLiveChart";
            plotViewLiveChart.PanCursor = Cursors.Hand;
            plotViewLiveChart.Size = new Size(1383, 674);
            plotViewLiveChart.TabIndex = 1;
            plotViewLiveChart.ZoomHorizontalCursor = Cursors.SizeWE;
            plotViewLiveChart.ZoomRectangleCursor = Cursors.SizeNWSE;
            plotViewLiveChart.ZoomVerticalCursor = Cursors.SizeNS;
            // 
            // tpDatasheet
            // 
            tpDatasheet.BackColor = Color.FromArgb(244, 246, 249);
            tpDatasheet.Controls.Add(tboSearch);
            tpDatasheet.Controls.Add(lbSearch);
            tpDatasheet.Controls.Add(btnImportData);
            tpDatasheet.Controls.Add(btnExportdata);
            tpDatasheet.Controls.Add(label12);
            tpDatasheet.Controls.Add(dgvWeightsheet);
            tpDatasheet.Location = new Point(4, 44);
            tpDatasheet.Name = "tpDatasheet";
            tpDatasheet.Padding = new Padding(3);
            tpDatasheet.Size = new Size(1395, 792);
            tpDatasheet.TabIndex = 1;
            tpDatasheet.Text = "Data Sheet";
            // 
            // tboSearch
            // 
            tboSearch.BackColor = Color.White;
            tboSearch.BorderStyle = BorderStyle.FixedSingle;
            tboSearch.Font = new Font("Segoe UI", 10.8F);
            tboSearch.ForeColor = Color.FromArgb(30, 41, 59);
            tboSearch.Location = new Point(600, 23);
            tboSearch.Name = "tboSearch";
            tboSearch.Size = new Size(145, 31);
            tboSearch.TabIndex = 5;
            // 
            // lbSearch
            // 
            lbSearch.AutoSize = true;
            lbSearch.BackColor = Color.Transparent;
            lbSearch.Font = new Font("Segoe UI", 10.8F);
            lbSearch.ForeColor = Color.FromArgb(100, 116, 139);
            lbSearch.Location = new Point(460, 25);
            lbSearch.Name = "lbSearch";
            lbSearch.Size = new Size(142, 25);
            lbSearch.TabIndex = 4;
            lbSearch.Text = "Tìm kiếm nhanh:";
            // 
            // btnImportData
            // 
            btnImportData.BackColor = Color.FromArgb(34, 197, 94);
            btnImportData.FlatAppearance.BorderSize = 0;
            btnImportData.FlatStyle = FlatStyle.Flat;
            btnImportData.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            btnImportData.ForeColor = Color.White;
            btnImportData.Location = new Point(755, 13);
            btnImportData.Name = "btnImportData";
            btnImportData.Size = new Size(305, 53);
            btnImportData.TabIndex = 3;
            btnImportData.Text = "📥  Nhập dữ liệu (.xlsx / .csv)";
            btnImportData.UseVisualStyleBackColor = false;
            // 
            // btnExportdata
            // 
            btnExportdata.BackColor = Color.FromArgb(0, 159, 227);
            btnExportdata.FlatAppearance.BorderSize = 0;
            btnExportdata.FlatStyle = FlatStyle.Flat;
            btnExportdata.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            btnExportdata.ForeColor = Color.White;
            btnExportdata.Location = new Point(1070, 13);
            btnExportdata.Name = "btnExportdata";
            btnExportdata.Size = new Size(305, 53);
            btnExportdata.TabIndex = 2;
            btnExportdata.Text = "📤  Xuất báo cáo (.xlsx / .csv)";
            btnExportdata.UseVisualStyleBackColor = false;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.BackColor = Color.Transparent;
            label12.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            label12.ForeColor = Color.FromArgb(30, 41, 59);
            label12.Location = new Point(8, 20);
            label12.Name = "label12";
            label12.Size = new Size(437, 37);
            label12.TabIndex = 1;
            label12.Text = "Dữ liệu Đã lưu (SQLite Database)";
            // 
            // dgvWeightsheet
            // 
            dataGridViewCellStyle1.BackColor = Color.FromArgb(248, 250, 252);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(30, 41, 59);
            dgvWeightsheet.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvWeightsheet.BackgroundColor = Color.FromArgb(244, 246, 249);
            dgvWeightsheet.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(0, 159, 227);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(0, 128, 192);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvWeightsheet.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvWeightsheet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(30, 41, 59);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(236, 248, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(0, 128, 192);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvWeightsheet.DefaultCellStyle = dataGridViewCellStyle3;
            dgvWeightsheet.EnableHeadersVisualStyles = false;
            dgvWeightsheet.GridColor = Color.FromArgb(226, 232, 240);
            dgvWeightsheet.Location = new Point(17, 72);
            dgvWeightsheet.Name = "dgvWeightsheet";
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(226, 232, 240);
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(100, 116, 139);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(236, 248, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.FromArgb(0, 128, 192);
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvWeightsheet.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvWeightsheet.RowHeadersWidth = 51;
            dgvWeightsheet.Size = new Size(1358, 714);
            dgvWeightsheet.TabIndex = 0;
            // 
            // trayIcon
            // 
            trayIcon.ContextMenuStrip = trayContextMenu;
            trayIcon.Text = "Scale Data Collection";
            // 
            // trayContextMenu
            // 
            trayContextMenu.ImageScalingSize = new Size(20, 20);
            trayContextMenu.Items.AddRange(new ToolStripItem[] { trayMenuOpen, trayMenuSep, trayMenuExit });
            trayContextMenu.Name = "trayContextMenu";
            trayContextMenu.Size = new Size(174, 58);
            // 
            // trayMenuOpen
            // 
            trayMenuOpen.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            trayMenuOpen.Name = "trayMenuOpen";
            trayMenuOpen.Size = new Size(173, 24);
            trayMenuOpen.Text = "Mở ứng dụng";
            // 
            // trayMenuSep
            // 
            trayMenuSep.Name = "trayMenuSep";
            trayMenuSep.Size = new Size(170, 6);
            // 
            // trayMenuExit
            // 
            trayMenuExit.Name = "trayMenuExit";
            trayMenuExit.Size = new Size(173, 24);
            trayMenuExit.Text = "Thoát";
            // 
            // sqliteConnection1
            // 
            sqliteConnection1.DefaultTimeout = 30;
            // 
            // sqliteConnection2
            // 
            sqliteConnection2.DefaultTimeout = 30;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(0, 159, 227);
            ClientSize = new Size(1403, 840);
            Controls.Add(tcDashboard);
            Font = new Font("Segoe UI", 9F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1421, 887);
            Name = "Form1";
            Text = "tesa Scale Data Collection - ML204T";
            WindowState = FormWindowState.Maximized;
            tcDashboard.ResumeLayout(false);
            tpDashboard.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            panel1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tpAnalytics.ResumeLayout(false);
            pnlStats.ResumeLayout(false);
            tpDatasheet.ResumeLayout(false);
            tpDatasheet.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvWeightsheet).EndInit();
            trayContextMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl  tcDashboard;
        private TabPage     tpDashboard;
        private TabPage     tpDatasheet;
        private GroupBox    groupBox3;
        private GroupBox    groupBox2;
        private GroupBox    groupBox1;
        private TextBox     tboTcpport;
        private Label       label3;
        private TextBox     tboIpadd;
        private Label       label2;
        private Label       label1;
        private Label       lbStatusconnection;
        private Label       lbScaleModel;
        private Label       lbScaleSN;
        private Button      btnConnectIpadd;
        private Panel       panel1;
        private Label       lbLiveweight;
        private Button      btnPolling;
        private Label       label11;
        private TextBox     tboLocation;
        private Label       label10;
        private TextBox     tboTester;
        private Label       lblTester;
        private TextBox     tboSamplename;
        private Label       label9;
        private TextBox     tboBatch;
        private Label       label8;
        private TextBox     tboNat;
        private Label       label7;
        private Label       label6;
        private Button      btnImportData;
        private Button      btnExportdata;
        private Label       lbSearch;
        private TextBox     tboSearch;
        private Label       label12;
        private DataGridView dgvWeightsheet;
        private CheckBox    chkAutoPolling;
        private TabPage           tpAnalytics;
        private Panel             pnlStats;
        private Label             lbStatTodayCaption;
        private Label             lbStatTodayValue;
        private Label             lbStatBatchCaption;
        private Label             lbStatBatchValue;
        private Label             lbStatMinCaption;
        private Label             lbStatMinValue;
        private Label             lbStatMaxCaption;
        private Label             lbStatMaxValue;
        private OxyPlot.WindowsForms.PlotView plotViewLiveChart;
        private NotifyIcon  trayIcon;
        private ContextMenuStrip  trayContextMenu;
        private ToolStripMenuItem trayMenuOpen;
        private ToolStripSeparator trayMenuSep;
        private ToolStripMenuItem trayMenuExit;
        private Microsoft.Data.Sqlite.SqliteConnection sqliteConnection1;
        private Microsoft.Data.Sqlite.SqliteConnection sqliteConnection2;
    }
}
