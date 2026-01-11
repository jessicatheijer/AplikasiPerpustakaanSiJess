namespace AplikasiPerpustakaanSiJess.Models
{
    public static class Session
    {
        public static int? IdAnggota { get; set; }
        public static string Username { get; set; } = "";
        public static string Role { get; set; } = ""; // ADMIN / ANGGOTA

        public static void Clear()
        {
            IdAnggota = null;
            Username = "";
            Role = "";
        }
    }
}
