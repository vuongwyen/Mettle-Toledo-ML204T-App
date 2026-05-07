using System.Drawing;

namespace Test
{
    /// <summary>
    /// tesa brand color palette — enterprise light theme.
    /// </summary>
    internal static class AppColors
    {
        // ── Brand core ────────────────────────────────────────────────
        public static readonly Color BrandRed       = Color.FromArgb(227, 6, 19);
        public static readonly Color BrandRedLight  = Color.FromArgb(255, 235, 235);
        public static readonly Color BrandBlue      = Color.FromArgb(0, 159, 227);
        public static readonly Color BrandBlueLight = Color.FromArgb(236, 248, 255);
        public static readonly Color BrandBlueDark  = Color.FromArgb(0, 128, 192);

        // ── Surfaces ──────────────────────────────────────────────────
        public static readonly Color Background  = Color.FromArgb(244, 246, 249);
        public static readonly Color Surface     = Color.White;
        public static readonly Color SurfaceAlt  = Color.FromArgb(248, 250, 252);
        public static readonly Color Border      = Color.FromArgb(226, 232, 240);

        // ── Text ──────────────────────────────────────────────────────
        public static readonly Color TextPrimary   = Color.FromArgb(30, 41, 59);
        public static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        public static readonly Color TextMuted     = Color.FromArgb(148, 163, 184);

        // ── Status ────────────────────────────────────────────────────
        public static readonly Color StatusConnected   = Color.FromArgb(22, 163, 74);
        public static readonly Color StatusConnectedBg = Color.FromArgb(240, 253, 244);
        public static readonly Color StatusWarning     = Color.FromArgb(217, 119, 6);
        public static readonly Color StatusWarningBg   = Color.FromArgb(255, 251, 235);
        public static readonly Color StatusIdle        = Color.FromArgb(226, 232, 240);
        public static readonly Color StatusIdleText    = Color.FromArgb(100, 116, 139);

        // ── Live weight ───────────────────────────────────────────────
        public static readonly Color WeightStable   = Color.FromArgb(22, 163, 74);
        public static readonly Color WeightUnstable = BrandRed;
        public static readonly Color PanelStable    = Color.FromArgb(240, 253, 244);
        public static readonly Color PanelUnstable  = BrandRedLight;
        public static readonly Color PanelIdle      = Surface;

        // ── Buttons ───────────────────────────────────────────────────
        public static readonly Color AccentGreen      = Color.FromArgb(39, 174, 96);
        public static readonly Color AccentGreenHover = Color.FromArgb(50, 200, 110);
        public static readonly Color AccentRed        = BrandRed;
        public static readonly Color AccentRedHover   = Color.FromArgb(200, 5, 15);
        public static readonly Color AccentOrange     = StatusWarning;

        // ── Analytics stat cards ──────────────────────────────────────
        public static readonly Color StatMin = StatusWarning;
        public static readonly Color StatMax = BrandRed;

        // ── Backwards-compat aliases ──────────────────────────────────
        public static readonly Color AccentBlue        = BrandBlue;
        public static readonly Color AccentBlueDark    = BrandBlueDark;
        public static readonly Color LivePanel         = PanelIdle;
        public static readonly Color TextPrimaryColor  = TextPrimary;
    }
}
