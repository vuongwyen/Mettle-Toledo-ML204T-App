# Rule: Form Design Conventions
activation: always

## Description
Every new WinForms Form must follow these structural and visual conventions.

## Required Setup (every Form)

ALWAYS apply to every new Form:
1. FormBorderStyle = FormBorderStyle.None
2. Add Guna2BorderlessForm component
3. DoubleBuffered = true
4. StartPosition = FormStartPosition.CenterScreen
5. BackColor = ThemeHelper.Colors.Background

ALWAYS add a custom title bar:
- Guna2Panel, Dock=Top, Height=36-40px
- Draggable via MouseDown + ReleaseCapture/SendMessage
- macOS-style close/minimize buttons (Guna2Button, 14x14, BorderRadius=7)
  · Close    : FillColor=#FF5F57
  · Minimize : FillColor=#FFBD2E

## Border Radius Standards
- Buttons   : 18px (pill)
- TextBoxes : 10px
- Panels    : 16px
- Dialogs   : 20-24px
- Badges    : 8px

## Shadow Convention
ALWAYS add ShadowDecoration to cards and important panels:
- Enabled = true
- Color   = Color.FromArgb(50, 0, 0, 0)
- Depth   = 18-20
- Angle   = 90

## Font
ALWAYS use Segoe UI:
- Title   : Segoe UI 18-22pt Bold
- Body    : Segoe UI 9.5pt Regular
- Caption : Segoe UI 8.5pt Regular

DENY: using any other font family.
DENY: leaving FormBorderStyle as anything other than None.
