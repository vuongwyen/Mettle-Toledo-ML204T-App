using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Test.Services;

namespace Test.UI.Forms
{
    public partial class HistoryTab : UserControl
    {
        private DatabaseService? _dbService;
        private List<ScaleRecord> _cache = new();
        private int _totalCount = 0;
        private string _currentFilter = "";
        private const int PageSize = 50;
        private int _cacheOffset = -1;

        public HistoryTab()
        {
            InitializeComponent();
            SetupGrid();
        }

        public void SetDatabaseService(DatabaseService service)
        {
            _dbService = service;
            RefreshData();
        }

        private void SetupGrid()
        {
            dgvHistory.VirtualMode = true;
            dgvHistory.AutoGenerateColumns = false;
            dgvHistory.AllowUserToAddRows = false;
            dgvHistory.RowHeadersVisible = false;
            dgvHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistory.BorderStyle = BorderStyle.None;
            dgvHistory.BackgroundColor = AppColors.Surface;
            dgvHistory.GridColor = AppColors.Border;
            
            // Columns
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColTime", HeaderText = "Thời gian", DataPropertyName = "Timestamp", Width = 150 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColWeight", HeaderText = "Khối lượng", DataPropertyName = "Weight", Width = 120 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColUnit", HeaderText = "ĐVT", DataPropertyName = "Unit", Width = 60 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColNat", HeaderText = "Mã NAT", DataPropertyName = "NatCode", Width = 120 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColBatch", HeaderText = "Lô hàng", DataPropertyName = "Batch", Width = 120 });
            dgvHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColSample", HeaderText = "Tên mẫu", DataPropertyName = "SampleName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            dgvHistory.CellValueNeeded += DgvHistory_CellValueNeeded;
            
            // Custom Styling for Header
            dgvHistory.EnableHeadersVisualStyles = false;
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = AppColors.SurfaceAlt;
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            dgvHistory.ColumnHeadersHeight = 48;
            
            dgvHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 244, 253);
            dgvHistory.DefaultCellStyle.SelectionForeColor = AppColors.BrandBlue;
            dgvHistory.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvHistory.RowTemplate.Height = 40;
        }

        public void RefreshData()
        {
            if (_dbService == null) return;
            
            _totalCount = _dbService.GetRecordCount(_currentFilter);
            _cacheOffset = -1;
            _cache.Clear();
            
            dgvHistory.RowCount = _totalCount;
            dgvHistory.Invalidate();
        }

        private void DgvHistory_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_dbService == null || e.RowIndex >= _totalCount) return;

            // Simple Caching Logic
            if (e.RowIndex < _cacheOffset || e.RowIndex >= _cacheOffset + PageSize)
            {
                _cacheOffset = (e.RowIndex / PageSize) * PageSize;
                _cache = _dbService.GetRecords(_cacheOffset, PageSize, _currentFilter);
            }

            int indexInCache = e.RowIndex - _cacheOffset;
            if (indexInCache < 0 || indexInCache >= _cache.Count) return;

            var record = _cache[indexInCache];
            e.Value = e.ColumnIndex switch
            {
                0 => record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                1 => record.Weight.ToString("F4"),
                2 => record.Unit,
                3 => record.NatCode,
                4 => record.Batch,
                5 => record.SampleName,
                _ => ""
            };
        }

        private void TboSearch_TextChanged(object sender, EventArgs e)
        {
            _currentFilter = tboSearch.Text.Trim();
            RefreshData();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            // Placeholder for Excel Export
            MessageBox.Show("Tính năng xuất Excel (Phase 4) đang được phát triển.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
