using System;
using MySql.Data.MySqlClient;
using AplikasiPerpustakaanSiJess.Data;

namespace AplikasiPerpustakaanSiJess.Repositories
{
    public class AuthRepository
    {
        public (bool ok, int? idAnggota, string role, string message) Login(string username, string password)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();

                var sql = @"
SELECT id_anggota, role
FROM akun
WHERE username=@u AND password_hash = SHA2(@p, 256)
LIMIT 1;";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return (false, null, "", "Username/password salah.");

                        int? idAnggota = r.IsDBNull(0) ? (int?)null : r.GetInt32(0);
                        string role = r.GetString(1);
                        return (true, idAnggota, role, "OK");
                    }
                }
            }
        }

        public (bool ok, string message) SignupAnggota(
            string namaLengkap, string nikOrNis, string jenisKelamin, string alamat, string noTelp,
            string username, string password)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        var sqlA = @"
INSERT INTO anggota (nama_lengkap, nik_or_nis, jenis_kelamin, alamat, no_telp)
VALUES (@nama, @nik, @jk, @alamat, @telp);";

                        using (MySqlCommand cmdA = new MySqlCommand(sqlA, conn, tx))
                        {
                            cmdA.Parameters.AddWithValue("@nama", namaLengkap);
                            cmdA.Parameters.AddWithValue("@nik", string.IsNullOrWhiteSpace(nikOrNis) ? (object)DBNull.Value : nikOrNis);
                            cmdA.Parameters.AddWithValue("@jk", jenisKelamin);
                            cmdA.Parameters.AddWithValue("@alamat", alamat);
                            cmdA.Parameters.AddWithValue("@telp", noTelp);
                            cmdA.ExecuteNonQuery();
                        }

                        // Menggunakan cmd eksplisit untuk scalar agar tetap dalam transaksi
                        int idAnggota;
                        using (MySqlCommand cmdGetId = new MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx))
                        {
                            idAnggota = Convert.ToInt32(cmdGetId.ExecuteScalar());
                        }

                        var sqlU = @"
INSERT INTO akun (id_anggota, username, password_hash, role)
VALUES (@id, @u, SHA2(@p,256), 'ANGGOTA');";

                        using (MySqlCommand cmdU = new MySqlCommand(sqlU, conn, tx))
                        {
                            cmdU.Parameters.AddWithValue("@id", idAnggota);
                            cmdU.Parameters.AddWithValue("@u", username);
                            cmdU.Parameters.AddWithValue("@p", password);
                            cmdU.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return (true, "Signup berhasil. Silakan login.");
                    }
                    catch (MySqlException ex)
                    {
                        tx.Rollback();
                        if (ex.Message.Contains("uk_akun_username")) return (false, "Username sudah dipakai.");
                        if (ex.Message.Contains("uk_anggota_no")) return (false, "NIK/NIS sudah terdaftar.");
                        return (false, "Gagal signup: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return (false, "Gagal signup: " + ex.Message);
                    }
                }
            }
        }
    }
}