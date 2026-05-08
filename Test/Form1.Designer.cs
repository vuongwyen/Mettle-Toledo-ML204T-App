namespace Test
{
    partial class Form1
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
            materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            tabPageDashboard = new TabPage();
            dashboardTab1 = new Test.UI.Forms.DashboardTab();
            tabPageAnalytics = new TabPage();
            this.analyticsTab1 = new Test.UI.Forms.AnalyticsTab();
            this.tabPageHistory = new System.Windows.Forms.TabPage();
            this.historyTab1 = new Test.UI.Forms.HistoryTab();
            this.materialTabControl1.SuspendLayout();
            this.tabPageDashboard.SuspendLayout();
            this.tabPageAnalytics.SuspendLayout();
            this.tabPageHistory.SuspendLayout();
            this.SuspendLayout();
            // 
            // materialTabControl1
            // 
            materialTabControl1.Controls.Add(tabPageDashboard);
            materialTabControl1.Controls.Add(tabPageAnalytics);
            materialTabControl1.Controls.Add(tabPageHistory);
            materialTabControl1.Depth = 0;
            materialTabControl1.Dock = DockStyle.Fill;
            materialTabControl1.Location = new Point(3, 85);
            materialTabControl1.Margin = new Padding(3, 4, 3, 4);
            materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            materialTabControl1.Multiline = true;
            materialTabControl1.Name = "materialTabControl1";
            materialTabControl1.SelectedIndex = 0;
            materialTabControl1.Size = new Size(1594, 978);
            materialTabControl1.TabIndex = 0;
            // 
            // tabPageDashboard
            // 
            tabPageDashboard.Controls.Add(dashboardTab1);
            tabPageDashboard.Location = new Point(4, 29);
            tabPageDashboard.Margin = new Padding(3, 4, 3, 4);
            tabPageDashboard.Name = "tabPageDashboard";
            tabPageDashboard.Padding = new Padding(3, 4, 3, 4);
            tabPageDashboard.Size = new Size(1586, 945);
            tabPageDashboard.TabIndex = 0;
            tabPageDashboard.Text = "Dashboard";
            tabPageDashboard.UseVisualStyleBackColor = true;
            // 
            // dashboardTab1
            // 
            dashboardTab1.BackColor = Color.FromArgb(244, 246, 249);
            dashboardTab1.Dock = DockStyle.Fill;
            dashboardTab1.Location = new Point(3, 4);
            dashboardTab1.Margin = new Padding(3, 4, 3, 4);
            dashboardTab1.Name = "dashboardTab1";
            dashboardTab1.Size = new Size(1580, 937);
            dashboardTab1.TabIndex = 0;
            // 
            // tabPageAnalytics
            // 
            tabPageAnalytics.Controls.Add(analyticsTab1);
            tabPageAnalytics.Location = new Point(4, 29);
            tabPageAnalytics.Margin = new Padding(3, 4, 3, 4);
            tabPageAnalytics.Name = "tabPageAnalytics";
            tabPageAnalytics.Padding = new Padding(3, 4, 3, 4);
            tabPageAnalytics.Size = new Size(1586, 945);
            tabPageAnalytics.TabIndex = 1;
            tabPageAnalytics.Text = "Analytics";
            tabPageAnalytics.UseVisualStyleBackColor = true;
            // 
            // analyticsTab1
            // 
            analyticsTab1.BackColor = Color.FromArgb(244, 246, 249);
            analyticsTab1.Dock = DockStyle.Fill;
            analyticsTab1.Location = new Point(3, 4);
            analyticsTab1.Margin = new Padding(3, 4, 3, 4);
            analyticsTab1.Name = "analyticsTab1";
            analyticsTab1.Size = new Size(1580, 937);
            analyticsTab1.TabIndex = 0;
            // 
            // tabPageHistory
            // 
            this.tabPageHistory.Controls.Add(this.historyTab1);
            this.tabPageHistory.Location = new System.Drawing.Point(4, 24);
            this.tabPageHistory.Name = "tabPageHistory";
            this.tabPageHistory.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHistory.Size = new System.Drawing.Size(1386, 705);
            this.tabPageHistory.TabIndex = 2;
            this.tabPageHistory.Text = "History";
            this.tabPageHistory.UseVisualStyleBackColor = true;
            // 
            // historyTab1
            // 
            this.historyTab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyTab1.Location = new System.Drawing.Point(3, 3);
            this.historyTab1.Name = "historyTab1";
            this.historyTab1.Size = new System.Drawing.Size(1380, 699);
            this.historyTab1.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1600, 1067);
            Controls.Add(materialTabControl1);
            DrawerTabControl = materialTabControl1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Padding = new Padding(3, 85, 3, 4);
            Text = "TESA - Scale Data Collection";
            materialTabControl1.ResumeLayout(false);
            this.tabPageDashboard.ResumeLayout(false);
            this.tabPageAnalytics.ResumeLayout(false);
            this.tabPageHistory.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private System.Windows.Forms.TabPage tabPageDashboard;
        private System.Windows.Forms.TabPage tabPageAnalytics;
        private System.Windows.Forms.TabPage tabPageHistory;
        public Test.UI.Forms.DashboardTab dashboardTab1;
        public Test.UI.Forms.AnalyticsTab analyticsTab1;
        public Test.UI.Forms.HistoryTab historyTab1;
    }
}
