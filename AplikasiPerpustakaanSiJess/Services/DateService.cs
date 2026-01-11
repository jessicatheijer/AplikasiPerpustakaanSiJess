using System;

namespace AplikasiPerpustakaanSiJess.Services
{
    public static class DateService
    {
        public static DateTime JatuhTempo7Hari(DateTime tglPinjam)
            => tglPinjam.AddDays(7);
    }
}
