using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace AplikasiPerpustakaanSiJess.Data
{
    public static class Db
    {
        public static MySqlConnection GetConnection()
        {
            var csSetting = ConfigurationManager.ConnectionStrings["LibraryDb"];

            if (csSetting == null || string.IsNullOrWhiteSpace(csSetting.ConnectionString))
                throw new Exception("ConnectionString 'LibraryDb' tidak ditemukan / kosong di App.config.");

            return new MySqlConnection(csSetting.ConnectionString);
        }
    }
}
