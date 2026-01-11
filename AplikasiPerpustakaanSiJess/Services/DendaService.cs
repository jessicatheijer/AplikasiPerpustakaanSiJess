using System;

namespace AplikasiPerpustakaanSiJess.Services
{
    public static class DendaService
    {
        public const int DendaPerHari = 10000;
        public const int DendaHilangTetap = 50000;

        public static int TelatHari(DateTime jatuhTempo, DateTime tanggalKembali)
        {
            var late = (tanggalKembali.Date - jatuhTempo.Date).Days;
            return late > 0 ? late : 0;
        }

        public static int DendaTerlambat(DateTime jatuhTempo, DateTime tanggalKembali)
            => TelatHari(jatuhTempo, tanggalKembali) * DendaPerHari;

        public static int DendaHilang(int hargaKoleksi)
            => hargaKoleksi + DendaHilangTetap;
    }
}
