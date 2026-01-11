using System.Drawing;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Models;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class AdminDashboardForm : Form
    {
        Panel sidebar, content, topbar;
        Button btnMaster, btnTransaksi, btnLaporan, btnLogout;

        public AdminDashboardForm()
        {
            Theme.ApplyForm(this);
            Text = "Aplikasi Perpustakaan SiJess - Admin";
            WindowState = FormWindowState.Maximized;
            BackColor = Theme.Bg;

            sidebar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.White };
            topbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };
            content = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg };

            Controls.Add(content);
            Controls.Add(topbar);
            Controls.Add(sidebar);

            topbar.Controls.Add(new Label { Text = "SiJess Admin", Font = Theme.H1, Left = 16, Top = 14, AutoSize = true });
            topbar.Controls.Add(new Label { Text = $"Login: {Session.Username}", Font = Theme.Body, ForeColor = Theme.Muted, Left = 160, Top = 18, AutoSize = true });

            sidebar.Controls.Add(new Label { Text = "Menu Admin", Font = Theme.H2, Left = 16, Top = 30, AutoSize = true });

            btnMaster = SideBtn("Master Data", 80);
            btnTransaksi = SideBtn("Transaksi", 130);
            btnLaporan = SideBtn("Laporan", 180);
            btnLogout = SideBtn("Logout", 420);

            btnMaster.Click += (_, __) => LoadPage(new AdminMasterDataForm());
            btnTransaksi.Click += (_, __) => LoadPage(new AdminTransaksiForm());
            btnLaporan.Click += (_, __) => LoadPage(new AdminLaporanForm());
            btnLogout.Click += (_, __) => Close();

            sidebar.Controls.Add(btnMaster);
            sidebar.Controls.Add(btnTransaksi);
            sidebar.Controls.Add(btnLaporan);
            sidebar.Controls.Add(btnLogout);

            LoadPage(new AdminMasterDataForm());
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
    }
}
