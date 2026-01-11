using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using AplikasiPerpustakaanSiJess.Models;

namespace AplikasiPerpustakaanSiJess.Repositories
{
    public class KoleksiRepository
    {
        // =========================
        // MEMBER CATALOG
        // =========================
        public List<KoleksiCard> GetKatalog(string search = "", string tipe = "SEMUA", int? idKategori = null)
        {
            var list = new List<KoleksiCard>();

            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();

                string sql = @"
SELECT
  k.id_koleksi, k.judul, k.foto_path, k.tipe_koleksi, k.harga, k.status,
  kt.nama_kategori,
  b.penulis, b.penerbit AS penerbit_buku, b.tahun_terbit AS tahun_buku, b.stok,
  c.artis, c.durasi_menit,
  j.penerbit AS penerbit_jurnal, j.tahun_terbit AS tahun_jurnal
FROM koleksi k
JOIN kategori kt ON kt.id_kategori = k.id_kategori
LEFT JOIN buku b ON b.id_koleksi = k.id_koleksi
LEFT JOIN cd c ON c.id_koleksi = k.id_koleksi
LEFT JOIN jurnal j ON j.id_koleksi = k.id_koleksi
WHERE 1=1
";

                if (!string.IsNullOrWhiteSpace(search))
                    sql += " AND k.judul LIKE @q ";

                if (!string.IsNullOrWhiteSpace(tipe) && tipe != "SEMUA")
                    sql += " AND k.tipe_koleksi = @tipe ";

                if (idKategori.HasValue)
                    sql += " AND k.id_kategori = @kat ";

                sql += " ORDER BY k.id_koleksi DESC;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                        cmd.Parameters.AddWithValue("@q", "%" + search.Trim() + "%");
                    if (!string.IsNullOrWhiteSpace(tipe) && tipe != "SEMUA")
                        cmd.Parameters.AddWithValue("@tipe", tipe);
                    if (idKategori.HasValue)
                        cmd.Parameters.AddWithValue("@kat", idKategori.Value);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var x = new KoleksiCard();
                            x.IdKoleksi = r.GetInt32("id_koleksi");
                            x.Judul = r.GetString("judul");
                            x.FotoPath = r.IsDBNull(r.GetOrdinal("foto_path")) ? null : r.GetString("foto_path");
                            x.TipeKoleksi = r.GetString("tipe_koleksi");
                            x.Harga = r.GetInt32("harga");
                            x.Status = r.GetString("status");
                            x.NamaKategori = r.GetString("nama_kategori");

                            if (x.TipeKoleksi == "BUKU")
                            {
                                x.Penulis = r.IsDBNull(r.GetOrdinal("penulis")) ? "" : r.GetString("penulis");
                                x.Penerbit = r.IsDBNull(r.GetOrdinal("penerbit_buku")) ? "" : r.GetString("penerbit_buku");
                                x.TahunTerbit = r.IsDBNull(r.GetOrdinal("tahun_buku")) ? 0 : r.GetInt32("tahun_buku");
                                x.Stok = r.IsDBNull(r.GetOrdinal("stok")) ? 0 : r.GetInt32("stok");
                            }
                            else if (x.TipeKoleksi == "CD")
                            {
                                x.Artis = r.IsDBNull(r.GetOrdinal("artis")) ? "" : r.GetString("artis");
                                x.DurasiMenit = r.IsDBNull(r.GetOrdinal("durasi_menit")) ? 0 : r.GetInt32("durasi_menit");
                            }
                            else if (x.TipeKoleksi == "JURNAL")
                            {
                                x.Penerbit = r.IsDBNull(r.GetOrdinal("penerbit_jurnal")) ? "" : r.GetString("penerbit_jurnal");
                                x.TahunTerbit = r.IsDBNull(r.GetOrdinal("tahun_jurnal")) ? 0 : r.GetInt32("tahun_jurnal");
                            }

                            if (x.TipeKoleksi == "BUKU")
                            {
                                x.DetailLine1 = "Penulis: " + (x.Penulis ?? "-");
                                x.DetailLine2 = "Penerbit: " + (x.Penerbit ?? "-") + " • " + x.TahunTerbit;
                            }
                            else if (x.TipeKoleksi == "CD")
                            {
                                x.DetailLine1 = "Artis: " + (x.Artis ?? "-");
                                x.DetailLine2 = "Durasi: " + x.DurasiMenit + " menit";
                            }
                            else if (x.TipeKoleksi == "JURNAL")
                            {
                                x.DetailLine1 = "Penerbit: " + (x.Penerbit ?? "-");
                                x.DetailLine2 = "Tahun: " + x.TahunTerbit;
                            }


                            x.Sinopsis = "Koleksi " + x.TipeKoleksi + " - " + x.NamaKategori + ".\nStatus: " + x.Status;
                            list.Add(x);
                        }
                    }
                }
            }

            return list;
        }

        // =========================
        // ADMIN: LIST KOLEKSI TERSEDIA (untuk tambah peminjaman)
        // =========================
        public List<KoleksiCard> GetKoleksiTersedia(string search = "", string tipe = "SEMUA", int? idKategori = null)
        {
            var list = new List<KoleksiCard>();

            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();

                string sql = @"
SELECT 
  k.id_koleksi, k.judul, k.foto_path, k.tipe_koleksi, k.harga, k.status,
  kt.nama_kategori,
  b.stok
FROM koleksi k
JOIN kategori kt ON kt.id_kategori = k.id_kategori
LEFT JOIN buku b ON b.id_koleksi = k.id_koleksi
WHERE k.status = 'TERSEDIA'
";

                if (!string.IsNullOrWhiteSpace(search)) sql += " AND k.judul LIKE @q ";
                if (!string.IsNullOrWhiteSpace(tipe) && tipe != "SEMUA") sql += " AND k.tipe_koleksi = @tipe ";
                if (idKategori.HasValue) sql += " AND k.id_kategori = @kat ";

                sql += " ORDER BY k.id_koleksi DESC;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@q", "%" + search.Trim() + "%");
                    if (!string.IsNullOrWhiteSpace(tipe) && tipe != "SEMUA") cmd.Parameters.AddWithValue("@tipe", tipe);
                    if (idKategori.HasValue) cmd.Parameters.AddWithValue("@kat", idKategori.Value);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var x = new KoleksiCard();
                            x.IdKoleksi = r.GetInt32("id_koleksi");
                            x.Judul = r.GetString("judul");
                            x.FotoPath = r.IsDBNull(r.GetOrdinal("foto_path")) ? null : r.GetString("foto_path");
                            x.TipeKoleksi = r.GetString("tipe_koleksi");
                            x.Harga = r.GetInt32("harga");
                            x.Status = r.GetString("status");
                            x.NamaKategori = r.GetString("nama_kategori");
                            x.Stok = r.IsDBNull(r.GetOrdinal("stok")) ? 0 : r.GetInt32("stok");

                            // khusus buku: stok harus > 0
                            if (x.TipeKoleksi == "BUKU" && x.Stok <= 0) continue;

                            list.Add(x);
                        }
                    }
                }
            }

            return list;
        }

        // =========================
        // ADMIN MASTER DATA: GetAll untuk grid
        // =========================
        public List<KoleksiRow> GetAll()
        {
            var list = new List<KoleksiRow>();

            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();

                string sql = @"
SELECT
  k.id_koleksi, k.judul, k.foto_path, k.id_kategori, kt.nama_kategori, k.tipe_koleksi, k.harga, k.status,
  b.isbn, b.penulis, b.penerbit AS penerbit_buku, b.tahun_terbit AS tahun_buku, b.stok,
  c.judul_album, c.artis, c.durasi_menit, c.format,
  j.issn, j.volume, j.nomor, j.tahun_terbit AS tahun_jurnal, j.penerbit AS penerbit_jurnal
FROM koleksi k
JOIN kategori kt ON kt.id_kategori = k.id_kategori
LEFT JOIN buku b ON b.id_koleksi = k.id_koleksi
LEFT JOIN cd c ON c.id_koleksi = k.id_koleksi
LEFT JOIN jurnal j ON j.id_koleksi = k.id_koleksi
ORDER BY k.id_koleksi DESC;";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var row = new KoleksiRow();
                        row.IdKoleksi = r.GetInt32("id_koleksi");
                        row.Judul = r.GetString("judul");
                        row.FotoPath = r.IsDBNull(r.GetOrdinal("foto_path")) ? null : r.GetString("foto_path");
                        row.IdKategori = r.GetInt32("id_kategori");
                        row.NamaKategori = r.GetString("nama_kategori");
                        row.TipeKoleksi = r.GetString("tipe_koleksi");
                        row.Harga = r.GetInt32("harga");
                        row.Status = r.GetString("status");

                        string tipe = row.TipeKoleksi;

                        if (tipe == "BUKU")
                        {
                            row.Isbn = r.IsDBNull(r.GetOrdinal("isbn")) ? "" : r.GetString("isbn");
                            row.Penulis = r.IsDBNull(r.GetOrdinal("penulis")) ? "" : r.GetString("penulis");
                            row.Penerbit = r.IsDBNull(r.GetOrdinal("penerbit_buku")) ? "" : r.GetString("penerbit_buku");
                            row.TahunTerbit = r.IsDBNull(r.GetOrdinal("tahun_buku")) ? 0 : r.GetInt32("tahun_buku");
                            row.Stok = r.IsDBNull(r.GetOrdinal("stok")) ? 0 : r.GetInt32("stok");
                        }
                        else if (tipe == "CD")
                        {
                            row.JudulAlbum = r.IsDBNull(r.GetOrdinal("judul_album")) ? "" : r.GetString("judul_album");
                            row.Artis = r.IsDBNull(r.GetOrdinal("artis")) ? "" : r.GetString("artis");
                            row.DurasiMenit = r.IsDBNull(r.GetOrdinal("durasi_menit")) ? 0 : r.GetInt32("durasi_menit");
                            row.Format = r.IsDBNull(r.GetOrdinal("format")) ? "" : r.GetString("format");
                        }
                        else if (tipe == "JURNAL")
                        {
                            row.Issn = r.IsDBNull(r.GetOrdinal("issn")) ? "" : r.GetString("issn");
                            row.Volume = r.IsDBNull(r.GetOrdinal("volume")) ? "" : r.GetString("volume");
                            row.Nomor = r.IsDBNull(r.GetOrdinal("nomor")) ? "" : r.GetString("nomor");
                            row.TahunTerbit = r.IsDBNull(r.GetOrdinal("tahun_jurnal")) ? 0 : r.GetInt32("tahun_jurnal");
                            row.Penerbit = r.IsDBNull(r.GetOrdinal("penerbit_jurnal")) ? "" : r.GetString("penerbit_jurnal");
                        }

                        list.Add(row);
                    }
                }
            }

            return list;
        }

        // =========================
        // ADD (return idKoleksi baru)
        // =========================
        public int AddBuku(string judul, int idKategori, int harga, string status,
            string isbn, string penulis, string penerbit, int tahun, int stok)
        {
            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int idKoleksi;

                        using (var cmd = new MySqlCommand(
                            @"INSERT INTO koleksi (judul, foto_path, id_kategori, tipe_koleksi, harga, status)
                              VALUES (@j, NULL, @k, 'BUKU', @h, @s);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@j", judul);
                            cmd.Parameters.AddWithValue("@k", idKategori);
                            cmd.Parameters.AddWithValue("@h", harga);
                            cmd.Parameters.AddWithValue("@s", status);
                            cmd.ExecuteNonQuery();
                            idKoleksi = (int)cmd.LastInsertedId;
                        }

                        using (var cmd2 = new MySqlCommand(
                            @"INSERT INTO buku (id_koleksi, isbn, penulis, penerbit, tahun_terbit, stok)
                              VALUES (@id, @isbn, @pen, @pub, @th, @stok);", conn, tx))
                        {
                            cmd2.Parameters.AddWithValue("@id", idKoleksi);
                            cmd2.Parameters.AddWithValue("@isbn", isbn);
                            cmd2.Parameters.AddWithValue("@pen", penulis);
                            cmd2.Parameters.AddWithValue("@pub", penerbit);
                            cmd2.Parameters.AddWithValue("@th", tahun);
                            cmd2.Parameters.AddWithValue("@stok", stok);
                            cmd2.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return idKoleksi;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public int AddCd(string judul, int idKategori, int harga, string status,
            string judulAlbum, string artis, int durasiMenit, string format)
        {
            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int idKoleksi;

                        using (var cmd = new MySqlCommand(
                            @"INSERT INTO koleksi (judul, foto_path, id_kategori, tipe_koleksi, harga, status)
                              VALUES (@j, NULL, @k, 'CD', @h, @s);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@j", judul);
                            cmd.Parameters.AddWithValue("@k", idKategori);
                            cmd.Parameters.AddWithValue("@h", harga);
                            cmd.Parameters.AddWithValue("@s", status);
                            cmd.ExecuteNonQuery();
                            idKoleksi = (int)cmd.LastInsertedId;
                        }

                        using (var cmd2 = new MySqlCommand(
                            @"INSERT INTO cd (id_koleksi, judul_album, artis, durasi_menit, format)
                              VALUES (@id, @alb, @art, @dur, @fmt);", conn, tx))
                        {
                            cmd2.Parameters.AddWithValue("@id", idKoleksi);
                            cmd2.Parameters.AddWithValue("@alb", judulAlbum);
                            cmd2.Parameters.AddWithValue("@art", artis);
                            cmd2.Parameters.AddWithValue("@dur", durasiMenit);
                            cmd2.Parameters.AddWithValue("@fmt", format);
                            cmd2.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return idKoleksi;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public int AddJurnal(string judul, int idKategori, int harga, string status,
            string issn, string volume, string nomor, int tahunTerbit, string penerbit)
        {
            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int idKoleksi;

                        using (var cmd = new MySqlCommand(
                            @"INSERT INTO koleksi (judul, foto_path, id_kategori, tipe_koleksi, harga, status)
                              VALUES (@j, NULL, @k, 'JURNAL', @h, @s);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@j", judul);
                            cmd.Parameters.AddWithValue("@k", idKategori);
                            cmd.Parameters.AddWithValue("@h", harga);
                            cmd.Parameters.AddWithValue("@s", status);
                            cmd.ExecuteNonQuery();
                            idKoleksi = (int)cmd.LastInsertedId;
                        }

                        using (var cmd2 = new MySqlCommand(
                            @"INSERT INTO jurnal (id_koleksi, issn, volume, nomor, tahun_terbit, penerbit)
                              VALUES (@id, @issn, @vol, @no, @th, @pub);", conn, tx))
                        {
                            cmd2.Parameters.AddWithValue("@id", idKoleksi);
                            cmd2.Parameters.AddWithValue("@issn", issn);
                            cmd2.Parameters.AddWithValue("@vol", volume);
                            cmd2.Parameters.AddWithValue("@no", nomor);
                            cmd2.Parameters.AddWithValue("@th", tahunTerbit);
                            cmd2.Parameters.AddWithValue("@pub", penerbit);
                            cmd2.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return idKoleksi;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // =========================
        // UPDATE
        // =========================
        public void UpdateBuku(int idKoleksi, string judul, int idKategori, int harga, string status,
            string isbn, string penulis, string penerbit, int tahunTerbit, int stok)
        {
            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new MySqlCommand(
                            @"UPDATE koleksi SET judul=@j, id_kategori=@k, harga=@h, status=@s
                              WHERE id_koleksi=@id;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", idKoleksi);
                            cmd.Parameters.AddWithValue("@j", judul);
                            cmd.Parameters.AddWithValue("@k", idKategori);
                            cmd.Parameters.AddWithValue("@h", harga);
                            cmd.Parameters.AddWithValue("@s", status);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd2 = new MySqlCommand(
                            @"UPDATE buku SET isbn=@isbn, penulis=@pen, penerbit=@pub, tahun_terbit=@th, stok=@stok
                              WHERE id_koleksi=@id;", conn, tx))
                        {
                            cmd2.Parameters.AddWithValue("@id", idKoleksi);
                            cmd2.Parameters.AddWithValue("@isbn", isbn);
                            cmd2.Parameters.AddWithValue("@pen", penulis);
                            cmd2.Parameters.AddWithValue("@pub", penerbit);
                            cmd2.Parameters.AddWithValue("@th", tahunTerbit);
                            cmd2.Parameters.AddWithValue("@stok", stok);
                            cmd2.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateCd(int idKoleksi, string judul, int idKategori, int harga, string status,
            string judulAlbum, string artis, int durasiMenit, string format)
        {
            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new MySqlCommand(
                            @"UPDATE koleksi SET judul=@j, id_kategori=@k, harga=@h, status=@s
                              WHERE id_koleksi=@id;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", idKoleksi);
                            cmd.Parameters.AddWithValue("@j", judul);
                            cmd.Parameters.AddWithValue("@k", idKategori);
                            cmd.Parameters.AddWithValue("@h", harga);
                            cmd.Parameters.AddWithValue("@s", status);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd2 = new MySqlCommand(
                            @"UPDATE cd SET judul_album=@alb, artis=@art, durasi_menit=@dur, format=@fmt
                              WHERE id_koleksi=@id;", conn, tx))
                        {
                            cmd2.Parameters.AddWithValue("@id", idKoleksi);
                            cmd2.Parameters.AddWithValue("@alb", judulAlbum);
                            cmd2.Parameters.AddWithValue("@art", artis);
                            cmd2.Parameters.AddWithValue("@dur", durasiMenit);
                            cmd2.Parameters.AddWithValue("@fmt", format);
                            cmd2.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateJurnal(int idKoleksi, string judul, int idKategori, int harga, string status,
            string issn, string volume, string nomor, int tahunTerbit, string penerbit)
        {
            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new MySqlCommand(
                            @"UPDATE koleksi SET judul=@j, id_kategori=@k, harga=@h, status=@s
                              WHERE id_koleksi=@id;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", idKoleksi);
                            cmd.Parameters.AddWithValue("@j", judul);
                            cmd.Parameters.AddWithValue("@k", idKategori);
                            cmd.Parameters.AddWithValue("@h", harga);
                            cmd.Parameters.AddWithValue("@s", status);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd2 = new MySqlCommand(
                            @"UPDATE jurnal SET issn=@issn, volume=@vol, nomor=@no, tahun_terbit=@th, penerbit=@pub
                              WHERE id_koleksi=@id;", conn, tx))
                        {
                            cmd2.Parameters.AddWithValue("@id", idKoleksi);
                            cmd2.Parameters.AddWithValue("@issn", issn);
                            cmd2.Parameters.AddWithValue("@vol", volume);
                            cmd2.Parameters.AddWithValue("@no", nomor);
                            cmd2.Parameters.AddWithValue("@th", tahunTerbit);
                            cmd2.Parameters.AddWithValue("@pub", penerbit);
                            cmd2.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // =========================
        // DELETE
        // =========================
        public void Delete(int idKoleksi)
        {
            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("DELETE FROM koleksi WHERE id_koleksi=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idKoleksi);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =========================
        // FOTO
        // =========================
        public void UpdateFotoPath(int idKoleksi, string fotoPath)
        {
            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE koleksi SET foto_path=@p WHERE id_koleksi=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@p", (object)fotoPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", idKoleksi);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
