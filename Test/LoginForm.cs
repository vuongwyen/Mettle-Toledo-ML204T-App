using System;
using System.Drawing;
using System.Windows.Forms;

namespace Test
{
    public class LoginForm : Form
    {
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblMessage;

        public bool IsAuthenticated { get; private set; } = false;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Đăng nhập Quản trị (Admin)";
            this.Size = new Size(350, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            var lblTitle = new Label
            {
                Text = "Nhập mật khẩu Admin:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            txtPassword = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(290, 25),
                Font = new Font("Segoe UI", 10F),
                PasswordChar = '•'
            };
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnLogin.PerformClick(); };

            lblMessage = new Label
            {
                Text = "",
                Location = new Point(20, 80),
                AutoSize = true,
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9F)
            };

            btnLogin = new Button
            {
                Text = "Đăng nhập",
                Location = new Point(130, 110),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(0, 159, 227),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnCancel = new Button
            {
                Text = "Hủy",
                Location = new Point(230, 110),
                Size = new Size(80, 30),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblMessage);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnCancel);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string input = txtPassword.Text;
            string correctPass = AppConfig.Load().AdminPassword;

            if (input == correctPass)
            {
                IsAuthenticated = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblMessage.Text = "Mật khẩu không đúng!";
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
