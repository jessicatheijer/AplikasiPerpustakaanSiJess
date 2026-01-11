using System;

namespace AplikasiPerpustakaanSiJess.Models
{
    public class PeminjamanInfo
    {
        public int IdDetail { get; set; }
        public int IdPinjam { get; set; }
        public int IdKoleksi { get; set; }
        public string Judul { get; set; } = "";
        public DateTime TglPinjam { get; set; }
        public DateTime TglJatuhTempo { get; set; }
        public string StatusItem { get; set; } = ""; // DIPINJAM/KEMBALI/HILANG
        public int Denda { get; set; }
    }
}
