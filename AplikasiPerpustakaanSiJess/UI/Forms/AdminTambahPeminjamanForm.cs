using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Repositories;
using AplikasiPerpustakaanSiJess.UI;
using AplikasiPerpustakaanSiJess.Models;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class AdminTambahPeminjamanForm : Form
    {
        ComboBox cbAnggota, cbTipe, cbKategori;
        TextBox txtSearch;
        DataGridView dgvKoleksi, dgvSelected;
        Button btnAdd, btnRemove, btnSave;

        readonly AnggotaRepository anggotaRepo = new AnggotaRepository();
        readonly KategoriRepository kategoriRepo = new KategoriRepository();
        readonly KoleksiRepository koleksiRepo = new KoleksiRepository();
        readonly PeminjamanRepository pinjamRepo = new PeminjamanRepository();

        public bool IsSaved { get; private set; } = false;

        public AdminTambahPeminjamanForm()
        {
            Theme.ApplyForm(this);
            Text = "Tambah Peminjaman";
            Width = 1100; Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Bg;

            var lbl = new Label { Text = "Buat Peminjaman Baru", Font = Theme.H1, Left = 18, Top = 12, AutoSize = true };
            Controls.Add(lbl);

            cbAnggota = new ComboBox { Left = 18, Top = 45, Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipe = new ComboBox { Left = 380, Top = 45, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cbKategori = new ComboBox { Left = 510, Top = 45, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            txtSearch = new TextBox { Left = 700, Top = 45, Width = 220 };
            var btnRefresh = new Button { Text = "Cari", Left = 930, Top = 43, Width = 70, Height = 28 };

            cbTipe.Items.AddRange(new object[] { "SEMUA", "BUKU", "CD", "JURNAL" });
            cbTipe.SelectedIndex = 0;

            Controls.Add(cbAnggota);
            Controls.Add(cbTipe);
            Controls.Add(cbKategori);
            Controls.Add(txtSearch);
            Controls.Add(btnRefresh);

            dgvKoleksi = new DataGridView
            {
                Left = 18,
                Top = 85,
                Width = 520,
                Height = 470,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };

            dgvSelected = new DataGridView
            {
                Left = 560,
                Top = 85,
                Width = 500,
                Height = 470,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };

            Controls.Add(dgvKoleksi);
            Controls.Add(dgvSelected);

            btnAdd = new Button { Text = ">> Tambah", Left = 18, Top = 565, Width = 120, Height = 32 };
            btnRemove = new Button { Text = "<< Hapus", Left = 150, Top = 565, Width = 120, Height = 32 };
            btnSave = new Button { Text = "SIMPAN PEMINJAMAN", Left = 560, Top = 565, Width = 500, Height = 36, BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.FlatAppearance.BorderSize = 0;

            Controls.Add(btnAdd);
            Controls.Add(btnRemove);
            Controls.Add(btnSave);

            btnRefresh.Click += (_, __) => LoadKoleksi();
            cbTipe.SelectedIndexChanged += (_, __) => LoadKoleksi();
            cbKategori.SelectedIndexChanged += (_, __) => LoadKoleksi();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadKoleksi(); };

            btnAdd.Click += (_, __) => AddSelected();
            btnRemove.Click += (_, __) => RemoveSelected();
            btnSave.Click += (_, __) => SavePinjam();

            LoadAnggota();
            LoadKategori();
            InitSelectedTable();
            LoadKoleksi();
        }

        void LoadAnggota()
        {
            cbAnggota.Items.Clear();
            var list = anggotaRepo.GetAll();
            foreach (var a in list)
            {
                cbAnggota.Items.Add(new { Id = a.IdAnggota, Name = $"{a.IdAnggota} - {a.NamaLengkap}" });
            }
            cbAnggota.DisplayMember = "Name";
            cbAnggota.ValueMember = "Id";
            if (cbAnggota.Items.Count > 0) cbAnggota.SelectedIndex = 0;
        }

        void LoadKategori()
        {
            cbKategori.Items.Clear();
            cbKategori.Items.Add(new { Id = (int?)null, Name = "Semua kategori" });

            var list = kategoriRepo.GetAll();
            foreach (var k in list)
                cbKategori.Items.Add(new { Id = (int?)k.IdKategori, Name = k.NamaKategori });

            cbKategori.DisplayMember = "Name";
            cbKategori.ValueMember = "Id";
            cbKategori.SelectedIndex = 0;
        }

        void LoadKoleksi()
        {
            dgvKoleksi.DataSource = null;

            int? katId = null;
            var sel = cbKategori.SelectedItem;
            if (sel != null)
            {
                var prop = sel.GetType().GetProperty("Id");
                katId = (int?)prop.GetValue(sel, null);
            }

            var items = koleksiRepo.GetKoleksiTersedia(txtSearch.Text.Trim(), cbTipe.SelectedItem.ToString(), katId);

            var dt = new DataTable();
            dt.Columns.Add("id_koleksi", typeof(int));
            dt.Columns.Add("judul");
            dt.Columns.Add("tipe");
            dt.Columns.Add("kategori");
            dt.Columns.Add("harga", typeof(int));

            foreach (var it in items)
                dt.Rows.Add(it.IdKoleksi, it.Judul, it.TipeKoleksi, it.NamaKategori, it.Harga);

            dgvKoleksi.DataSource = dt;
        }

        void InitSelectedTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("id_koleksi", typeof(int));
            dt.Columns.Add("judul");
            dt.Columns.Add("tipe");
            dt.Columns.Add("kategori");
            dgvSelected.DataSource = dt;
        }

        void AddSelected()
        {
            if (dgvKoleksi.CurrentRow == null) return;

            int id = Convert.ToInt32(dgvKoleksi.CurrentRow.Cells["id_koleksi"].Value);
            string judul = dgvKoleksi.CurrentRow.Cells["judul"].Value.ToString();
            string tipe = dgvKoleksi.CurrentRow.Cells["tipe"].Value.ToString();
            string kat = dgvKoleksi.CurrentRow.Cells["kategori"].Value.ToString();

            var dtSel = dgvSelected.DataSource as DataTable;
            if (dtSel.AsEnumerable().Any(x => x.Field<int>("id_koleksi") == id))
            {
                MessageBox.Show("Koleksi sudah ada di daftar pinjam.");
                return;
            }

            dtSel.Rows.Add(id, judul, tipe, kat);
        }

        void RemoveSelected()
        {
            if (dgvSelected.CurrentRow == null) return;
            dgvSelected.Rows.Remove(dgvSelected.CurrentRow);
        }

        void SavePinjam()
        {
            if (cbAnggota.SelectedItem == null)
            {
                MessageBox.Show("Pilih anggota dulu.");
                return;
            }

            var dtSel = dgvSelected.DataSource as DataTable;
            if (dtSel.Rows.Count == 0)
            {
                MessageBox.Show("Pilih minimal 1 koleksi.");
                return;
            }

            int idAnggota = (int)cbAnggota.SelectedItem.GetType().GetProperty("Id").GetValue(cbAnggota.SelectedItem, null);

            List<int> ids = new List<int>();
            foreach (DataRow row in dtSel.Rows)
                ids.Add(Convert.ToInt32(row["id_koleksi"]));

            try
            {
                int idPinjam = pinjamRepo.CreatePeminjaman(idAnggota, ids);
                MessageBox.Show("Berhasil membuat peminjaman. ID Pinjam: " + idPinjam);
                IsSaved = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal simpan peminjaman: " + ex.Message);
            }
        }
    }
}
