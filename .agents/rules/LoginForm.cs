// ============================================================
// LoginForm.cs — Ví dụ Form đăng nhập với Guna UI2
// Đây là CODE-BEHIND (viết tay), không phụ thuộc Designer
// ============================================================

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using YourApp.Helpers;

namespace YourApp.Forms
{
    public partial class LoginForm : Form
    {
        // ── Win32 để drag borderless form ─────────────────────────
        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        [DllImport("user32.dll")] static extern int  SendMessage(IntPtr h, int msg, int w, int l);

        // ── Controls (khai báo tường minh, không dùng Designer) ───
        private Guna2Panel      pnlTitleBar, pnlMain, pnlCard;
        private Guna2Button     btnClose, btnMinimize, btnLogin;
        private Guna2TextBox    txtUsername, txtPassword;
        private Guna2CheckBox   chkRemember;
        private Guna2ToggleSwitch tglTheme;
        private Label           lblTitle, lblSub, lblForgot, lblThemeIcon;

        public LoginForm()
        {
            InitializeComponent(); // designer stub
            SetupForm();
            BuildUI();
        }

        // ── Form Setup ────────────────────────────────────────────
        private void SetupForm()
        {
            Text              = "Login";
            Size              = new Size(440, 580);
            FormBorderStyle   = FormBorderStyle.None;
            StartPosition     = FormStartPosition.CenterScreen;
            BackColor         = ThemeHelper.Colors.Background;
            DoubleBuffered    = true;

            // Guna2 Borderless component
            var borderless = new Guna2BorderlessForm();
            borderless.ContainerControl = this;
        }

        // ── Build UI programmatically ─────────────────────────────
        private void BuildUI()
        {
            // ── Title Bar ─────────────────────────────────────────
            pnlTitleBar = new Guna2Panel {
                Dock         = DockStyle.Top,
                Height       = 36,
                FillColor    = ThemeHelper.Colors.Background,
                BorderRadius = 0
            };
            pnlTitleBar.MouseDown += DragForm;

            btnClose = new Guna2Button {
                Size        = new Size(14, 14),
                Location    = new Point(pnlTitleBar.Width - 24, 11),
                BorderRadius = 7,
                FillColor   = Color.FromArgb(255, 95, 87),
                FillColor2  = Color.FromArgb(255, 59, 48),
                Text        = ""
            };
            btnClose.Click += (s, e) => Application.Exit();

            btnMinimize = new Guna2Button {
                Size        = new Size(14, 14),
                Location    = new Point(pnlTitleBar.Width - 46, 11),
                BorderRadius = 7,
                FillColor   = Color.FromArgb(255, 189, 46),
                FillColor2  = Color.FromArgb(255, 159, 0),
                Text        = ""
            };
            btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;

            pnlTitleBar.Controls.AddRange(new Control[] { btnMinimize, btnClose });

            // ── Main Card ─────────────────────────────────────────
            pnlCard = new Guna2Panel {
                Width        = 360,
                Height       = 480,
                BorderRadius = 24,
                FillColor    = ThemeHelper.Colors.Surface,
                FillColor2   = ThemeHelper.Colors.Surface2,
                GradientAngle = 160,
            };
            ThemeHelper.Apply(pnlCard, PanelStyle.Card);
            pnlCard.Location = new Point(
                (Width - pnlCard.Width) / 2,
                (Height - pnlCard.Height) / 2 + 10
            );

            // ── Logo / Title ───────────────────────────────────────
            lblTitle = new Label {
                Text      = "Chào mừng trở lại",
                Font      = ThemeHelper.Fonts.H2,
                ForeColor = ThemeHelper.Colors.TextPrimary,
                AutoSize  = true,
                Location  = new Point(32, 48)
            };

            lblSub = new Label {
                Text      = "Đăng nhập vào tài khoản của bạn",
                Font      = ThemeHelper.Fonts.Small,
                ForeColor = ThemeHelper.Colors.TextMuted,
                AutoSize  = true,
                Location  = new Point(32, 80)
            };

            // ── Username ──────────────────────────────────────────
            var lblUser = MakeLabel("Tên đăng nhập", 32, 132);
            txtUsername = new Guna2TextBox { Width = 296, Height = 44, Location = new Point(32, 154) };
            ThemeHelper.Apply(txtUsername, "Nhập username...");

            // ── Password ──────────────────────────────────────────
            var lblPass = MakeLabel("Mật khẩu", 32, 214);
            txtPassword = new Guna2TextBox {
                Width       = 296, Height = 44,
                Location    = new Point(32, 236),
                UseSystemPasswordChar = true
            };
            ThemeHelper.Apply(txtPassword, "Nhập mật khẩu...");

            // ── Remember Me ───────────────────────────────────────
            chkRemember = new Guna2CheckBox {
                Text          = "Ghi nhớ đăng nhập",
                Font          = ThemeHelper.Fonts.Body,
                ForeColor     = ThemeHelper.Colors.TextMuted,
                AutoSize      = true,
                Location      = new Point(32, 302),
                CheckedState  = { FillColor = ThemeHelper.Colors.Primary }
            };

            // ── Forgot Password ───────────────────────────────────
            lblForgot = new Label {
                Text      = "Quên mật khẩu?",
                Font      = ThemeHelper.Fonts.Small,
                ForeColor = ThemeHelper.Colors.Primary,
                AutoSize  = true,
                Cursor    = Cursors.Hand,
                Location  = new Point(202, 306)
            };

            // ── Login Button ──────────────────────────────────────
            btnLogin = new Guna2Button {
                Text     = "Đăng nhập",
                Width    = 296, Height = 48,
                Location = new Point(32, 348),
                Font     = new Font("Segoe UI", 10.5f, FontStyle.Bold)
            };
            ThemeHelper.Apply(btnLogin, BtnStyle.Primary);
            btnLogin.Click += BtnLogin_Click;

            // ── Divider + Social (optional) ───────────────────────
            var lblOr = new Label {
                Text      = "— atau masuk dengan —",
                Font      = ThemeHelper.Fonts.Caption,
                ForeColor = ThemeHelper.Colors.TextMuted,
                AutoSize  = true,
                Location  = new Point(90, 414)
            };

            // ── Assemble Card ─────────────────────────────────────
            pnlCard.Controls.AddRange(new Control[] {
                lblTitle, lblSub,
                lblUser, txtUsername,
                lblPass, txtPassword,
                chkRemember, lblForgot,
                btnLogin, lblOr
            });

            // ── Theme Toggle ──────────────────────────────────────
            tglTheme = new Guna2ToggleSwitch {
                Location     = new Point(Width - 60, 6),
                Size         = new Size(44, 20),
                CheckedState = {
                    FillColor  = ThemeHelper.Colors.Primary,
                    FillColor2 = ThemeHelper.Colors.Primary2
                }
            };
            tglTheme.CheckedChanged += (s, e) => {
                ThemeHelper.SwitchTheme(tglTheme.Checked ? AppTheme.Light : AppTheme.Dark);
                // Re-apply (hoặc restart form)
                BackColor = ThemeHelper.Colors.Background;
            };

            // ── Add to Form ───────────────────────────────────────
            Controls.Add(pnlTitleBar);
            Controls.Add(tglTheme);
            Controls.Add(pnlCard);
            pnlTitleBar.BringToFront();

            // Anchor resize
            pnlTitleBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }

