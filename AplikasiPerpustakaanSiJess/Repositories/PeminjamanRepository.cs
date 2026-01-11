using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using AplikasiPerpustakaanSiJess.Data;
using AplikasiPerpustakaanSiJess.Models;

namespace AplikasiPerpustakaanSiJess.Repositories
{
    public class PeminjamanRepository
    {
        public int CreatePeminjaman(int idAnggota, List<int> idKoleksiList)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        int idPinjam;
                        using (MySqlCommand cmdH = new MySqlCommand(
                            "INSERT INTO peminjaman(id_anggota,tgl_pinjam,tgl_jatuh_tempo) VALUES(@a,NOW(),DATE_ADD(NOW(),INTERVAL 7 DAY));",
                            conn, tx))
                        {
                            cmdH.Parameters.AddWithValue("@a", idAnggota);
                            cmdH.ExecuteNonQuery();

                            cmdH.CommandText = "SELECT LAST_INSERT_ID();";
                            idPinjam = Convert.ToInt32(cmdH.ExecuteScalar());
                        }

                        foreach (int idKoleksi in idKoleksiList)
                        {
                            // 1. Cek ketersediaan
                            using (MySqlCommand cek = new MySqlCommand("SELECT status FROM koleksi WHERE id_koleksi=@k LIMIT 1;", conn, tx))
                            {
                                cek.Parameters.AddWithValue("@k", idKoleksi);
                                string status = (cek.ExecuteScalar() ?? "").ToString();
                                if (status != "TERSEDIA")
                                    throw new Exception("Koleksi ID " + idKoleksi + " tidak tersedia (status: " + status + ").");
                            }

                            // 2. Insert Detail
                            using (MySqlCommand cmdD = new MySqlCommand("INSERT INTO peminjaman_detail(id_pinjam,id_koleksi,status_item) VALUES(@p,@k,'DIPINJAM');", conn, tx))
                            {
                                cmdD.Parameters.AddWithValue("@p", idPinjam);
                                cmdD.Parameters.AddWithValue("@k", idKoleksi);
                                cmdD.ExecuteNonQuery();
                            }

                            // 3. Update Status Koleksi
                            using (MySqlCommand up = new MySqlCommand("UPDATE koleksi SET status='DIPINJAM' WHERE id_koleksi=@k;", conn, tx))
                            {
                                up.Parameters.AddWithValue("@k", idKoleksi);
                                up.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return idPinjam;
                    }
                    catch (Exception)
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<PeminjamanInfo> GetLoansByAnggota(int idAnggota, string statusFilter)
        {
            var list = new List<PeminjamanInfo>();
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                var sql = @"
SELECT d.id_detail, p.id_pinjam, d.id_koleksi, k.judul, p.tgl_pinjam, p.tgl_jatuh_tempo, d.status_item, d.denda
FROM peminjaman p
JOIN peminjaman_detail d ON d.id_pinjam = p.id_pinjam
JOIN koleksi k ON k.id_koleksi = d.id_koleksi
WHERE p.id_anggota=@a
  AND (@s='SEMUA' OR d.status_item=@s)
ORDER BY d.id_detail DESC;";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@a", idAnggota);
                    cmd.Parameters.AddWithValue("@s", statusFilter ?? "SEMUA");

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new PeminjamanInfo
                            {
                                IdDetail = r.GetInt32(0),
                                IdPinjam = r.GetInt32(1),
                                IdKoleksi = r.GetInt32(2),
                                Judul = r.GetString(3),
                                TglPinjam = r.GetDateTime(4),
                                TglJatuhTempo = r.GetDateTime(5),
                                StatusItem = r.GetString(6),
                                Denda = r.GetInt32(7)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public List<dynamic> GetAllTransaksi(string statusFilter, string search)
        {
            var list = new List<dynamic>();
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                var sql = @"
SELECT
 d.id_detail, d.status_item, d.denda,
 p.id_pinjam, p.tgl_pinjam, p.tgl_jatuh_tempo,
 a.id_anggota, a.nama_lengkap,
 k.id_koleksi, k.judul, k.harga
FROM peminjaman_detail d
JOIN peminjaman p ON p.id_pinjam = d.id_pinjam
JOIN anggota a ON a.id_anggota = p.id_anggota
JOIN koleksi k ON k.id_koleksi = d.id_koleksi
WHERE (@s='SEMUA' OR d.status_item=@s)
  AND (@q='' OR k.judul LIKE CONCAT('%',@q,'%') OR a.nama_lengkap LIKE CONCAT('%',@q,'%'))
ORDER BY d.id_detail DESC;";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@s", statusFilter ?? "SEMUA");
                    cmd.Parameters.AddWithValue("@q", search ?? "");

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new
                            {
                                IdDetail = r.GetInt32(0),
                                StatusItem = r.GetString(1),
                                Denda = r.GetInt32(2),
                                IdPinjam = r.GetInt32(3),
                                TglPinjam = r.GetDateTime(4),
                                TglJatuhTempo = r.GetDateTime(5),
                                IdAnggota = r.GetInt32(6),
                                NamaAnggota = r.GetString(7),
                                IdKoleksi = r.GetInt32(8),
                                Judul = r.GetString(9),
                                Harga = r.GetInt32(10),
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void SetKembali(int idDetail, int idKoleksi, int denda)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand(
                            "UPDATE peminjaman_detail SET status_item='KEMBALI', tgl_kembali=NOW(), denda=@d WHERE id_detail=@id;",
                            conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@d", denda);
                            cmd.Parameters.AddWithValue("@id", idDetail);
                            cmd.ExecuteNonQuery();
                        }

                        using (MySqlCommand up = new MySqlCommand("UPDATE koleksi SET status='TERSEDIA' WHERE id_koleksi=@k;", conn, tx))
                        {
                            up.Parameters.AddWithValue("@k", idKoleksi);
                            up.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
        }

        public void SetHilang(int idDetail, int idKoleksi, int denda)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand(
                            "UPDATE peminjaman_detail SET status_item='HILANG', tgl_kembali=NOW(), denda=@d WHERE id_detail=@id;",
                            conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@d", denda);
                            cmd.Parameters.AddWithValue("@id", idDetail);
                            cmd.ExecuteNonQuery();
                        }

                        using (MySqlCommand up = new MySqlCommand("UPDATE koleksi SET status='HILANG' WHERE id_koleksi=@k;", conn, tx))
                        {
                            up.Parameters.AddWithValue("@k", idKoleksi);
                            up.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
        }


        public void UpdateTanggalPeminjaman(int idPinjam, DateTime tglPinjam, DateTime tglJatuhTempo)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                // Update kedua kolom sekaligus: tgl_pinjam dan tgl_jatuh_tempo
                string sql = "UPDATE peminjaman SET tgl_pinjam=@tp, tgl_jatuh_tempo=@tj WHERE id_pinjam=@id;";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tp", tglPinjam);
                    cmd.Parameters.AddWithValue("@tj", tglJatuhTempo);
                    cmd.Parameters.AddWithValue("@id", idPinjam);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteDetail(int idDetail)
        {
            using (var conn = AplikasiPerpustakaanSiJess.Data.Db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int idPinjam = 0;
                        int idKoleksi = 0;
                        string statusItem = "";

                        using (var cmdGet = new MySql.Data.MySqlClient.MySqlCommand(
                            "SELECT id_pinjam,id_koleksi,status_item FROM peminjaman_detail WHERE id_detail=@id;", conn, tx))
                        {
                            cmdGet.Parameters.AddWithValue("@id", idDetail);
                            using (var r = cmdGet.ExecuteReader())
                            {
                                if (!r.Read()) throw new System.Exception("Data detail tidak ditemukan.");
                                idPinjam = r.GetInt32(0);
                                idKoleksi = r.GetInt32(1);
                                statusItem = r.GetString(2);
                            }
                        }

                        // kalau sedang DIPINJAM lalu dihapus, balikin status koleksi ke TERSEDIA
                        if (statusItem == "DIPINJAM")
                        {
                            using (var cmdUp = new MySql.Data.MySqlClient.MySqlCommand(
                                "UPDATE koleksi SET status='TERSEDIA' WHERE id_koleksi=@k;", conn, tx))
                            {
                                cmdUp.Parameters.AddWithValue("@k", idKoleksi);
                                cmdUp.ExecuteNonQuery();
                            }
                        }

                        using (var cmdDel = new MySql.Data.MySqlClient.MySqlCommand(
                            "DELETE FROM peminjaman_detail WHERE id_detail=@id;", conn, tx))
                        {
                            cmdDel.Parameters.AddWithValue("@id", idDetail);
                            cmdDel.ExecuteNonQuery();
                        }

                        // kalau header peminjaman sudah tidak punya detail, hapus header juga
                        int count = 0;
                        using (var cmdCnt = new MySql.Data.MySqlClient.MySqlCommand(
                            "SELECT COUNT(*) FROM peminjaman_detail WHERE id_pinjam=@p;", conn, tx))
                        {
                            cmdCnt.Parameters.AddWithValue("@p", idPinjam);
                            count = System.Convert.ToInt32(cmdCnt.ExecuteScalar());
                        }

                        if (count == 0)
                        {
                            using (var cmdDelH = new MySql.Data.MySqlClient.MySqlCommand(
                                "DELETE FROM peminjaman WHERE id_pinjam=@p;", conn, tx))
                            {
                                cmdDelH.Parameters.AddWithValue("@p", idPinjam);
                                cmdDelH.ExecuteNonQuery();
                            }
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

    }
}