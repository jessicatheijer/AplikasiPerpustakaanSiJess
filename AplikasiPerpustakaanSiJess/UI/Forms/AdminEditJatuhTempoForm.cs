using System;
using System.Drawing;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Repositories;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class AdminEditJatuhTempoForm : Form
    {
        DateTimePicker dtpPinjam, dtpJatuhTempo; // Ditambah dtpPinjam
        Button btnSave;
        readonly PeminjamanRepository repo = new PeminjamanRepository();
        readonly int idPinjam;

        public bool IsSaved { get; private set; }

        // Constructor sekarang menerima dua DateTime
        public AdminEditJatuhTempoForm(int idPinjam, DateTime pinjam, DateTime jatuhTempo)
        {
            Theme.ApplyForm(this);
            this.idPinjam = idPinjam;

            Text = "Edit Tanggal Transaksi"; // Judul visual diubah agar lebih umum
            Width = 360;
            Height = 250; // Tinggi ditambah agar muat dua input
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Bg;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // 1. Kontrol Tanggal Pinjam
            Controls.Add(new Label { Text = "Tanggal Pinjam", Left = 18, Top = 18, AutoSize = true, Font = Theme.Body });
            dtpPinjam = new DateTimePicker { Left = 18, Top = 40, Width = 300 };
            dtpPinjam.Value = pinjam;
            Controls.Add(dtpPinjam);

            // 2. Kontrol Tanggal Jatuh Tempo
            Controls.Add(new Label { Text = "Tanggal Jatuh Tempo", Left = 18, Top = 80, AutoSize = true, Font = Theme.Body });
            dtpJatuhTempo = new DateTimePicker { Left = 18, Top = 102, Width = 300 };
            dtpJatuhTempo.Value = jatuhTempo;
            Controls.Add(dtpJatuhTempo);

            // 3. Tombol Simpan
            btnSave = new Button
            {
                Text = "Simpan Perubahan",
                Left = 18,
                Top = 155,
                Width = 300,
                Height = 35,
                BackColor = Theme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            Controls.Add(btnSave);

            btnSave.Click += (s, e) =>
            {
                try
                {
                    repo.UpdateTanggalPeminjaman(idPinjam, dtpPinjam.Value, dtpJatuhTempo.Value);
                    IsSaved = true;
                    MessageBox.Show("Tanggal berhasil diperbarui.");
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menyimpan: " + ex.Message);
                }
            };
        }
    }
}