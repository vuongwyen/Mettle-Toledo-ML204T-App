using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using MaterialSkin;
using MaterialSkin.Controls;

namespace Test.UI.Controls
{
    public partial class ConnectionLed : UserControl
    {
        private System.Windows.Forms.Timer _pulseTimer;
        private float _alpha = 1.0f;
        private bool _increasing = false;
        private Color _baseColor = AppColors.BrandRed;
        private bool _isConnected = false;
        private bool _isTransferring = false;

        public ConnectionLed()
        {
            this.Size = new Size(24, 24);
            this.DoubleBuffered = true;
            _pulseTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _pulseTimer.Tick += (s, e) => {
                if (_increasing) {
                    _alpha += 0.05f;
                    if (_alpha >= 1.0f) _increasing = false;
                } else {
                    _alpha -= 0.05f;
                    if (_alpha <= 0.3f) _increasing = true;
                }
                this.Invalidate();
            };
            _pulseTimer.Start();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsConnected
        {
            get => _isConnected;
            set {
                _isConnected = value;
                UpdateColor();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTransferring
        {
            get => _isTransferring;
            set {
                _isTransferring = value;
                UpdateColor();
            }
        }

        private void UpdateColor()
        {
            if (!_isConnected) _baseColor = AppColors.BrandRed;
            else if (_isTransferring) _baseColor = Color.FromArgb(39, 174, 96); // AccentGreen
            else _baseColor = Color.FromArgb(39, 174, 96); // Solid Green
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int diameter = Math.Min(Width, Height) - 4;
            var rect = new Rectangle(2, 2, diameter, diameter);

            // Shadow/Glow
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (var pthGrBrush = new PathGradientBrush(path))
                {
                    pthGrBrush.CenterColor = Color.FromArgb((int)(_alpha * 255), _baseColor);
                    pthGrBrush.SurroundColors = new[] { Color.Transparent };
                    e.Graphics.FillEllipse(pthGrBrush, rect);
                }
            }

            // Core LED
            int coreDim = diameter / 2;
            var coreRect = new Rectangle(Width / 2 - coreDim / 2, Height / 2 - coreDim / 2, coreDim, coreDim);
            using (var brush = new SolidBrush(_baseColor))
            {
                e.Graphics.FillEllipse(brush, coreRect);
            }
        }
    }
}
