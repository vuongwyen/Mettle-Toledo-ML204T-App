# Rule: Theme & Colors Convention
activation: always

## Description
Enforce consistent color usage via ThemeHelper — never hardcode colors in logic.

## Rules

ALWAYS use ThemeHelper.Colors.* for colors:
- Background  → ThemeHelper.Colors.Background
- Surface     → ThemeHelper.Colors.Surface
- Primary     → ThemeHelper.Colors.Primary
- Text        → ThemeHelper.Colors.TextPrimary
- Muted text  → ThemeHelper.Colors.TextMuted
- Border      → ThemeHelper.Colors.Border

DENY: hardcoding Color.FromArgb() or Color hex values outside of ThemeHelper.cs itself.

## Dark Theme Values (reference only — use ThemeHelper)
- Background : Color.FromArgb(20,  20,  30)
- Surface    : Color.FromArgb(30,  30,  46)
- Primary    : Color.FromArgb(108, 99,  255)  #6C63FF
- Accent     : Color.FromArgb(67,  223, 173)  #43DFAD
- Danger     : Color.FromArgb(255, 101, 132)  #FF6584
- TextMuted  : Color.FromArgb(138, 138, 154)

## Gradient Convention
- Primary button : FillColor=Primary  → FillColor2=Accent, GradientAngle=135
- Danger button  : FillColor=#FF6584 → FillColor2=#FF3860, GradientAngle=135
- Card panel     : FillColor=Surface → FillColor2=Surface2, GradientAngle=160

## Apply Helper
ALWAYS call ThemeHelper.Apply(control, style) when creating controls:
- ThemeHelper.Apply(btn, BtnStyle.Primary)
- ThemeHelper.Apply(tb, "placeholder text")
- ThemeHelper.Apply(panel, PanelStyle.Card)
