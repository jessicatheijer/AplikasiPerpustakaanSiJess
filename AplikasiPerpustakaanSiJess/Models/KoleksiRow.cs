namespace AplikasiPerpustakaanSiJess.Models
{
    public class KoleksiRow
    {
        public int IdKoleksi { get; set; }
        public int IdKategori { get; set; }
        public string NamaKategori { get; set; }

        public string Judul { get; set; }
        public string TipeKoleksi { get; set; }
        public int Harga { get; set; }
        public string Status { get; set; }
        public string FotoPath { get; set; }


        // Buku
        public string Isbn { get; set; }
        public string Penulis { get; set; }
        public string Penerbit { get; set; }
        public int TahunTerbit { get; set; }
        public int Stok { get; set; }

        // CD
        public string JudulAlbum { get; set; }
        public string Artis { get; set; }
        public int DurasiMenit { get; set; }
        public string Format { get; set; }

        // Jurnal
        public string Issn { get; set; }
        public string Volume { get; set; }
        public string Nomor { get; set; }
    }
}
