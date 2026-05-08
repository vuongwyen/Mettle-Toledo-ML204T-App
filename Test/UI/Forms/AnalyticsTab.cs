using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Windows.Forms;
using ScottPlot;
using ScottPlot.WinForms;

namespace Test.UI.Forms
{
    /// <summary>
    /// Analytics Tab — Phase 2.
    /// Data flow: TCP thread → ConcurrentQueue → 100ms Timer → SignalPlot
    /// CPU target: < 5% at 10 Hz input.
    /// </summary>
    public partial class AnalyticsTab : UserControl
    {
        // ── Rolling Window ─────────────────────────────────────────────
        private const int MaxSamples = 600;   // 60 s @ 10 Hz
        private double[] _ringBuffer = new double[MaxSamples];
        private int _head = 0;           // index to write next sample
        private int _count = 0;          // current number of valid samples

        // ── Thread-safe ingestion queue ─────────────────────────────────
        private readonly ConcurrentQueue<double> _ingestQueue = new();

        // ── ScottPlot ──────────────────────────────────────────────────
        private FormsPlot _formsPlot = null!;
        private ScottPlot.Plottables.Signal _signal = null!;

        // ── 10 FPS UI render timer ──────────────────────────────────────
        private readonly System.Windows.Forms.Timer _renderTimer;

        // ── Stat labels ────────────────────────────────────────────────
        private System.Windows.Forms.Label _lbMin = null!, _lbMax = null!, _lbAvg = null!, _lbCount = null!;
        private double _runMin = double.MaxValue, _runMax = double.MinValue, _runSum;
        private long   _runCount;

        public AnalyticsTab()
        {
            InitializeComponent();
            BuildPlot();
            BuildStats();

            _renderTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _renderTimer.Tick += OnRenderTick;
            _renderTimer.Start();
        }

        // ── Public API (called from background TCP thread — thread-safe) ─
        /// <summary>Push a new weight value. Safe to call from any thread.</summary>
        public void PushSample(double weightGrams)
        {
            _ingestQueue.Enqueue(weightGrams);
        }

        public void ResetChart()
        {
            _ingestQueue.Clear();
            Array.Clear(_ringBuffer, 0, MaxSamples);
            _head = _count = 0;
            _runMin = double.MaxValue;
            _runMax = double.MinValue;
            _runSum = 0; _runCount = 0;
            _formsPlot.Refresh();
        }

        // ── Private helpers ─────────────────────────────────────────────
        private void OnRenderTick(object? sender, EventArgs e)
        {
            if (_ingestQueue.IsEmpty) return;   // nothing new — skip render, save CPU

            bool newData = false;
            while (_ingestQueue.TryDequeue(out double sample))
            {
                // PERF-02 FIX: Single write path — Array.Copy rolling window only.
                if (_count < MaxSamples)
                {
                    _ringBuffer[_count] = sample;
                    _count++;
                }
                else
                {
                    Array.Copy(_ringBuffer, 1, _ringBuffer, 0, MaxSamples - 1);
                    _ringBuffer[MaxSamples - 1] = sample;
                }

                _runSum += sample;
                _runCount++;
                if (sample < _runMin) _runMin = sample;
                if (sample > _runMax) _runMax = sample;

                newData = true;
            }

            if (!newData) return;

            // Update stats labels (already on UI thread via Forms.Timer)
            double avg = _runCount > 0 ? _runSum / _runCount : 0;
            _lbMin.Text   = $"MIN  {_runMin:F4} g";
            _lbMax.Text   = $"MAX  {_runMax:F4} g";
            _lbAvg.Text   = $"AVG  {avg:F4} g";
            _lbCount.Text = $"N  {_runCount}";

            _formsPlot.Refresh();
        }

        private void BuildPlot()
        {
            _formsPlot = new FormsPlot
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background
            };

            // Minimal, dark-accent style
            _formsPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#F4F6F9");
            _formsPlot.Plot.DataBackground.Color   = ScottPlot.Color.FromHex("#FFFFFF");

            _formsPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#E2E8F0");
            _formsPlot.Plot.Grid.MinorLineColor = ScottPlot.Color.FromHex("#F1F5F9");
            _formsPlot.Plot.Grid.XAxisStyle.IsVisible = false; // Tắt lưới dọc

            // Task 2.1: SignalPlot — Tối ưu nhất cho mảng dữ liệu cố định
            _signal = _formsPlot.Plot.Add.Signal(_ringBuffer);
            _signal.Color = ScottPlot.Color.FromHex("#009FE3"); // BrandBlue
            _signal.LineWidth = 2;

            _formsPlot.Plot.Title("Real-time Weight — MT-SICS Stream");
            _formsPlot.Plot.YLabel("Weight (g)");
            _formsPlot.Plot.XLabel("Samples");

            _formsPlot.Interaction.Disable(); // Tắt zoom/pan để ổn định CPU

            Controls.Add(_formsPlot);
        }

        private void BuildStats()
        {
            var panel = new System.Windows.Forms.Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = AppColors.Surface,
                Padding = new System.Windows.Forms.Padding(12, 8, 12, 8)
            };

            _lbMin   = MakeStatLabel("MIN  --", AppColors.StatusWarning);
            _lbMax   = MakeStatLabel("MAX  --", AppColors.BrandRed);
            _lbAvg   = MakeStatLabel("AVG  --", AppColors.BrandBlue);
            _lbCount = MakeStatLabel("N  0",    AppColors.TextSecondary);

            _lbMin.Left   = 12;
            _lbMax.Left   = 240;
            _lbAvg.Left   = 468;
            _lbCount.Left = 696;

            panel.Controls.AddRange(new System.Windows.Forms.Control[] { _lbMin, _lbMax, _lbAvg, _lbCount });

            var btnReset = new System.Windows.Forms.Button
            {
                Text      = "⟳  Reset",
                FlatStyle = FlatStyle.Flat,
                ForeColor = AppColors.TextSecondary,
                BackColor = AppColors.SurfaceAlt,
                Location  = new System.Drawing.Point(panel.Width - 110, 8),
                Size      = new System.Drawing.Size(96, 36),
                Anchor    = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top,
                Font      = new System.Drawing.Font("Segoe UI", 9F)
            };
            btnReset.FlatAppearance.BorderColor = AppColors.Border;
            btnReset.Click += (_, __) => ResetChart();
            panel.Controls.Add(btnReset);

            Controls.Add(panel);
        }

        private static System.Windows.Forms.Label MakeStatLabel(string text, System.Drawing.Color color) => new System.Windows.Forms.Label
        {
            Text      = text,
            ForeColor = color,
            Font      = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold),
            AutoSize  = true,
            Top       = 12
        };
    }
}
