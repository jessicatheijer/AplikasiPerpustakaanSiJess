using System;
using System.Drawing;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Models;
using AplikasiPerpustakaanSiJess.Repositories;
using AplikasiPerpustakaanSiJess.Services;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class MemberLoansForm : Form
    {
        DataGridView dgv;
        ComboBox cbStatus;
        readonly PeminjamanRepository pinjamRepo = new PeminjamanRepository();

        public MemberLoansForm()
        {
            Theme.ApplyForm(this);
            BackColor = Theme.Bg;

            Panel header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Theme.Bg };
            header.Controls.Add(new Label { Text = "Peminjaman Saya", Font = Theme.H1, Left = 18, Top = 16, AutoSize = true });

            cbStatus = new ComboBox { Left = 220, Top = 18, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStatus.Items.AddRange(new object[] { "SEMUA", "DIPINJAM", "KEMBALI", "HILANG" });
            cbStatus.SelectedIndex = 0;

            header.Controls.Add(cbStatus);
            this.Controls.Add(header);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgv.CellFormatting += Dgv_CellFormatting;

            this.Controls.Add(dgv);
            dgv.BringToFront();

            cbStatus.SelectedIndexChanged += (s, e) => LoadData();
            LoadData();
        }

        // Cell jd merah klo telat
        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name == "Terlambat" && e.Value != null)
            {
                string statusValue = e.Value.ToString();
                if (statusValue.Contains("hari"))
                {
                    e.CellStyle.ForeColor = Color.Red;
                    e.CellStyle.SelectionForeColor = Color.Red;
                    e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
            }
        }

        void LoadData()
        {
            if (!Session.IdAnggota.HasValue) return;

            var list = pinjamRepo.GetLoansByAnggota(Session.IdAnggota.Value, cbStatus.SelectedItem.ToString());

            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("Judul");
            table.Columns.Add("Tgl Pinjam");
            table.Columns.Add("Jatuh Tempo");
            table.Columns.Add("Status");
            table.Columns.Add("Terlambat"); 
            table.Columns.Add("Denda (Rp)");

            foreach (var x in list)
            {
                string telatStr = "-";
                long totalDenda = x.Denda;

                if (x.StatusItem == "DIPINJAM")
                {
                    int hariTerlambat = DendaService.TelatHari(x.TglJatuhTempo, DateTime.Now);

                    if (hariTerlambat > 0)
                    {
                        telatStr = hariTerlambat + " hari";
                        totalDenda = hariTerlambat * 10000;
                    }
                    else
                    {
                        telatStr = "Tidak";
                        totalDenda = 0;
                    }
                }

                table.Rows.Add(
                    x.Judul,
                    x.TglPinjam.ToString("dd MMM yyyy"),
                    x.TglJatuhTempo.ToString("dd MMM yyyy"),
                    x.StatusItem,
                    telatStr,
                    totalDenda.ToString("n0")
                );
            }

            dgv.DataSource = table;
        }
    }
}