# Rule: Guna UI2 Controls Only
activation: always

## Description
Enforce Guna UI2 controls instead of default WinForms controls at all times.

## Rules

NEVER use these default WinForms controls:
- Button
- TextBox
- Panel
- ComboBox
- CheckBox
- RadioButton
- ProgressBar
- TabControl
- DataGridView
- PictureBox
- NumericUpDown
- TrackBar

ALWAYS replace with:
- Button        → Guna2Button
- TextBox       → Guna2TextBox
- Panel         → Guna2Panel
- ComboBox      → Guna2ComboBox
- CheckBox      → Guna2CheckBox
- RadioButton   → Guna2RadioButton
- ProgressBar   → Guna2ProgressBar
- TabControl    → Guna2TabControl
- DataGridView  → Guna2DataGridView
- PictureBox    → Guna2CirclePictureBox hoặc Guna2GradientPanel
- NumericUpDown → Guna2NumericUpDown
- TrackBar      → Guna2TrackBar

DENY: generating any code that instantiates default WinForms controls.
DENY: using MessageBox.Show() — dùng Guna2MessageDialog hoặc custom toast.

## Required Namespace
using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Suite;
