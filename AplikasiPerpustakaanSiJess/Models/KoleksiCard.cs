namespace AplikasiPerpustakaanSiJess.Models
{
    public class KoleksiCard
    {
        public int IdKoleksi { get; set; }
        public string Judul { get; set; }
        public string TipeKoleksi { get; set; }
        public string NamaKategori { get; set; }
        public int Harga { get; set; }
        public string Status { get; set; }

        public string DetailLine1 { get; set; }
        public string DetailLine2 { get; set; }
        public string FotoPath { get; set; }



        // detail display (optional)
        public string Penerbit { get; set; }   // buku/jurnal
        public string Penulis { get; set; }    // buku
        public int TahunTerbit { get; set; }   // buku/jurnal
        public int Stok { get; set; }          // buku
        public string Artis { get; set; }      // cd
        public int DurasiMenit { get; set; }   // cd
        public string Sinopsis { get; set; }   // popup kecil
    }
}
