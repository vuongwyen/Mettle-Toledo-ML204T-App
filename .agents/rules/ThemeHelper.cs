// ============================================================
// ThemeHelper.cs — Guna UI2 WinForms Theme System
// Themes: Dark | Light | TesaBlue | VibrantRed | CrispWhite
// Đặt ở: Helpers/ThemeHelper.cs
// ============================================================
 
using System.Drawing;
using Guna.UI2.WinForms;
 
namespace YourApp.Helpers
{
    public enum AppTheme
    {
        Dark,
        Light,
        TesaBlue,
        VibrantRed,
        CrispWhite
    }
 
    public static class ThemeHelper
    {
        public static AppTheme Current { get; private set; } = AppTheme.Dark;
 
        // ── Dynamic Surface Colors (đổi theo theme) ───────────────
        public static class Colors
        {
            public static Color Background  => Resolve(AppColors.Background);
            public static Color Surface     => Resolve(AppColors.Surface);
            public static Color Surface2    => Resolve(AppColors.Surface2);
            public static Color Border      => Resolve(AppColors.Border);
            public static Color TextPrimary => Resolve(AppColors.TextPrimary);
            public static Color TextMuted   => Resolve(AppColors.TextMuted);
 
            // Accent colors — lấy từ theme hiện tại
            public static Color Primary     => Resolve(AppColors.Primary);
            public static Color Primary2    => Resolve(AppColors.Primary2);
            public static Color Accent      => Resolve(AppColors.Accent);
 
            // Semantic — cố định, không đổi theo theme
            public static readonly Color Success = Color.FromArgb( 67, 223, 173); // #43DFAD
            public static readonly Color Warning = Color.FromArgb(255, 200,  80); // #FFC850
            public static readonly Color Danger  = Color.FromArgb(255,  56,  96); // #FF3860
            public static readonly Color Info    = Color.FromArgb( 56, 189, 248); // #38BDF8
 
            private static Color Resolve(ColorSlot slot) => slot.Get(Current);
        }
 
        // ── Color Slot (5 theme values per color) ─────────────────
        private class ColorSlot
        {
            private readonly Color _dark, _light, _tesaBlue, _vibrantRed, _crispWhite;
            public ColorSlot(Color dark, Color light, Color tesaBlue, Color vibrantRed, Color crispWhite)
            {
                _dark = dark; _light = light;
                _tesaBlue = tesaBlue; _vibrantRed = vibrantRed; _crispWhite = crispWhite;
            }
            public Color Get(AppTheme t) => t switch {
                AppTheme.Dark       => _dark,
                AppTheme.Light      => _light,
                AppTheme.TesaBlue   => _tesaBlue,
                AppTheme.VibrantRed => _vibrantRed,
                AppTheme.CrispWhite => _crispWhite,
                _                   => _dark
            };
        }
 
        // ── App Color Definitions ─────────────────────────────────
        //                                   Dark                              Light                            TesaBlue                         VibrantRed                       CrispWhite
        private static class AppColors
        {
            public static readonly ColorSlot Background  = new(
                Color.FromArgb( 20,  20,  30),   // Dark    : near-black navy
                Color.FromArgb(245, 245, 250),   // Light   : off-white
                Color.FromArgb(  8,  44,  84),   // TesaBlue: deep navy
                Color.FromArgb( 28,   8,  10),   // VibrantRed: deep crimson bg
                Color.FromArgb(255, 255, 255)    // CrispWhite: pure white
            );
            public static readonly ColorSlot Surface = new(
                Color.FromArgb( 30,  30,  46),
                Color.White,
                Color.FromArgb( 13,  71, 136),   // TesaBlue: rich blue
                Color.FromArgb( 60,  10,  16),   // VibrantRed: dark red surface
                Color.FromArgb(248, 249, 252)    // CrispWhite: light grey-white
            );
            public static readonly ColorSlot Surface2 = new(
                Color.FromArgb( 36,  36,  56),
                Color.FromArgb(238, 238, 248),
                Color.FromArgb( 20,  90, 160),   // TesaBlue: medium blue
                Color.FromArgb( 90,  15,  24),   // VibrantRed
                Color.FromArgb(240, 242, 246)    // CrispWhite: subtle grey
            );
            public static readonly ColorSlot Border = new(
                Color.FromArgb( 46,  46,  78),
                Color.FromArgb(220, 220, 240),
                Color.FromArgb( 30, 110, 190),   // TesaBlue: blue border
                Color.FromArgb(180,  30,  50),   // VibrantRed: red border
                Color.FromArgb(210, 215, 225)    // CrispWhite: soft grey border
            );
            public static readonly ColorSlot TextPrimary = new(
                Color.White,
                Color.FromArgb( 26,  26,  46),
                Color.White,
                Color.White,
                Color.FromArgb( 22,  26,  38)    // CrispWhite: near-black text
            );
            public static readonly ColorSlot TextMuted = new(
                Color.FromArgb(138, 138, 154),
                Color.FromArgb(120, 120, 140),
                Color.FromArgb(160, 196, 230),   // TesaBlue: muted sky
                Color.FromArgb(220, 140, 150),   // VibrantRed: muted rose
                Color.FromArgb(140, 148, 164)    // CrispWhite: grey text
            );
 
