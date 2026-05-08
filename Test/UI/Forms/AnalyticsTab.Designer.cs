namespace Test.UI.Forms
{
    partial class AnalyticsTab
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderTimer?.Stop();
                _renderTimer?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.BackColor = System.Drawing.Color.FromArgb(244, 246, 249);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Name = "AnalyticsTab";
            this.Size = new System.Drawing.Size(1386, 705);
            this.ResumeLayout(false);
        }
    }
}
