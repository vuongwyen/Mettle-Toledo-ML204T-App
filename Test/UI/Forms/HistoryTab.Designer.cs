namespace Test.UI.Forms
{
    partial class HistoryTab
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
            this.tboSearch = new MaterialSkin.Controls.MaterialTextBox();
            this.dgvHistory = new System.Windows.Forms.DataGridView();
            this.btnExport = new MaterialSkin.Controls.MaterialButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // tboSearch
            // 
            this.tboSearch.AnimateReadOnly = false;
            this.tboSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tboSearch.Depth = 0;
            this.tboSearch.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tboSearch.Hint = "Tìm kiếm theo Mã NAT, Lô hoặc Tên mẫu...";
            this.tboSearch.LeadingIcon = null;
            this.tboSearch.Location = new System.Drawing.Point(15, 15);
            this.tboSearch.MaxLength = 50;
            this.tboSearch.MouseState = MaterialSkin.MouseState.OUT;
            this.tboSearch.Multiline = false;
            this.tboSearch.Name = "tboSearch";
            this.tboSearch.Size = new System.Drawing.Size(1000, 50);
            this.tboSearch.TabIndex = 0;
            this.tboSearch.Text = "";
            this.tboSearch.TrailingIcon = null;
            this.tboSearch.TextChanged += new System.EventHandler(this.TboSearch_TextChanged);
            // 
            // dgvHistory
            // 
            this.dgvHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHistory.Location = new System.Drawing.Point(15, 80);
            this.dgvHistory.Name = "dgvHistory";
            this.dgvHistory.Size = new System.Drawing.Size(1355, 610);
            this.dgvHistory.TabIndex = 1;
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExport.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnExport.Depth = 0;
            this.btnExport.HighEmphasis = true;
            this.btnExport.Icon = null;
            this.btnExport.Location = new System.Drawing.Point(1250, 22);
            this.btnExport.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnExport.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnExport.Name = "btnExport";
            this.btnExport.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnExport.Size = new System.Drawing.Size(120, 36);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "XUẤT EXCEL";
            this.btnExport.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnExport.UseAccentColor = false;
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // HistoryTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(249)))));
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.dgvHistory);
            this.Controls.Add(this.tboSearch);
            this.Name = "HistoryTab";
            this.Size = new System.Drawing.Size(1386, 705);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private MaterialSkin.Controls.MaterialTextBox tboSearch;
        private System.Windows.Forms.DataGridView dgvHistory;
        private MaterialSkin.Controls.MaterialButton btnExport;
    }
}