            // ── Accent (Primary button / highlight color) ──────────
            public static readonly ColorSlot Primary = new(
                Color.FromArgb(108,  99, 255),   // Dark   : purple  #6C63FF
                Color.FromArgb(108,  99, 255),   // Light  : same purple
                Color.FromArgb(  0, 122, 255),   // TesaBlue: Apple-blue #007AFF
                Color.FromArgb(220,  38,  38),   // VibrantRed: bold red  #DC2626
                Color.FromArgb( 22, 119, 255)    // CrispWhite: vivid blue #1677FF
            );
            public static readonly ColorSlot Primary2 = new(
                Color.FromArgb( 67, 223, 173),   // Dark   : teal gradient end
                Color.FromArgb( 67, 223, 173),   // Light
                Color.FromArgb( 56, 189, 248),   // TesaBlue: sky #38BDF8
                Color.FromArgb(251,  80,  80),   // VibrantRed: coral
                Color.FromArgb( 99, 179, 255)    // CrispWhite: light blue
            );
            public static readonly ColorSlot Accent = new(
                Color.FromArgb( 67, 223, 173),
                Color.FromArgb( 67, 223, 173),
                Color.FromArgb( 56, 189, 248),
                Color.FromArgb(255, 145,  77),   // VibrantRed: orange accent
                Color.FromArgb( 22, 119, 255)
            );
        }
 
        // ── Font Helpers ──────────────────────────────────────────
        public static class Fonts
        {
            public static Font H1      => new Font("Segoe UI", 22f,  FontStyle.Bold);
            public static Font H2      => new Font("Segoe UI", 16f,  FontStyle.Bold);
            public static Font H3      => new Font("Segoe UI", 12f,  FontStyle.Bold);
            public static Font Body    => new Font("Segoe UI",  9.5f, FontStyle.Regular);
            public static Font Small   => new Font("Segoe UI",  8.5f, FontStyle.Regular);
            public static Font Caption => new Font("Segoe UI",  8f,   FontStyle.Regular);
        }
 
        // ── Apply Helpers ─────────────────────────────────────────
        public static void Apply(Guna2Button btn, BtnStyle style = BtnStyle.Primary)
        {
            btn.Font                = Fonts.Body;
            btn.BorderRadius        = 18;
            btn.ForeColor           = Color.White;
            btn.AnimationHoverSpeed = 0.07f;
            btn.AnimationClickSpeed = 0.07f;
 
            switch (style)
            {
                case BtnStyle.Primary:
                    btn.FillColor  = Colors.Primary;
                    btn.FillColor2 = Colors.Primary2;
                    btn.GradientAngle = 135;
                    btn.ShadowDecoration.Enabled = true;
                    btn.ShadowDecoration.Color   = Color.FromArgb(80, Colors.Primary);
                    btn.ShadowDecoration.Depth   = 16;
                    break;
                case BtnStyle.Secondary:
                    btn.FillColor       = Color.Transparent;
                    btn.ForeColor       = Colors.Primary;
                    btn.BorderColor     = Colors.Primary;
                    btn.BorderThickness = 2;
                    break;
                case BtnStyle.Danger:
                    btn.FillColor     = Color.FromArgb(220, 38, 38);
                    btn.FillColor2    = Color.FromArgb(185, 18, 18);
                    btn.GradientAngle = 135;
                    btn.ShadowDecoration.Enabled = true;
                    btn.ShadowDecoration.Color   = Color.FromArgb(80, 220, 38, 38);
                    btn.ShadowDecoration.Depth   = 14;
                    break;
                case BtnStyle.Ghost:
                    btn.FillColor = Color.Transparent;
                    btn.ForeColor = Colors.TextMuted;
                    break;
                case BtnStyle.Success:
                    btn.FillColor     = Colors.Success;
                    btn.FillColor2    = Color.FromArgb(16, 185, 129);
                    btn.GradientAngle = 135;
                    btn.ForeColor     = Color.White;
                    break;
            }
        }
 
