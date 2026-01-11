using System;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using AplikasiPerpustakaanSiJess.Data;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class AdminLaporanForm : Form
    {
        Label lbl;
        Button btnExport;

        public AdminLaporanForm()
        {
            Theme.ApplyForm(this);
            BackColor = Theme.Bg;
            Controls.Add(new Label { Text = "Laporan", Font = Theme.H1, Left = 18, Top = 12, AutoSize = true });

            lbl = new Label { Left = 18, Top = 60, Width = 800, Height = 200, Font = Theme.Body };
            btnExport = new Button
            {
                Text = "Export CSV",
                Left = 18,
                Top = 270,
                Width = 140,
                Height = 32,
                BackColor = Theme.Primary,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExport.FlatAppearance.BorderSize = 0;

            Controls.Add(lbl);
            Controls.Add(btnExport);

            btnExport.Click += (_, __) => ExportCsv();

            LoadReport();
        }

        void LoadReport()
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();

                string sql = @"
SELECT status_item, COUNT(*) jumlah, SUM(denda) total_denda
FROM peminjaman_detail
GROUP BY status_item;";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        string text = "Rekap Status:\n";
                        int grand = 0;

                        while (r.Read())
                        {
                            string status = r.GetString(0);
                            int jumlah = r.GetInt32(1);
                            int total = r.IsDBNull(2) ? 0 : r.GetInt32(2);
                            grand += total;

                            // String interpolation tetap bisa digunakan di .NET 4.8
                            text += string.Format("- {0}: {1} transaksi | Total denda Rp{2:n0}\n", status, jumlah, total);
                        }

                        text += string.Format("\nTOTAL DENDA TERKUMPUL: Rp{0:n0}", grand);
                        lbl.Text = text;
                    }
                }
            }
        }

        void ExportCsv()
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "laporan_sijess.csv" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                using (MySqlConnection conn = Db.GetConnection())
                {
                    conn.Open();

                    string sql = @"
SELECT d.id_detail, d.status_item, d.denda,
       p.id_pinjam, p.tgl_pinjam, p.tgl_jatuh_tempo,
       a.nama_lengkap,
       k.judul
FROM peminjaman_detail d
JOIN peminjaman p ON p.id_pinjam=d.id_pinjam
JOIN anggota a ON a.id_anggota=p.id_anggota
JOIN koleksi k ON k.id_koleksi=d.id_koleksi
ORDER BY d.id_detail DESC;";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            // StreamWriter wajib menggunakan using agar file tertutup sempurna
                            using (StreamWriter sw = new StreamWriter(sfd.FileName))
                            {
                                sw.WriteLine("id_detail,status_item,denda,id_pinjam,tgl_pinjam,tgl_jatuh_tempo,nama_anggota,judul");

                                while (r.Read())
                                {
                                    sw.WriteLine(string.Format("{0},{1},{2},{3},{4:yyyy-MM-dd},{5:yyyy-MM-dd},\"{6}\",\"{7}\"",
                                        r.GetInt32(0),
                                        r.GetString(1),
                                        r.GetInt32(2),
                                        r.GetInt32(3),
                                        r.GetDateTime(4),
                                        r.GetDateTime(5),
                                        r.GetString(6),
                                        r.GetString(7)));
                                }
                            }
                        }
                    }
                }
            }

            MessageBox.Show("Export selesai.");
        }
    }
}