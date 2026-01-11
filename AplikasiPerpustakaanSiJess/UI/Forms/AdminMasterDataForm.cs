using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Models;
using AplikasiPerpustakaanSiJess.Repositories;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class AdminMasterDataForm : Form
    {
        TabControl tabs;

        // --- TAB ANGGOTA ---
        DataGridView dgvAnggota;
        TextBox aNama, aNik, aAlamat, aTelp;
        ComboBox aJK;
        Button aAdd, aUpd, aDel, aPrint;

        // --- TAB KATEGORI ---
        DataGridView dgvKategori;
        TextBox kNama;
        Button kAdd, kUpd, kDel;

        // --- TAB KOLEKSI ---
        DataGridView dgvKoleksi;
        TextBox cJudul, cHarga;
        ComboBox cKategori, cStatus;
        string currentKoleksiType = "BUKU"; // Penanda Tipe Aktif (BUKU/CD/JURNAL)

        // Panel detail spesifik (Kanan)
        Panel panelBuku, panelCd, panelJurnal;

        // Buku fields
        TextBox bIsbn, bPenulis, bPenerbit, bTahun, bStok;
        // CD fields
        TextBox cdAlbum, cdArtis, cdDurasi;
        ComboBox cdFormat;
        // Jurnal fields
        TextBox jIssn, jVolume, jNomor, jTahun, jPenerbit;

        Button cUploadFoto, cClearFoto;
        PictureBox picPreview;
        Label lblFotoInfo;
        string selectedFotoPath = null; // relative path yg akan disimpan


        Button cAdd, cUpd, cDel;
        Button btnViewBuku, btnViewCd, btnViewJurnal;

        // Repository
        readonly AnggotaRepository anggotaRepo = new AnggotaRepository();
        readonly KategoriRepository kategoriRepo = new KategoriRepository();
        readonly KoleksiRepository koleksiRepo = new KoleksiRepository();

        public AdminMasterDataForm()
        {
            Theme.ApplyForm(this);
            Text = "Master Data - SiJess Library";
            BackColor = Theme.Bg;
            ClientSize = new Size(1150, 720);
            StartPosition = FormStartPosition.CenterScreen;

            Controls.Add(new Label { Text = "Master Data", Font = Theme.H1, Left = 18, Top = 12, AutoSize = true });

            tabs = new TabControl { Left = 18, Top = 44, Width = 1100, Height = 640 };
            Controls.Add(tabs);

            BuildTabAnggota();
            BuildTabKategori();
            BuildTabKoleksi();

            // Load Awal
            LoadAnggota();
            LoadKategori();
            LoadKategoriCombo();
            SwitchKoleksiView("BUKU"); // Set default ke tampilan Buku
        }

        #region TAB ANGGOTA
        void BuildTabAnggota()
        {
            TabPage tab = new TabPage("Anggota");
            tabs.TabPages.Add(tab);

            dgvAnggota = new DataGridView
            {
                Left = 10,
                Top = 10,
                Width = 650,
                Height = 550,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                AllowUserToAddRows = false
            };
            tab.Controls.Add(dgvAnggota);
            dgvAnggota.SelectionChanged += (s, e) => FillAnggotaFromGrid();

            int x = 680, y = 20;
            tab.Controls.Add(new Label { Text = "Nama Lengkap", Left = x, Top = y, AutoSize = true });
            aNama = new TextBox { Left = x, Top = y + 18, Width = 360 }; y += 55;

            tab.Controls.Add(new Label { Text = "NIK/NIS (opsional)", Left = x, Top = y, AutoSize = true });
            aNik = new TextBox { Left = x, Top = y + 18, Width = 360 }; y += 55;

            tab.Controls.Add(new Label { Text = "Jenis Kelamin", Left = x, Top = y, AutoSize = true });
            aJK = new ComboBox { Left = x, Top = y + 18, Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
            aJK.Items.AddRange(new object[] { "Laki-laki", "Perempuan" });
            aJK.SelectedIndex = 0; y += 55;

            tab.Controls.Add(new Label { Text = "Alamat", Left = x, Top = y, AutoSize = true });
            aAlamat = new TextBox { Left = x, Top = y + 18, Width = 360 }; y += 55;

            tab.Controls.Add(new Label { Text = "No Telp", Left = x, Top = y, AutoSize = true });
            aTelp = new TextBox { Left = x, Top = y + 18, Width = 360 }; y += 60;

            aAdd = new Button { Text = "Tambah", Left = x, Top = y, Width = 110, Height = 32 };
            aUpd = new Button { Text = "Update", Left = x + 125, Top = y, Width = 110, Height = 32 };
            aDel = new Button { Text = "Hapus", Left = x + 250, Top = y, Width = 110, Height = 32 };
            y += 45;

            aPrint = new Button { Text = "CETAK ID CARD", Left = x, Top = y, Width = 360, Height = 35, BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            tab.Controls.Add(aNama); tab.Controls.Add(aNik); tab.Controls.Add(aJK); tab.Controls.Add(aAlamat); tab.Controls.Add(aTelp);
            tab.Controls.Add(aAdd); tab.Controls.Add(aUpd); tab.Controls.Add(aDel); tab.Controls.Add(aPrint);

            aAdd.Click += (s, e) => {
                try
                {
                    if (string.IsNullOrWhiteSpace(aNama.Text)) throw new Exception("Nama wajib diisi.");
                    var a = new Anggota { NamaLengkap = aNama.Text.Trim(), NikOrNis = aNik.Text.Trim(), JenisKelamin = aJK.Text, Alamat = aAlamat.Text.Trim(), NoTelp = aTelp.Text.Trim() };
                    string msg;
                    if (anggotaRepo.TryAddAnggota(a, out msg)) { LoadAnggota(); ClearAnggota(); }
                    MessageBox.Show(msg);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            };

            aUpd.Click += (s, e) => {
                try
                {
                    if (dgvAnggota.CurrentRow == null) return;
                    var a = dgvAnggota.CurrentRow.DataBoundItem as Anggota;
                    a.NamaLengkap = aNama.Text; a.NikOrNis = aNik.Text; a.JenisKelamin = aJK.Text; a.Alamat = aAlamat.Text; a.NoTelp = aTelp.Text;
                    anggotaRepo.Update(a); LoadAnggota(); MessageBox.Show("Berhasil update.");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            };

            aDel.Click += (s, e) => {
                try
                {
                    if (dgvAnggota.CurrentRow == null) return;
                    var a = dgvAnggota.CurrentRow.DataBoundItem as Anggota;
                    if (MessageBox.Show("Hapus anggota ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        anggotaRepo.Delete(a.IdAnggota); LoadAnggota(); ClearAnggota();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            };


            aPrint.Click += (_, __) => PrintIdCard();
        }

        void LoadAnggota() { dgvAnggota.DataSource = null; dgvAnggota.DataSource = anggotaRepo.GetAll(); if (dgvAnggota.Columns["IdAnggota"] != null) dgvAnggota.Columns["IdAnggota"].Visible = false; }
        void FillAnggotaFromGrid() { if (dgvAnggota.CurrentRow == null) return; var a = dgvAnggota.CurrentRow.DataBoundItem as Anggota; aNama.Text = a.NamaLengkap; aNik.Text = a.NikOrNis; aJK.Text = a.JenisKelamin; aAlamat.Text = a.Alamat; aTelp.Text = a.NoTelp; }
        void ClearAnggota() { aNama.Clear(); aNik.Clear(); aAlamat.Clear(); aTelp.Clear(); }
        #endregion

        #region TAB KATEGORI
        void BuildTabKategori()
        {
            TabPage tab = new TabPage("Kategori");
            tabs.TabPages.Add(tab);
            dgvKategori = new DataGridView { Left = 10, Top = 10, Width = 650, Height = 550, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, BackgroundColor = Color.White, RowHeadersVisible = false, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            tab.Controls.Add(dgvKategori);
            dgvKategori.SelectionChanged += (s, e) => { if (dgvKategori.CurrentRow != null) kNama.Text = ((Kategori)dgvKategori.CurrentRow.DataBoundItem).NamaKategori; };

            kNama = new TextBox { Left = 680, Top = 38, Width = 360 };
            tab.Controls.Add(new Label { Text = "Nama Kategori", Left = 680, Top = 20, AutoSize = true });
            tab.Controls.Add(kNama);

            kAdd = new Button { Text = "Tambah", Left = 680, Top = 80, Width = 110, Height = 32 };
            kUpd = new Button { Text = "Edit", Left = 805, Top = 80, Width = 110, Height = 32 };
            kDel = new Button { Text = "Hapus", Left = 930, Top = 80, Width = 110, Height = 32 };
            tab.Controls.Add(kAdd); tab.Controls.Add(kUpd); tab.Controls.Add(kDel);

            kAdd.Click += (s, e) => { try { kategoriRepo.Add(kNama.Text); LoadKategori(); kNama.Clear(); LoadKategoriCombo(); } catch (Exception ex) { MessageBox.Show(ex.Message); } };
            kUpd.Click += (s, e) => { try { if (dgvKategori.CurrentRow == null) return; var k = (Kategori)dgvKategori.CurrentRow.DataBoundItem; kategoriRepo.Update(k.IdKategori, kNama.Text); LoadKategori(); LoadKategoriCombo(); } catch (Exception ex) { MessageBox.Show(ex.Message); } };
            kDel.Click += (s, e) => { try { if (dgvKategori.CurrentRow == null) return; var k = (Kategori)dgvKategori.CurrentRow.DataBoundItem; kategoriRepo.Delete(k.IdKategori); LoadKategori(); kNama.Clear(); LoadKategoriCombo(); } catch (Exception ex) { MessageBox.Show(ex.Message); } };
        }
        void LoadKategori() { dgvKategori.DataSource = null; dgvKategori.DataSource = kategoriRepo.GetAll(); if (dgvKategori.Columns["IdKategori"] != null) dgvKategori.Columns["IdKategori"].Visible = false; }
        #endregion

        #region TAB KOLEKSI (LOGIKA TERPISAH BUKU/CD/JURNAL)
        void BuildTabKoleksi()
        {
            TabPage tab = new TabPage("Koleksi");
            tabs.TabPages.Add(tab);

            // 1. Navigasi Tipe (Switch)
            btnViewBuku = new Button { Text = "BUKU", Left = 10, Top = 10, Width = 100, Height = 35, FlatStyle = FlatStyle.Flat };
            btnViewCd = new Button { Text = "CD", Left = 115, Top = 10, Width = 100, Height = 35, FlatStyle = FlatStyle.Flat };
            btnViewJurnal = new Button { Text = "JURNAL", Left = 220, Top = 10, Width = 100, Height = 35, FlatStyle = FlatStyle.Flat };
            tab.Controls.Add(btnViewBuku); tab.Controls.Add(btnViewCd); tab.Controls.Add(btnViewJurnal);

            // 2. DataGridView
            dgvKoleksi = new DataGridView
            {
                Left = 10,
                Top = 55,
                Width = 650,
                Height = 510,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            };
            tab.Controls.Add(dgvKoleksi);
            dgvKoleksi.SelectionChanged += (s, e) => FillKoleksiFromGrid();

            // 3. Input Utama (Common Fields)
            int x = 680, y = 20;

            tab.Controls.Add(new Label { Text = "Judul", Left = x, Top = y, AutoSize = true });
            cJudul = new TextBox { Left = x, Top = y + 18, Width = 360 };
            tab.Controls.Add(cJudul);
            y += 55;

            tab.Controls.Add(new Label { Text = "Kategori", Left = x, Top = y, AutoSize = true });
            cKategori = new ComboBox { Left = x, Top = y + 18, Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
            tab.Controls.Add(cKategori);
            y += 55;

            tab.Controls.Add(new Label { Text = "Harga (Rp)", Left = x, Top = y, AutoSize = true });
            cHarga = new TextBox { Left = x, Top = y + 18, Width = 170 };
            tab.Controls.Add(cHarga);

            tab.Controls.Add(new Label { Text = "Status", Left = x + 190, Top = y, AutoSize = true });
            cStatus = new ComboBox { Left = x + 190, Top = y + 18, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
            cStatus.Items.AddRange(new object[] { "TERSEDIA", "DIPINJAM", "HILANG", "NONAKTIF" });
            cStatus.SelectedIndex = 0;
            tab.Controls.Add(cStatus);

            y += 60;

            // 3.5 Upload Foto (opsional)
            tab.Controls.Add(new Label { Text = "Cover (opsional)", Left = x, Top = y, AutoSize = true });

            picPreview = new PictureBox
            {
                Left = x,
                Top = y + 18,
                Width = 110,
                Height = 140,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            tab.Controls.Add(picPreview);

            cUploadFoto = new Button { Text = "Upload Foto", Left = x + 125, Top = y + 18, Width = 235, Height = 32 };
            cClearFoto = new Button { Text = "Hapus Foto", Left = x + 125, Top = y + 56, Width = 235, Height = 32 };

            lblFotoInfo = new Label { Text = "(Belum ada foto)", Left = x + 125, Top = y + 95, Width = 235, Height = 60 };

            tab.Controls.Add(cUploadFoto);
            tab.Controls.Add(cClearFoto);
            tab.Controls.Add(lblFotoInfo);

            cUploadFoto.Click += (s, e) => PickAndSaveCoverFile();
            cClearFoto.Click += (s, e) => ClearCoverSelection();

            y += 170; // geser posisi panel detail di bawah preview foto



            // 4. Panel Input Spesifik (Ditumpuk di koordinat yang sama)
            panelBuku = new Panel { Left = x, Top = y, Width = 380, Height = 180 };
            panelCd = new Panel { Left = x, Top = y, Width = 380, Height = 180 };
            panelJurnal = new Panel { Left = x, Top = y, Width = 380, Height = 180 };
            tab.Controls.Add(panelBuku); tab.Controls.Add(panelCd); tab.Controls.Add(panelJurnal);

            InitSubPanels(); // Membangun TextBox di dalam panel-panel di atas

            // 5. Action Buttons
            int by = 570;
            cAdd = new Button { Text = "Tambah", Left = x, Top = by, Width = 110, Height = 35, BackColor = Theme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            cUpd = new Button { Text = "Edit", Left = x + 120, Top = by, Width = 110, Height = 35 };
            cDel = new Button { Text = "Hapus", Left = x + 240, Top = by, Width = 110, Height = 35 };
            tab.Controls.Add(cAdd); tab.Controls.Add(cUpd); tab.Controls.Add(cDel);

            // Events
            btnViewBuku.Click += (s, e) => SwitchKoleksiView("BUKU");
            btnViewCd.Click += (s, e) => SwitchKoleksiView("CD");
            btnViewJurnal.Click += (s, e) => SwitchKoleksiView("JURNAL");

            cAdd.Click += (s, e) => ExecAddKoleksi();
            cUpd.Click += (s, e) => ExecUpdateKoleksi();
            cDel.Click += (s, e) => ExecDeleteKoleksi();
        }

        void InitSubPanels()
        {
            // ========= PANEL BUKU =========
            panelBuku.Controls.Clear();
            panelBuku.BackColor = Color.Transparent;

            int yy = 0;

            // ISBN
            panelBuku.Controls.Add(new Label { Text = "ISBN", Left = 0, Top = yy, AutoSize = true });
            bIsbn = new TextBox { Left = 0, Top = yy + 18, Width = 360 };
            panelBuku.Controls.Add(bIsbn);
            yy += 45;

            // Penulis
            panelBuku.Controls.Add(new Label { Text = "Penulis", Left = 0, Top = yy, AutoSize = true });
            bPenulis = new TextBox { Left = 0, Top = yy + 18, Width = 360 };
            panelBuku.Controls.Add(bPenulis);
            yy += 45;

            // Penerbit + Tahun + Stok (1 baris)
            panelBuku.Controls.Add(new Label { Text = "Penerbit", Left = 0, Top = yy, AutoSize = true });
            bPenerbit = new TextBox { Left = 0, Top = yy + 18, Width = 180 };
            panelBuku.Controls.Add(bPenerbit);

            panelBuku.Controls.Add(new Label { Text = "Tahun", Left = 190, Top = yy, AutoSize = true });
            bTahun = new TextBox { Left = 190, Top = yy + 18, Width = 80 };
            panelBuku.Controls.Add(bTahun);

            panelBuku.Controls.Add(new Label { Text = "Stok", Left = 280, Top = yy, AutoSize = true });
            bStok = new TextBox { Left = 280, Top = yy + 18, Width = 80 };
            panelBuku.Controls.Add(bStok);


            // ========= PANEL CD =========
            panelCd.Controls.Clear();
            panelCd.BackColor = Color.Transparent;

            yy = 0;

            // Judul Album
            panelCd.Controls.Add(new Label { Text = "Judul Album", Left = 0, Top = yy, AutoSize = true });
            cdAlbum = new TextBox { Left = 0, Top = yy + 18, Width = 360 };
            panelCd.Controls.Add(cdAlbum);
            yy += 45;

            // Artis
            panelCd.Controls.Add(new Label { Text = "Artis", Left = 0, Top = yy, AutoSize = true });
            cdArtis = new TextBox { Left = 0, Top = yy + 18, Width = 360 };
            panelCd.Controls.Add(cdArtis);
            yy += 45;

            // Durasi + Format (1 baris)
            panelCd.Controls.Add(new Label { Text = "Durasi (Menit)", Left = 0, Top = yy, AutoSize = true });
            cdDurasi = new TextBox { Left = 0, Top = yy + 18, Width = 170 };
            panelCd.Controls.Add(cdDurasi);

            panelCd.Controls.Add(new Label { Text = "Format", Left = 190, Top = yy, AutoSize = true });
            cdFormat = new ComboBox
            {
                Left = 190,
                Top = yy + 18,
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cdFormat.Items.Clear();
            cdFormat.Items.AddRange(new object[] { "AUDIO", "VIDEO", "DATA" });
            cdFormat.SelectedIndex = 0;
            panelCd.Controls.Add(cdFormat);


            // ========= PANEL JURNAL =========
            panelJurnal.Controls.Clear();
            panelJurnal.BackColor = Color.Transparent;

            yy = 0;

            // ISSN
            panelJurnal.Controls.Add(new Label { Text = "ISSN", Left = 0, Top = yy, AutoSize = true });
            jIssn = new TextBox { Left = 0, Top = yy + 18, Width = 360 };
            panelJurnal.Controls.Add(jIssn);
            yy += 45;

            // Volume + Nomor (1 baris)
            panelJurnal.Controls.Add(new Label { Text = "Volume", Left = 0, Top = yy, AutoSize = true });
            jVolume = new TextBox { Left = 0, Top = yy + 18, Width = 170 };
            panelJurnal.Controls.Add(jVolume);

            panelJurnal.Controls.Add(new Label { Text = "Nomor", Left = 190, Top = yy, AutoSize = true });
            jNomor = new TextBox { Left = 190, Top = yy + 18, Width = 170 };
            panelJurnal.Controls.Add(jNomor);
            yy += 45;

            // Penerbit + Tahun (1 baris)
            panelJurnal.Controls.Add(new Label { Text = "Penerbit", Left = 0, Top = yy, AutoSize = true });
            jPenerbit = new TextBox { Left = 0, Top = yy + 18, Width = 170 };
            panelJurnal.Controls.Add(jPenerbit);

            panelJurnal.Controls.Add(new Label { Text = "Tahun", Left = 190, Top = yy, AutoSize = true });
            jTahun = new TextBox { Left = 190, Top = yy + 18, Width = 170 };
            panelJurnal.Controls.Add(jTahun);
        }


        void SwitchKoleksiView(string type)
        {
            currentKoleksiType = type;
            btnViewBuku.BackColor = (type == "BUKU") ? Color.SkyBlue : Color.White;
            btnViewCd.BackColor = (type == "CD") ? Color.SkyBlue : Color.White;
            btnViewJurnal.BackColor = (type == "JURNAL") ? Color.SkyBlue : Color.White;

            panelBuku.Visible = (type == "BUKU");
            panelCd.Visible = (type == "CD");
            panelJurnal.Visible = (type == "JURNAL");

            LoadKoleksi();
            ClearKoleksiInput();
        }

        void LoadKoleksi()
        {
            dgvKoleksi.DataSource = null;
            // Ambil semua data lalu filter berdasarkan tipe aktif
            var data = koleksiRepo.GetAll().Where(k => k.TipeKoleksi == currentKoleksiType).ToList();
            dgvKoleksi.DataSource = data;

            // Sembunyikan kolom ID
            if (dgvKoleksi.Columns["IdKoleksi"] != null) dgvKoleksi.Columns["IdKoleksi"].Visible = false;
            if (dgvKoleksi.Columns["IdKategori"] != null) dgvKoleksi.Columns["IdKategori"].Visible = false;
            if (dgvKoleksi.Columns["TipeKoleksi"] != null) dgvKoleksi.Columns["TipeKoleksi"].Visible = false;

            // Logika Penyembunyian Kolom Tidak Relevan
            string[] bukuOnly = { "Isbn", "Penulis", "Penerbit", "TahunTerbit", "Stok" };
            string[] cdOnly = { "JudulAlbum", "Artis", "DurasiMenit", "Format" };
            string[] jurnalOnly = { "Issn", "Volume", "Nomor" };

            foreach (DataGridViewColumn col in dgvKoleksi.Columns)
            {
                if (currentKoleksiType == "BUKU" && (cdOnly.Contains(col.Name) || jurnalOnly.Contains(col.Name))) col.Visible = false;
                if (currentKoleksiType == "CD" && (bukuOnly.Contains(col.Name) || jurnalOnly.Contains(col.Name))) col.Visible = false;
                if (currentKoleksiType == "JURNAL" && (bukuOnly.Contains(col.Name) || cdOnly.Contains(col.Name))) col.Visible = false;
            }
        }

        void FillKoleksiFromGrid()
        {
            if (dgvKoleksi.CurrentRow == null) return;
            var k = dgvKoleksi.CurrentRow.DataBoundItem as KoleksiRow;
            if (k == null) return;

            cJudul.Text = k.Judul; cHarga.Text = k.Harga.ToString(); cStatus.Text = k.Status;

            selectedFotoPath = k.FotoPath;
            lblFotoInfo.Text = string.IsNullOrWhiteSpace(selectedFotoPath) ? "(Belum ada foto)" : selectedFotoPath;
            LoadPreviewImage(selectedFotoPath);


            // Set Combo Kategori
            for (int i = 0; i < cKategori.Items.Count; i++)
                if (((ComboItem)cKategori.Items[i]).Id == k.IdKategori) { cKategori.SelectedIndex = i; break; }

            // Isi Detail Spesifik
            if (currentKoleksiType == "BUKU")
            {
                bIsbn.Text = k.Isbn; bPenulis.Text = k.Penulis; bPenerbit.Text = k.Penerbit;
                bTahun.Text = k.TahunTerbit.ToString(); bStok.Text = k.Stok.ToString();
            }
            else if (currentKoleksiType == "CD")
            {
                cdAlbum.Text = k.JudulAlbum; cdArtis.Text = k.Artis;
                cdDurasi.Text = k.DurasiMenit.ToString(); cdFormat.Text = k.Format;
            }
            else
            {
                jIssn.Text = k.Issn; jVolume.Text = k.Volume; jNomor.Text = k.Nomor;
                jPenerbit.Text = k.Penerbit; jTahun.Text = k.TahunTerbit.ToString();
            }
        }
        void LoadPreviewImage(string relativePath)
        {
            try
            {
                picPreview.Image = null;

                if (string.IsNullOrWhiteSpace(relativePath)) return;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string full = System.IO.Path.Combine(baseDir, relativePath);

                if (!System.IO.File.Exists(full)) return;

                using (var fs = new System.IO.FileStream(full, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    picPreview.Image = Image.FromStream(fs);
                }
            }
            catch
            {
                // kalau error, biarkan kosong
                picPreview.Image = null;
            }
        }

        void ExecAddKoleksi()
        {
            try
            {
                if (cKategori.SelectedItem == null)
                {
                    MessageBox.Show("Pilih kategori dulu.");
                    return;
                }

                string judul = (cJudul.Text ?? "").Trim();
                if (judul.Length == 0)
                {
                    MessageBox.Show("Judul wajib diisi.");
                    return;
                }

                int idKat = ((ComboItem)cKategori.SelectedItem).Id;

                int harga;
                if (!int.TryParse((cHarga.Text ?? "").Trim(), out harga)) harga = 0;
                if (harga < 0) harga = 0;

                string status = cStatus.Text;

                int newId = 0;

                if (currentKoleksiType == "BUKU")
                {
                    string isbn = (bIsbn.Text ?? "").Trim();
                    string penulis = (bPenulis.Text ?? "").Trim();
                    string penerbit = (bPenerbit.Text ?? "").Trim();

                    int tahun, stok;
                    if (!int.TryParse((bTahun.Text ?? "").Trim(), out tahun))
                    {
                        MessageBox.Show("Tahun terbit harus angka.");
                        return;
                    }
                    if (!int.TryParse((bStok.Text ?? "").Trim(), out stok))
                    {
                        MessageBox.Show("Stok harus angka.");
                        return;
                    }
                    if (stok < 0) stok = 0;

                    // INSERT + ambil id_koleksi baru
                    newId = koleksiRepo.AddBuku(judul, idKat, harga, status, isbn, penulis, penerbit, tahun, stok);
                }
                else if (currentKoleksiType == "CD")
                {
                    string album = (cdAlbum.Text ?? "").Trim();
                    string artis = (cdArtis.Text ?? "").Trim();

                    int durasi;
                    if (!int.TryParse((cdDurasi.Text ?? "").Trim(), out durasi))
                    {
                        MessageBox.Show("Durasi harus angka (menit).");
                        return;
                    }
                    if (durasi <= 0)
                    {
                        MessageBox.Show("Durasi harus lebih dari 0.");
                        return;
                    }

                    string format = cdFormat.Text;

                    newId = koleksiRepo.AddCd(judul, idKat, harga, status, album, artis, durasi, format);
                }
                else // JURNAL
                {
                    string issn = (jIssn.Text ?? "").Trim();
                    string volume = (jVolume.Text ?? "").Trim();
                    string nomor = (jNomor.Text ?? "").Trim();
                    string penerbit = (jPenerbit.Text ?? "").Trim();

                    int tahun;
                    if (!int.TryParse((jTahun.Text ?? "").Trim(), out tahun))
                    {
                        MessageBox.Show("Tahun terbit jurnal harus angka.");
                        return;
                    }

                    newId = koleksiRepo.AddJurnal(judul, idKat, harga, status, issn, volume, nomor, tahun, penerbit);
                }

                // kalau admin pilih foto -> simpan path ke DB
                if (newId > 0 && !string.IsNullOrWhiteSpace(selectedFotoPath))
                {
                    koleksiRepo.UpdateFotoPath(newId, selectedFotoPath);
                }

                LoadKoleksi();
                MessageBox.Show("Data koleksi berhasil ditambah!");
                ClearKoleksiInput();
                selectedFotoPath = null;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                // Tangkap error yang umum: duplicate key
                if (ex.Number == 1062)
                {
                    MessageBox.Show("Data koleksi duplikat (misal ISBN/ISSN sudah ada).");
                    return;
                }

                MessageBox.Show("MySQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal tambah koleksi: " + ex.Message);
            }
        }

        void ExecUpdateKoleksi()
        {
            try
            {
                if (dgvKoleksi.CurrentRow == null) return;
                var row = dgvKoleksi.CurrentRow.DataBoundItem as KoleksiRow;
                if (row == null) return;

                if (cKategori.SelectedItem == null)
                {
                    MessageBox.Show("Pilih kategori dulu.");
                    return;
                }

                string judul = (cJudul.Text ?? "").Trim();
                if (judul.Length == 0)
                {
                    MessageBox.Show("Judul wajib diisi.");
                    return;
                }

                int idKat = ((ComboItem)cKategori.SelectedItem).Id;

                int harga;
                if (!int.TryParse((cHarga.Text ?? "").Trim(), out harga)) harga = 0;
                if (harga < 0) harga = 0;

                string status = cStatus.Text;

                if (currentKoleksiType == "BUKU")
                {
                    int tahun, stok;
                    if (!int.TryParse((bTahun.Text ?? "").Trim(), out tahun))
                    {
                        MessageBox.Show("Tahun terbit harus angka.");
                        return;
                    }
                    if (!int.TryParse((bStok.Text ?? "").Trim(), out stok))
                    {
                        MessageBox.Show("Stok harus angka.");
                        return;
                    }
                    if (stok < 0) stok = 0;

                    koleksiRepo.UpdateBuku(
                        row.IdKoleksi, judul, idKat, harga, status,
                        (bIsbn.Text ?? "").Trim(),
                        (bPenulis.Text ?? "").Trim(),
                        (bPenerbit.Text ?? "").Trim(),
                        tahun,
                        stok
                    );
                }
                else if (currentKoleksiType == "CD")
                {
                    int durasi;
                    if (!int.TryParse((cdDurasi.Text ?? "").Trim(), out durasi))
                    {
                        MessageBox.Show("Durasi harus angka (menit).");
                        return;
                    }
                    if (durasi <= 0)
                    {
                        MessageBox.Show("Durasi harus lebih dari 0.");
                        return;
                    }

                    koleksiRepo.UpdateCd(
                        row.IdKoleksi, judul, idKat, harga, status,
                        (cdAlbum.Text ?? "").Trim(),
                        (cdArtis.Text ?? "").Trim(),
                        durasi,
                        cdFormat.Text
                    );
                }
                else // JURNAL
                {
                    int tahun;
                    if (!int.TryParse((jTahun.Text ?? "").Trim(), out tahun))
                    {
                        MessageBox.Show("Tahun terbit jurnal harus angka.");
                        return;
                    }

                    koleksiRepo.UpdateJurnal(
                        row.IdKoleksi, judul, idKat, harga, status,
                        (jIssn.Text ?? "").Trim(),
                        (jVolume.Text ?? "").Trim(),
                        (jNomor.Text ?? "").Trim(),
                        tahun,
                        (jPenerbit.Text ?? "").Trim()
                    );
                }

                // kalau admin memilih foto baru, update foto_path
                if (!string.IsNullOrWhiteSpace(selectedFotoPath))
                {
                    koleksiRepo.UpdateFotoPath(row.IdKoleksi, selectedFotoPath);
                    selectedFotoPath = null;
                }

                LoadKoleksi();
                MessageBox.Show("Berhasil update.");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    MessageBox.Show("Update gagal: data duplikat (misal ISBN/ISSN sudah terpakai).");
                    return;
                }
                MessageBox.Show("MySQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        void ExecDeleteKoleksi()
        {
            try
            {
                if (dgvKoleksi.CurrentRow == null) return;
                var row = dgvKoleksi.CurrentRow.DataBoundItem as KoleksiRow;
                if (MessageBox.Show("Hapus koleksi ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    koleksiRepo.Delete(row.IdKoleksi); LoadKoleksi(); ClearKoleksiInput();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        void ClearKoleksiInput()
        {
            cJudul.Clear(); cHarga.Clear(); bIsbn.Clear(); bPenulis.Clear(); bPenerbit.Clear(); bTahun.Clear(); bStok.Clear();
            cdAlbum.Clear(); cdArtis.Clear(); cdDurasi.Clear(); jIssn.Clear(); jVolume.Clear(); jNomor.Clear(); jPenerbit.Clear(); jTahun.Clear();
        }
        #endregion
        void PickAndSaveCoverFile()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Pilih Foto Cover";
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // Folder output (bin\Debug) + Assets\Covers
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string coversDir = System.IO.Path.Combine(baseDir, @"Assets\Covers");
                    if (!System.IO.Directory.Exists(coversDir))
                        System.IO.Directory.CreateDirectory(coversDir);

                    // Nama file aman: cover_{timestamp}.ext
                    string ext = System.IO.Path.GetExtension(ofd.FileName);
                    string fileName = "cover_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ext;
                    string destFull = System.IO.Path.Combine(coversDir, fileName);

                    System.IO.File.Copy(ofd.FileName, destFull, true);

                    // Simpan sebagai relative path (sesuai DB)
                    selectedFotoPath = @"Assets\Covers\" + fileName;
                    lblFotoInfo.Text = selectedFotoPath;

                    // Preview
                    using (var fs = new System.IO.FileStream(destFull, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        picPreview.Image = Image.FromStream(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal upload foto: " + ex.Message);
                }
            }
        }

        void ClearCoverSelection()
        {
            selectedFotoPath = null;
            lblFotoInfo.Text = "(Belum ada foto)";
            picPreview.Image = null;
        }

        void LoadKategoriCombo()
        {
            cKategori.Items.Clear();
            foreach (var k in kategoriRepo.GetAll()) cKategori.Items.Add(new ComboItem(k.IdKategori, k.NamaKategori));
            if (cKategori.Items.Count > 0) cKategori.SelectedIndex = 0;
        }
        void PrintIdCard()
        {
            if (dgvAnggota.CurrentRow == null) return;
            var a = dgvAnggota.CurrentRow.DataBoundItem as Anggota;
            if (a == null) return;

            new IdCardForm(a).ShowDialog();
        }
    }

    // Helper Class untuk ComboBox
    public class ComboItem
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ComboItem(int id, string text) { Id = id; Text = text; }
        public override string ToString() { return Text; }
    }
}