using System.Collections.Generic;
using MySql.Data.MySqlClient;
using AplikasiPerpustakaanSiJess.Data;
using AplikasiPerpustakaanSiJess.Models;
using System;

namespace AplikasiPerpustakaanSiJess.Repositories
{
    public class AnggotaRepository
    {
        public List<Anggota> GetAll()
        {
            var list = new List<Anggota>();
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                var sql = "SELECT id_anggota,nama_lengkap,nik_or_nis,jenis_kelamin,alamat,no_telp FROM anggota ORDER BY id_anggota DESC;";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new Anggota
                            {
                                IdAnggota = r.GetInt32(0),
                                NamaLengkap = r.GetString(1),
                                NikOrNis = r.IsDBNull(2) ? "" : r.GetString(2),
                                JenisKelamin = r.GetString(3),
                                Alamat = r.GetString(4),
                                NoTelp = r.GetString(5)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public Anggota GetById(int id)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(Sql.AnggotaGetById, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        return new Anggota
                        {
                            IdAnggota = r.GetInt32(0),
                            NamaLengkap = r.GetString(1),
                            NikOrNis = r.IsDBNull(2) ? "" : r.GetString(2),
                            JenisKelamin = r.GetString(3),
                            Alamat = r.GetString(4),
                            NoTelp = r.GetString(5)
                        };
                    }
                }
            }
        }

        public void Add(Anggota a)
        {
            if (!string.IsNullOrWhiteSpace(a.NikOrNis) && ExistsNikOrNis(a.NikOrNis))
                throw new Exception("NIK/NIS sudah terdaftar. Gunakan yang lain atau kosongkan bila tidak ada.");

            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                var sql = @"INSERT INTO anggota(nama_lengkap,nik_or_nis,jenis_kelamin,alamat,no_telp)
                            VALUES(@n,@nik,@jk,@al,@t);";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@n", a.NamaLengkap);
                    cmd.Parameters.AddWithValue("@nik", string.IsNullOrWhiteSpace(a.NikOrNis) ? (object)System.DBNull.Value : a.NikOrNis);
                    cmd.Parameters.AddWithValue("@jk", a.JenisKelamin);
                    cmd.Parameters.AddWithValue("@al", a.Alamat);
                    cmd.Parameters.AddWithValue("@t", a.NoTelp);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(Anggota a)
        {
            if (!string.IsNullOrWhiteSpace(a.NikOrNis) && ExistsNikOrNis(a.NikOrNis, a.IdAnggota))
                throw new Exception("NIK/NIS sudah dipakai anggota lain."); 
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                var sql = @"UPDATE anggota SET nama_lengkap=@n, nik_or_nis=@nik, jenis_kelamin=@jk, alamat=@al, no_telp=@t
                            WHERE id_anggota=@id;";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@n", a.NamaLengkap);
                    cmd.Parameters.AddWithValue("@nik", string.IsNullOrWhiteSpace(a.NikOrNis) ? (object)System.DBNull.Value : a.NikOrNis);
                    cmd.Parameters.AddWithValue("@jk", a.JenisKelamin);
                    cmd.Parameters.AddWithValue("@al", a.Alamat);
                    cmd.Parameters.AddWithValue("@t", a.NoTelp);
                    cmd.Parameters.AddWithValue("@id", a.IdAnggota);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public bool TryAddAnggota(Anggota a, out string message)
        {
            try
            {
                // Pengecekan Duplikat NIK
                if (!string.IsNullOrWhiteSpace(a.NikOrNis) && ExistsNikOrNis(a.NikOrNis))
                {
                    message = "NIK/NIS sudah terdaftar.";
                    return false;
                }

                // Jika NIK kosong, cek Nama + Telp (mencegah duplicate data kosong)
                if (string.IsNullOrWhiteSpace(a.NikOrNis) && ExistsNamaDanTelp(a.NamaLengkap, a.NoTelp))
                {
                    message = "Anggota dengan Nama dan No Telp ini sudah ada.";
                    return false;
                }

                Add(a);
                message = "Anggota berhasil ditambahkan.";
                return true;
            }
            catch (Exception ex)
            {
                message = "Gagal: " + ex.Message;
                return false;
            }
        }

        private bool ExistsNamaDanTelp(string nama, string telp)
        {
            using (var conn = Db.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM anggota WHERE nama_lengkap=@n AND no_telp=@t";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@n", nama.Trim());
                    cmd.Parameters.AddWithValue("@t", telp.Trim());
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public void Delete(int id)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("DELETE FROM anggota WHERE id_anggota=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ExistsNikOrNis(string nikOrNis, int? excludeIdAnggota = null)
        {
            if (string.IsNullOrWhiteSpace(nikOrNis)) return false; // karena opsional

            using (var conn = Data.Db.GetConnection())
            {
                conn.Open();

                string sql = "SELECT COUNT(*) FROM anggota WHERE nik_or_nis=@no";
                if (excludeIdAnggota.HasValue)
                    sql += " AND id_anggota<>@id";

                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@no", nikOrNis.Trim());
                    if (excludeIdAnggota.HasValue)
                        cmd.Parameters.AddWithValue("@id", excludeIdAnggota.Value);

                    int count = System.Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    
    }
}