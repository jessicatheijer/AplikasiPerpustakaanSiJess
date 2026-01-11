using System.Collections.Generic;
using MySql.Data.MySqlClient;
using AplikasiPerpustakaanSiJess.Data;
using AplikasiPerpustakaanSiJess.Models;

namespace AplikasiPerpustakaanSiJess.Repositories
{
    public class KategoriRepository
    {
        public List<Kategori> GetAll()
        {
            var list = new List<Kategori>();
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(Sql.KategoriGetAll, conn))
                {
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new Kategori
                            {
                                IdKategori = r.GetInt32(0),
                                NamaKategori = r.GetString(1)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void Add(string nama)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("INSERT INTO kategori(nama_kategori) VALUES(@n);", conn))
                {
                    cmd.Parameters.AddWithValue("@n", nama);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(int id, string nama)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("UPDATE kategori SET nama_kategori=@n WHERE id_kategori=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@n", nama);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (MySqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("DELETE FROM kategori WHERE id_kategori=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}