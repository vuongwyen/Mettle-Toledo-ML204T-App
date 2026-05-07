namespace Test
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tcDashboard = new TabControl();
            tpDashboard = new TabPage();
            groupBox3 = new GroupBox();
            btnPolling = new Button();
            label11 = new Label();
            panel1 = new Panel();
            lbLiveweight = new Label();
            groupBox2 = new GroupBox();
            tboLocation = new TextBox();
            label10 = new Label();
            tboSamplename = new TextBox();
            label9 = new Label();
            tboBatch = new TextBox();
            label8 = new Label();
            tboNat = new TextBox();
            label7 = new Label();
            label6 = new Label();
            groupBox1 = new GroupBox();
            lbStatusconnection = new Label();
            btnConnectIpadd = new Button();
            tboTcpport = new TextBox();
            label3 = new Label();
            tboIpadd = new TextBox();
            label2 = new Label();
            label1 = new Label();
            tpDatasheet = new TabPage();
            btnExportdata = new Button();
            label12 = new Label();
            dgvWeightsheet = new DataGridView();
            tcDashboard.SuspendLayout();
            tpDashboard.SuspendLayout();
            groupBox3.SuspendLayout();
            panel1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            tpDatasheet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvWeightsheet).BeginInit();
            SuspendLayout();
            // 
            // tcDashboard
            // 
            tcDashboard.Controls.Add(tpDashboard);
            tcDashboard.Controls.Add(tpDatasheet);
            tcDashboard.Dock = DockStyle.Fill;
            tcDashboard.Location = new Point(0, 0);
            tcDashboard.Name = "tcDashboard";
            tcDashboard.SelectedIndex = 0;
            tcDashboard.Size = new Size(1403, 840);
            tcDashboard.TabIndex = 0;
            // 
            // tpDashboard
            // 
            tpDashboard.Controls.Add(groupBox3);
            tpDashboard.Controls.Add(groupBox2);
            tpDashboard.Controls.Add(groupBox1);
            tpDashboard.Location = new Point(4, 29);
            tpDashboard.Name = "tpDashboard";
            tpDashboard.Padding = new Padding(3);
            tpDashboard.Size = new Size(1395, 807);
            tpDashboard.TabIndex = 0;
            tpDashboard.Text = "Dashboard";
            tpDashboard.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnPolling);
            groupBox3.Controls.Add(label11);
            groupBox3.Controls.Add(panel1);
            groupBox3.Location = new Point(447, 6);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(943, 793);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            // 
            // btnPolling
            // 
            btnPolling.Font = new Font("Segoe UI", 36F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnPolling.Location = new Point(21, 620);
            btnPolling.Name = "btnPolling";
            btnPolling.Size = new Size(907, 117);
            btnPolling.TabIndex = 6;
            btnPolling.Text = "Ch?t s? li?u (Polling)";
            btnPolling.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label11.Location = new Point(6, 23);
            label11.Name = "label11";
            label11.Size = new Size(338, 38);
            label11.TabIndex = 1;
            label11.Text = "1. C?u hình M?ng (TCP/IP)";
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.WindowFrame;
            panel1.Controls.Add(lbLiveweight);
            panel1.Location = new Point(21, 145);
            panel1.Name = "panel1";
            panel1.Size = new Size(907, 459);
            panel1.TabIndex = 0;
            // 
            // lbLiveweight
            // 
            lbLiveweight.AutoSize = true;
            lbLiveweight.Font = new Font("Bahnschrift Condensed", 72F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lbLiveweight.ForeColor = Color.LimeGreen;
            lbLiveweight.Location = new Point(525, 175);
            lbLiveweight.Name = "lbLiveweight";
            lbLiveweight.Size = new Size(364, 144);
            lbLiveweight.TabIndex = 0;
            lbLiveweight.Text = "0.0000g";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(tboLocation);
            groupBox2.Controls.Add(label10);
            groupBox2.Controls.Add(tboSamplename);
            groupBox2.Controls.Add(label9);
            groupBox2.Controls.Add(tboBatch);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(tboNat);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label6);
            groupBox2.Location = new Point(8, 357);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(433, 442);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            // 
            // tboLocation
            // 
            tboLocation.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tboLocation.Location = new Point(203, 208);
            tboLocation.Name = "tboLocation";
            tboLocation.Size = new Size(217, 31);
            tboLocation.TabIndex = 10;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label10.Location = new Point(203, 180);
            label10.Name = "label10";
            label10.Size = new Size(134, 25);
            label10.TabIndex = 9;
            label10.Text = "V? trí (Location)";
            // 
            // tboSamplename
            // 
            tboSamplename.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tboSamplename.Location = new Point(6, 332);
            tboSamplename.Name = "tboSamplename";
            tboSamplename.Size = new Size(421, 31);
            tboSamplename.TabIndex = 8;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label9.Location = new Point(6, 304);
            label9.Name = "label9";
            label9.Size = new Size(203, 25);
            label9.TabIndex = 7;
            label9.Text = "Tên m?u (Sample Name)";
            // 
            // tboBatch
            // 
            tboBatch.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tboBatch.Location = new Point(0, 208);
            tboBatch.Name = "tboBatch";
            tboBatch.Size = new Size(198, 31);
            tboBatch.TabIndex = 6;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label8.Location = new Point(0, 180);
            label8.Name = "label8";
            label8.Size = new Size(134, 25);
            label8.TabIndex = 5;
            label8.Text = "Lô hàng (Batch)";
            // 
            // tboNat
            // 
            tboNat.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tboNat.Location = new Point(0, 103);
            tboNat.Name = "tboNat";
            tboNat.Size = new Size(421, 31);
            tboNat.TabIndex = 4;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label7.Location = new Point(0, 75);
            label7.Name = "label7";
            label7.Size = new Size(75, 25);
            label7.TabIndex = 3;
            label7.Text = "Mã NAT";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label6.Location = new Point(6, 23);
            label6.Name = "label6";
            label6.Size = new Size(309, 38);
            label6.TabIndex = 1;
            label6.Text = "2. Thông tin tham chi?u";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lbStatusconnection);
            groupBox1.Controls.Add(btnConnectIpadd);
            groupBox1.Controls.Add(tboTcpport);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(tboIpadd);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(8, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(433, 345);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // lbStatusconnection
            // 
            lbStatusconnection.AutoSize = true;
            lbStatusconnection.BackColor = Color.Silver;
            lbStatusconnection.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lbStatusconnection.Location = new Point(53, 277);
            lbStatusconnection.Name = "lbStatusconnection";
            lbStatusconnection.Size = new Size(204, 25);
            lbStatusconnection.TabIndex = 6;
            lbStatusconnection.Text = "Status : DISCONNECTED";
            // 
            // btnConnectIpadd
            // 
            btnConnectIpadd.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnConnectIpadd.Location = new Point(53, 226);
            btnConnectIpadd.Name = "btnConnectIpadd";
            btnConnectIpadd.Size = new Size(288, 34);
            btnConnectIpadd.TabIndex = 5;
            btnConnectIpadd.Text = "K?t n?i (Connect)";
            btnConnectIpadd.UseVisualStyleBackColor = true;
            // 
            // tboTcpport
            // 
            tboTcpport.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tboTcpport.Location = new Point(6, 173);
            tboTcpport.Name = "tboTcpport";
            tboTcpport.Size = new Size(421, 31);
            tboTcpport.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.Location = new Point(6, 145);
            label3.Name = "label3";
            label3.Size = new Size(78, 25);
            label3.TabIndex = 3;
            label3.Text = "TCP Port";
            // 
            // tboIpadd
            // 
            tboIpadd.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tboIpadd.Location = new Point(6, 91);
            tboIpadd.Name = "tboIpadd";
            tboIpadd.Size = new Size(421, 31);
            tboIpadd.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(6, 63);
            label2.Name = "label2";
            label2.Size = new Size(97, 25);
            label2.TabIndex = 1;
            label2.Text = "IP Address";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(0, 12);
            label1.Name = "label1";
            label1.Size = new Size(338, 38);
            label1.TabIndex = 0;
            label1.Text = "1. C?u hình M?ng (TCP/IP)";
            // 
            // tpDatasheet
            // 
            tpDatasheet.Controls.Add(btnExportdata);
            tpDatasheet.Controls.Add(label12);
            tpDatasheet.Controls.Add(dgvWeightsheet);
            tpDatasheet.Location = new Point(4, 29);
            tpDatasheet.Name = "tpDatasheet";
            tpDatasheet.Padding = new Padding(3);
            tpDatasheet.Size = new Size(1395, 807);
            tpDatasheet.TabIndex = 1;
            tpDatasheet.Text = "Data Sheet";
            tpDatasheet.UseVisualStyleBackColor = true;
            // 
            // btnExportdata
            // 
            btnExportdata.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnExportdata.Location = new Point(1070, 13);
            btnExportdata.Name = "btnExportdata";
            btnExportdata.Size = new Size(305, 53);
            btnExportdata.TabIndex = 2;
            btnExportdata.Text = "Xu?t báo cáo (Export .xlsx, .csv)";
            btnExportdata.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 19.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label12.Location = new Point(8, 20);
            label12.Name = "label12";
            label12.Size = new Size(498, 46);
            label12.TabIndex = 1;
            label12.Text = "D? li?u Ðã luu (SQLite Database)";
            // 
            // dgvWeightsheet
            // 
            dgvWeightsheet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvWeightsheet.Location = new Point(17, 72);
            dgvWeightsheet.Name = "dgvWeightsheet";
            dgvWeightsheet.RowHeadersWidth = 51;
            dgvWeightsheet.Size = new Size(1358, 714);
            dgvWeightsheet.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1403, 840);
            Controls.Add(tcDashboard);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(1421, 887);
            MinimumSize = new Size(1421, 887);
            Name = "Form1";
            Text = "tesa Scale Data Collection - ML204T";
            tcDashboard.ResumeLayout(false);
            tpDashboard.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tpDatasheet.ResumeLayout(false);
            tpDatasheet.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvWeightsheet).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tcDashboard;
        private TabPage tpDashboard;
        private TabPage tpDatasheet;
        private GroupBox groupBox3;
        private GroupBox groupBox2;
        private GroupBox groupBox1;
        private TextBox tboTcpport;
        private Label label3;
        private TextBox tboIpadd;
        private Label label2;
        private Label label1;
        private Label lbStatusconnection;
        private Button btnConnectIpadd;
        private Panel panel1;
        private Label lbLiveweight;
        private Button btnPolling;
        private Label label11;
        private TextBox tboLocation;
        private Label label10;
        private TextBox tboSamplename;
        private Label label9;
        private TextBox tboBatch;
        private Label label8;
        private TextBox tboNat;
        private Label label7;
        private Label label6;
        private Button btnExportdata;
        private Label label12;
        private DataGridView dgvWeightsheet;
    }
}

