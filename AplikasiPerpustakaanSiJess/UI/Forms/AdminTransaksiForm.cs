using System;
using System.Drawing;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Repositories;
using AplikasiPerpustakaanSiJess.Services;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class AdminTransaksiForm : Form
    {
        DataGridView dgv;
        ComboBox cbStatus;
        TextBox txtSearch;
        Button btnRefresh, btnTambah, btnEdit, btnHapus;

        readonly PeminjamanRepository repo = new PeminjamanRepository();

        public AdminTransaksiForm()
        {
            Theme.ApplyForm(this);
            BackColor = Theme.Bg;

            Controls.Add(new Label { Text = "Transaksi Peminjaman", Font = Theme.H1, Left = 18, Top = 12, AutoSize = true });

            btnTambah = new Button { Text = "TAMBAH", Left = 18, Top = 42, Width = 100, Height = 30, BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnTambah.FlatAppearance.BorderSize = 0;

            btnEdit = new Button { Text = "EDIT", Left = 128, Top = 42, Width = 80, Height = 30 };
            btnHapus = new Button { Text = "HAPUS", Left = 216, Top = 42, Width = 80, Height = 30 };

            cbStatus = new ComboBox { Left = 310, Top = 44, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStatus.Items.AddRange(new object[] { "SEMUA", "DIPINJAM", "KEMBALI", "HILANG" });
            cbStatus.SelectedIndex = 0;

            txtSearch = new TextBox { Left = 460, Top = 44, Width = 220 };
            btnRefresh = new Button { Text = "Cari/Refresh", Left = 690, Top = 42, Width = 110 };

            Controls.Add(btnTambah);
            Controls.Add(btnEdit);
            Controls.Add(btnHapus);
            Controls.Add(cbStatus);
            Controls.Add(txtSearch);
            Controls.Add(btnRefresh);

            dgv = new DataGridView
            {
                Left = 18,
                Top = 78,
                Width = 1150,
                Height = 560,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Controls.Add(dgv);

            dgv.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgv.IsCurrentCellDirty)
                    dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };


            btnRefresh.Click += (_, __) => LoadData();
            cbStatus.SelectedIndexChanged += (_, __) => LoadData();

            btnTambah.Click += (_, __) =>
            {
                using (var f = new AdminTambahPeminjamanForm())
                {
                    f.ShowDialog();
                    if (f.IsSaved) LoadData();
                }
            };

            btnEdit.Click += (_, __) => EditJatuhTempoSelected();
            btnHapus.Click += (_, __) => DeleteSelected();

            LoadData();
        }

        void LoadData()
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();
            dgv.AllowUserToAddRows = false;

            var list = repo.GetAllTransaksi(cbStatus.SelectedItem.ToString(), txtSearch.Text.Trim());

            dgv.Columns.Add("IdDetail", "IdDetail");
            dgv.Columns.Add("IdPinjam", "IdPinjam");
            dgv.Columns.Add("NamaAnggota", "Anggota");
            dgv.Columns.Add("Judul", "Koleksi");
            dgv.Columns.Add("TglPinjam", "Tgl Pinjam");
            dgv.Columns.Add("JatuhTempo", "Jatuh Tempo");
            dgv.Columns.Add("Denda", "Denda (Rp)");

            var colStatus = new DataGridViewComboBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                DataSource = new object[] { "DIPINJAM", "KEMBALI", "HILANG" }
            };
            dgv.Columns.Add(colStatus);

            foreach (dynamic x in list)
            {
                int row = dgv.Rows.Add(
                    x.IdDetail,
                    x.IdPinjam,
                    x.NamaAnggota,
                    x.Judul,
                    x.TglPinjam.ToString("yyyy-MM-dd"),
                    x.TglJatuhTempo.ToString("yyyy-MM-dd"),
                    x.Denda.ToString()
                );

                dgv.Rows[row].Cells["Status"].Value = x.StatusItem;
                dgv.Rows[row].Tag = x;
            }

            dgv.CellValueChanged -= Dgv_CellValueChanged;
            dgv.CellValueChanged += Dgv_CellValueChanged;
        }

        private void Dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                if (e.ColumnIndex < 0) return;

                if (dgv.Columns[e.ColumnIndex].Name != "Status") return;

                var row = dgv.Rows[e.RowIndex];
                if (row == null) return;

                if (row.Tag == null) return;

                dynamic x = row.Tag;

                object cellVal = row.Cells["Status"].Value;
                if (cellVal == null) return;

                string newStatus = cellVal.ToString();
                if (string.IsNullOrWhiteSpace(newStatus)) return;

                string oldStatus = (x.StatusItem == null) ? "" : x.StatusItem.ToString();

                if (newStatus == oldStatus) return;

                if (oldStatus != "DIPINJAM")
                {
                    MessageBox.Show("Status hanya boleh diubah dari DIPINJAM.");
                    row.Cells["Status"].Value = oldStatus; // balikin lagi
                    return;
                }

                if (newStatus == "KEMBALI")
                {
                    int denda = DendaService.DendaTerlambat((DateTime)x.TglJatuhTempo, DateTime.Now);
                    int telat = DendaService.TelatHari((DateTime)x.TglJatuhTempo, DateTime.Now);

                    MessageBox.Show(telat > 0
                        ? "Terlambat " + telat + " hari.\nDenda: Rp" + denda.ToString("n0")
                        : "Tidak terlambat. Denda: Rp0");

                    repo.SetKembali((int)x.IdDetail, (int)x.IdKoleksi, denda);
                }
                else if (newStatus == "HILANG")
                {
                    int harga = (x.Harga == null) ? 0 : (int)x.Harga;
                    int denda = DendaService.DendaHilang(harga);

                    MessageBox.Show("Koleksi hilang.\nHarga: Rp" + harga.ToString("n0")
                        + "\nDenda tetap: Rp" + DendaService.DendaHilangTetap.ToString("n0")
                        + "\nTotal: Rp" + denda.ToString("n0"));

                    repo.SetHilang((int)x.IdDetail, (int)x.IdKoleksi, denda);
                }
                else
                {
                    return;
                }

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error perubahan status: " + ex.Message);
                LoadData();
            }
        }


        void EditJatuhTempoSelected()
        {
            if (dgv.CurrentRow == null) return;

            dynamic x = dgv.CurrentRow.Tag;
            if (x == null) return;

            using (AdminEditJatuhTempoForm f = new AdminEditJatuhTempoForm(x.IdPinjam, x.TglPinjam, x.TglJatuhTempo))
            {
                f.ShowDialog();
                if (f.IsSaved) LoadData();
            }
        }

        void DeleteSelected()
        {
            if (dgv.CurrentRow == null) return;
            dynamic x = dgv.CurrentRow.Tag;
            if (x == null) return;

            var confirm = MessageBox.Show("Hapus transaksi detail ini?\n(ID Detail: " + x.IdDetail + ")", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            try
            {
                repo.DeleteDetail(x.IdDetail);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal hapus: " + ex.Message);
            }
        }
    }
}
