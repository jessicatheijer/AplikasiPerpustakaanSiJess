using System;
using System.Drawing;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Models;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Controls
{
    public class KoleksiCardControl : UserControl
    {
        private PictureBox pic;
        private Label lblTitle, lblSub, lblSub2, lblStatus;
        private Panel pop;
        private Label popText;

        public KoleksiCard Data { get; private set; }

        public event Action<KoleksiCard> OnSelect;

        public KoleksiCardControl(KoleksiCard data, Image cover)
        {
            Data = data;
            Width = 220;
            Height = 280;
            BackColor = Theme.Card;
            Theme.RoundControl(this, 16);
            Margin = new Padding(10);
            Padding = new Padding(10);

            pic = new PictureBox
            {
                Width = 200,
                Height = 140,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = cover,
                Cursor = Cursors.Hand
            };

            lblTitle = new Label { AutoSize = false, Width = 200, Height = 40, Font = Theme.H2, Text = data.Judul };
            lblSub = new Label { AutoSize = false, Width = 200, Height = 18, Font = Theme.Body, ForeColor = Theme.Muted, Text = data.DetailLine1 };
            lblSub2 = new Label { AutoSize = false, Width = 200, Height = 18, Font = Theme.Body, ForeColor = Theme.Muted, Text = data.DetailLine2 };
            lblStatus = new Label { AutoSize = true, Font = Theme.Body, ForeColor = (data.Status == "TERSEDIA") ? Color.Green : Color.IndianRed, Text = $"Status: {data.Status}" };

            if (data.TipeKoleksi == "BUKU")
            {
                lblStatus.Text += " | Stok: " + data.Stok.ToString();
            }

            Controls.Add(pic);
            Controls.Add(lblTitle);
            Controls.Add(lblSub);
            Controls.Add(lblSub2);
            Controls.Add(lblStatus);

            pic.Top = 10; pic.Left = 10;
            lblTitle.Top = pic.Bottom + 8; lblTitle.Left = 10;
            lblSub.Top = lblTitle.Bottom + 4; lblSub.Left = 10;
            lblSub2.Top = lblSub.Bottom + 2; lblSub2.Left = 10;
            lblStatus.Top = lblSub2.Bottom + 6; lblStatus.Left = 10;

            // popup sinopsis kecil 
            pop = new Panel
            {
                Visible = false,
                Width = 200,
                Height = 90,
                BackColor = Color.FromArgb(255, 255, 240),
                BorderStyle = BorderStyle.FixedSingle
            };
            popText = new Label
            {
                Dock = DockStyle.Fill,
                Font = Theme.Body,
                Padding = new Padding(6),
                Text = BuildSinopsis(data)
            };
            pop.Controls.Add(popText);
            Controls.Add(pop);
            pop.Left = 10; pop.Top = 10;

            pic.Click += (_, __) => TogglePopup();
            lblTitle.Click += (_, __) => TogglePopup();
            this.DoubleClick += (_, __) => OnSelect?.Invoke(Data);
        }

        private void TogglePopup()
        {
            pop.Visible = !pop.Visible;
        }

        private string BuildSinopsis(KoleksiCard d)
        {
            return
$@"{d.TipeKoleksi} • {d.NamaKategori}
Harga: Rp{d.Harga:n0}

Klik lagi untuk menutup.
(Double click kartu untuk pilih.)";
        }

        private Image LoadCoverImage(string relativePath)
        {
            try
            {
                // base folder aplikasi (bin\Debug / bin\Release)
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                // path file cover dari DB (relative)
                if (!string.IsNullOrWhiteSpace(relativePath))
                {
                    string full = System.IO.Path.Combine(baseDir, relativePath);
                    if (System.IO.File.Exists(full))
                    {
                        // biar file nggak ke-lock: load via stream
                        using (var fs = new System.IO.FileStream(full, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            return Image.FromStream(fs);
                        }
                    }
                }

                // fallback placeholder
                string placeholder = System.IO.Path.Combine(baseDir, @"Assets\placeholder.png");
                if (System.IO.File.Exists(placeholder))
                    return Image.FromFile(placeholder);

                return null;
            }
            catch
            {
                return null;
            }
        }

    }
}
