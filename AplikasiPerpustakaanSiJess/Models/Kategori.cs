namespace AplikasiPerpustakaanSiJess.Models
{
    public class Kategori
    {
        public int IdKategori { get; set; }
        public string NamaKategori { get; set; } = "";
        public override string ToString() => NamaKategori;
    }
}