        // ── Helpers ───────────────────────────────────────────────
        private Label MakeLabel(string text, int x, int y) => new Label {
            Text      = text,
            Font      = ThemeHelper.Fonts.Small,
            ForeColor = ThemeHelper.Colors.TextMuted,
            AutoSize  = true,
            Location  = new Point(x, y)
        };

        private void DragForm(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, 0x112, 0xF010, 0);
            }
        }

        // ── Login Logic ───────────────────────────────────────────
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            // Validate
            bool hasError = false;
            if (string.IsNullOrWhiteSpace(txtUsername.Text)) {
                SetInputError(txtUsername, true); hasError = true;
            } else SetInputError(txtUsername, false);

            if (string.IsNullOrWhiteSpace(txtPassword.Text)) {
                SetInputError(txtPassword, true); hasError = true;
            } else SetInputError(txtPassword, false);

            if (hasError) return;

            // Loading state
            btnLogin.Text    = "Đang xử lý...";
            btnLogin.Enabled = false;
            btnLogin.FillColor  = Color.FromArgb(80, 80, 100);
            btnLogin.FillColor2 = Color.FromArgb(80, 80, 100);

            await Task.Delay(1800); // replace với actual API call

            // Thành công → mở MainForm
            // new MainForm().Show();
            // this.Hide();

            // Reset (demo)
            btnLogin.Text    = "Đăng nhập";
            btnLogin.Enabled = true;
            ThemeHelper.Apply(btnLogin, BtnStyle.Primary);
        }

        private void SetInputError(Guna2TextBox tb, bool error)
        {
            tb.BorderColor     = error ? Color.FromArgb(255, 101, 132)
                                       : ThemeHelper.Colors.Border;
            tb.BorderThickness = error ? 2 : 1;
        }
    }
}
