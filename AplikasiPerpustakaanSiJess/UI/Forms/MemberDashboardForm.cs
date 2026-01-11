using System;
using System.Drawing;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Models;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class MemberDashboardForm : Form
    {
        Panel sidebar, content, topbar;
        Button btnCatalog, btnLoans, btnLogout;

        public MemberDashboardForm()
        {
            Theme.ApplyForm(this);
            Text = "Aplikasi Perpustakaan SiJess - Anggota";
            WindowState = FormWindowState.Maximized;
            BackColor = Theme.Bg;

            sidebar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.White };
            topbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };
            content = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg };

            Controls.Add(content);
            Controls.Add(topbar);
            Controls.Add(sidebar);

            var lbl = new Label { Text = "SiJess", Font = Theme.H1, Left = 16, Top = 14, AutoSize = true };
            var user = new Label { Text = $"Hi, {Session.Username}", Font = Theme.Body, ForeColor = Theme.Muted, Left = 120, Top = 18, AutoSize = true };
            topbar.Controls.Add(lbl);
            topbar.Controls.Add(user);

            btnCatalog = SideBtn("Katalog", 80);
            btnLoans = SideBtn("Peminjaman Saya", 130);
            btnLogout = SideBtn("Logout", 420);

            btnCatalog.Click += (_, __) => LoadPage(new MemberCatalogForm());
            btnLoans.Click += (_, __) => LoadPage(new MemberLoansForm());
            btnLogout.Click += (_, __) => Close();

            sidebar.Controls.Add(new Label { Text = "Menu Anggota", Font = Theme.H2, Left = 16, Top = 30, AutoSize = true });
            sidebar.Controls.Add(btnCatalog);
            sidebar.Controls.Add(btnLoans);
            sidebar.Controls.Add(btnLogout);

            LoadPage(new MemberCatalogForm());
        }

        Button SideBtn(string text, int top)
        {
            var b = new Button
            {
                Text = text,
                Left = 16,
                Top = top,
                Width = 188,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            b.FlatAppearance.BorderColor = Color.Gainsboro;
            return b;
        }

        void LoadPage(Form f)
        {
            content.Controls.Clear();
            f.TopLevel = false;
            f.FormBorderStyle = FormBorderStyle.None;
            f.Dock = DockStyle.Fill;
            content.Controls.Add(f);
            f.Show();
        }

        public void NavigateLoans()
        {
            LoadPage(new MemberLoansForm());
        }

    }
}
