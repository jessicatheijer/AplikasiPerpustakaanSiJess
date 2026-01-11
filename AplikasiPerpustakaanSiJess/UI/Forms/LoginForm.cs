using System;
using System.Drawing;
using System.Runtime.InteropServices; 
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Models;
using AplikasiPerpustakaanSiJess.Repositories;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class LoginForm : Form
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        TextBox txtUser, txtPass;
        Button btnLogin, btnSignup;

        public LoginForm()
        {
            Theme.ApplyForm(this);
            Text = "Aplikasi Perpustakaan SiJess - Login";
            Width = 520; Height = 360;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var card = new Panel { Width = 420, Height = 240, BackColor = Theme.Card, Left = 45, Top = 40 };
            Controls.Add(card);

            var title = new Label { Text = "SiJess Library", Font = Theme.H1, Left = 20, Top = 18, AutoSize = true };
            var sub = new Label { Text = "Login untuk masuk", Font = Theme.Body, ForeColor = Theme.Muted, Left = 20, Top = 52, AutoSize = true };


            txtUser = new TextBox { Left = 20, Top = 85, Width = 370 };
            txtPass = new TextBox { Left = 20, Top = 120, Width = 370, UseSystemPasswordChar = true };

            SendMessage(txtUser.Handle, EM_SETCUEBANNER, 0, "Username");
            SendMessage(txtPass.Handle, EM_SETCUEBANNER, 0, "Password");

            btnLogin = new Button { Text = "Login", Left = 20, Top = 165, Width = 180, Height = 34, BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSignup = new Button { Text = "Sign Up Anggota", Left = 210, Top = 165, Width = 180, Height = 34, BackColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnLogin.FlatAppearance.BorderSize = 0;
            btnSignup.FlatAppearance.BorderColor = Theme.Primary;

            card.Controls.Add(title);
            card.Controls.Add(sub);
            card.Controls.Add(txtUser);
            card.Controls.Add(txtPass);
            card.Controls.Add(btnLogin);
            card.Controls.Add(btnSignup);

            btnLogin.Click += (_, __) => DoLogin();
            btnSignup.Click += (_, __) => new SignupForm().ShowDialog();
        }

        private void DoLogin()
        {
            var u = txtUser.Text.Trim();
            var p = txtPass.Text;
            
            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                MessageBox.Show("Isi username dan password.");
                return;
            }

            var repo = new AuthRepository();
            var res = repo.Login(u, p);
            if (!res.ok)
            {
                MessageBox.Show(res.message);
                return;
            }

            Session.Username = u;
            Session.Role = res.role;
            Session.IdAnggota = res.idAnggota;

            Hide();

            if (Session.Role == "ADMIN")
                new AdminDashboardForm().ShowDialog();
            else
                new MemberDashboardForm().ShowDialog();

            Session.Clear();
            Show();
        }
    }
}