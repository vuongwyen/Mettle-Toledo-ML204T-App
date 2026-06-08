# Rule: Code Structure & Architecture
activation: always

## Description
Enforce clean code structure for WinForms + Guna UI2 projects.

## File Structure
ALWAYS organise project as:
- Forms/       → Form files (.cs + .Designer.cs)
- Controls/    → Reusable UserControls
- Helpers/     → ThemeHelper.cs, AnimationHelper.cs
- Models/      → Data models

## UI Code Placement
ALWAYS write UI setup code in the constructor, after InitializeComponent():
- SetupForm()  → size, borderless, backcolor, doublebuffered
- BuildUI()    → create and add all controls programmatically

DENY: writing complex UI logic inside Designer-generated .Designer.cs files.
DENY: mixing UI construction across random event handlers.

## Reusable Components
ALWAYS create UserControls for repeated UI patterns:
- Sidebar navigation → SidebarControl.cs
- Stat cards        → StatCardControl.cs
- Toast messages    → ToastControl.cs

UserControls MUST:
- Expose relevant events (e.g. MenuItemClicked)
- Have ApplyTheme(AppTheme) method
- Be Designer-safe

## Async Pattern for Buttons
ALWAYS implement loading state on action buttons:
1. Disable button + change text + dim color
2. await the async operation
3. Re-enable + restore text + restore ThemeHelper style

## Input Validation
ALWAYS use inline validation (not MessageBox):
- Error   : BorderColor=#FF6584, BorderThickness=2
- Default : BorderColor=ThemeHelper.Colors.Border, BorderThickness=1

## Naming Convention
- Forms    : PascalCase + "Form"   (e.g. LoginForm, DashboardForm)
- Controls : PascalCase + "Control" (e.g. SidebarControl)
- Helpers  : PascalCase + "Helper"  (e.g. ThemeHelper)
- Private fields : camelCase with prefix (e.g. btnLogin, pnlSidebar, txtUsername)
