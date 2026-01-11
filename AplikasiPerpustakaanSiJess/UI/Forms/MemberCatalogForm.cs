using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Repositories;
using AplikasiPerpustakaanSiJess.UI;
using AplikasiPerpustakaanSiJess.UI.Controls;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class MemberCatalogForm : Form
    {
        TextBox txtSearch;
        ComboBox cbTipe, cbKategori;
        FlowLayoutPanel flow;

        readonly KoleksiRepository koleksiRepo = new KoleksiRepository();
        readonly KategoriRepository kategoriRepo = new KategoriRepository();

        Image imgBook, imgCd, imgJurnal, imgPlaceholder;

        public MemberCatalogForm()
        {
            Theme.ApplyForm(this);
            BackColor = Theme.Bg;
            Dock = DockStyle.Fill;           
            AutoScroll = false;

            LoadImages();

            var header = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Theme.Bg };
            Controls.Add(header);

            header.Controls.Add(new Label { Text = "Katalog Koleksi", Font = Theme.H1, Left = 18, Top = 5, AutoSize = true });

            txtSearch = new TextBox { Left = 18, Top = 35, Width = 280 };
            cbTipe = new ComboBox { Left = 310, Top = 35, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cbKategori = new ComboBox { Left = 460, Top = 35, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            var btn = new Button { Text = "Cari", Left = 670, Top = 32, Width = 70, Height = 28 };


            cbTipe.Items.AddRange(new object[] { "SEMUA", "BUKU", "CD", "JURNAL" });
            cbTipe.SelectedIndex = 0;

            header.Controls.Add(txtSearch);
            header.Controls.Add(cbTipe);
            header.Controls.Add(cbKategori);
            header.Controls.Add(btn);
            
            flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,     
                WrapContents = true,
                BackColor = Theme.Bg,
                Padding = new Padding(8),
                Margin = new Padding(0)
            };
            Controls.Add(flow);

            // event
            btn.Click += (_, __) => LoadKatalog();
            cbTipe.SelectedIndexChanged += (_, __) => LoadKatalog();
            cbKategori.SelectedIndexChanged += (_, __) => LoadKatalog();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadKatalog(); };

            LoadKategori();
            LoadKatalog();
        }

        void LoadImages()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            imgPlaceholder = LoadImg(Path.Combine(baseDir, "Assets", "placeholder.png"));
            imgBook = LoadImg(Path.Combine(baseDir, "Assets", "book.png")) ?? imgPlaceholder;
            imgCd = LoadImg(Path.Combine(baseDir, "Assets", "cd.png")) ?? imgPlaceholder;
            imgJurnal = LoadImg(Path.Combine(baseDir, "Assets", "jurnal.png")) ?? imgPlaceholder;
        }

        Image LoadImg(string path)
        {
            try { return File.Exists(path) ? Image.FromFile(path) : null; }
            catch { return null; }
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

        void LoadKatalog()
        {
            flow.Controls.Clear();

            string q = txtSearch.Text.Trim();
            string tipe = cbTipe.SelectedItem?.ToString() ?? "SEMUA";

            int? katId = null;
            var selected = cbKategori.SelectedItem;
            if (selected != null)
            {
                var prop = selected.GetType().GetProperty("Id");
                katId = (int?)prop.GetValue(selected, null);
            }

            var items = koleksiRepo.GetKatalog(q, tipe, katId);

            foreach (var it in items)
            {
                var fallback = (it.TipeKoleksi == "BUKU") ? imgBook : (it.TipeKoleksi == "CD") ? imgCd : imgJurnal;

                // kalau ada foto_path, load dari file
                Image cover = TryLoadCover(it.FotoPath) ?? fallback ?? imgPlaceholder;

                var card = new KoleksiCardControl(it, cover);
                flow.Controls.Add(card);
            }

        }
        Image TryLoadCover(string fotoPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fotoPath)) return null;

                string full = Path.IsPathRooted(fotoPath)
                    ? fotoPath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fotoPath);

                if (!File.Exists(full)) return null;

                // biar ga nge-lock file:
                using (var fs = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var img = Image.FromStream(fs))
                {
                    return new Bitmap(img); // clone supaya stream boleh ditutup
                }
            }
            catch
            {
                return null;
            }
        }

    }
}
