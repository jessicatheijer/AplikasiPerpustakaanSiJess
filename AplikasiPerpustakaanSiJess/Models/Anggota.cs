namespace AplikasiPerpustakaanSiJess.Models
{
    public class Anggota
    {
        public int IdAnggota { get; set; }
        public string NamaLengkap { get; set; } = "";
        public string NikOrNis { get; set; } = "";
        public string JenisKelamin { get; set; } = "";
        public string Alamat { get; set; } = "";
        public string NoTelp { get; set; } = "";
    }
}