        public static void Apply(Guna2TextBox tb, string placeholder = "")
        {
            tb.Font                 = Fonts.Body;
            tb.FillColor            = Colors.Surface2;
            tb.ForeColor            = Colors.TextPrimary;
            tb.BorderColor          = Colors.Border;
            tb.BorderRadius         = 10;
            tb.BorderThickness      = 1;
            tb.PlaceholderText      = placeholder;
            tb.PlaceholderForeColor = Colors.TextMuted;
            tb.Padding              = new Padding(8, 0, 8, 0);
        }
 
        public static void Apply(Guna2Panel panel, PanelStyle style = PanelStyle.Card)
        {
            switch (style)
            {
                case PanelStyle.Card:
                    panel.FillColor     = Colors.Surface;
                    panel.FillColor2    = Colors.Surface2;
                    panel.GradientAngle = 135;
                    panel.BorderRadius  = 16;
                    panel.ShadowDecoration.Enabled = true;
                    panel.ShadowDecoration.Color   = Color.FromArgb(50, 0, 0, 0);
                    panel.ShadowDecoration.Depth   = 20;
                    panel.ShadowDecoration.Angle   = 90;
                    break;
                case PanelStyle.Sidebar:
                    panel.FillColor    = Colors.Background;
                    panel.BorderRadius = 0;
                    panel.ShadowDecoration.Enabled = true;
                    panel.ShadowDecoration.Color   = Color.FromArgb(60, Colors.Primary);
                    panel.ShadowDecoration.Depth   = 20;
                    panel.ShadowDecoration.Angle   = 0;
                    break;
                case PanelStyle.Header:
                    panel.FillColor    = Colors.Surface;
                    panel.BorderRadius = 0;
                    break;
                case PanelStyle.Accent:
                    panel.FillColor     = Colors.Primary;
                    panel.FillColor2    = Colors.Primary2;
                    panel.GradientAngle = 135;
                    panel.BorderRadius  = 16;
                    panel.ShadowDecoration.Enabled = true;
                    panel.ShadowDecoration.Color   = Color.FromArgb(80, Colors.Primary);
                    panel.ShadowDecoration.Depth   = 18;
                    panel.ShadowDecoration.Angle   = 90;
                    break;
            }
        }
 
        // ── Theme Switch (gọi rồi re-apply toàn bộ controls) ──────
        public static void SwitchTheme(AppTheme theme)
        {
            Current = theme;
        }
 
        // ── Theme Meta (dùng cho UI picker) ───────────────────────
        public static (string Name, Color Preview1, Color Preview2)[] GetAllThemes() =>
        [
            ("Dark",        Color.FromArgb( 20,  20,  30), Color.FromArgb(108,  99, 255)),
            ("Light",       Color.FromArgb(245, 245, 250), Color.FromArgb(108,  99, 255)),
            ("Tesa Blue",   Color.FromArgb(  8,  44,  84), Color.FromArgb(  0, 122, 255)),
            ("Vibrant Red", Color.FromArgb( 28,   8,  10), Color.FromArgb(220,  38,  38)),
            ("Crisp White", Color.FromArgb(255, 255, 255), Color.FromArgb( 22, 119, 255)),
        ];
    }
 
    public enum BtnStyle   { Primary, Secondary, Danger, Ghost, Success }
    public enum PanelStyle { Card, Sidebar, Header, Accent }
}