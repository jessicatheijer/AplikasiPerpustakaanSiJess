namespace AplikasiPerpustakaanSiJess.Data
{
    public static class Sql
    {
        public const string KategoriGetAll =
            "SELECT id_kategori, nama_kategori FROM kategori ORDER BY nama_kategori;";

        public const string AnggotaGetById =
            "SELECT id_anggota,nama_lengkap,nik_or_nis,jenis_kelamin,alamat,no_telp FROM anggota WHERE id_anggota=@id;";

        public const string KoleksiKatalog =
@"
SELECT
  k.id_koleksi, k.judul, k.tipe_koleksi, kt.nama_kategori, k.status, k.harga,
  b.penerbit AS penerbit_buku, b.tahun_terbit AS tahun_buku, b.stok,
  c.artis AS artis_cd, c.durasi_menit,
  j.penerbit AS penerbit_jurnal, j.tahun_terbit AS tahun_jurnal
FROM koleksi k
JOIN kategori kt ON kt.id_kategori = k.id_kategori
LEFT JOIN buku b ON b.id_koleksi = k.id_koleksi
LEFT JOIN cd c ON c.id_koleksi = k.id_koleksi
LEFT JOIN jurnal j ON j.id_koleksi = k.id_koleksi
WHERE 1=1
  AND (@q = '' OR k.judul LIKE CONCAT('%', @q, '%'))
  AND (@tipe = 'SEMUA' OR k.tipe_koleksi = @tipe)
  AND (@kat IS NULL OR k.id_kategori = @kat)
ORDER BY k.id_koleksi DESC;";
    }
}
