using System;
using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace WizardsServer
{
    public static class Database
    {
        private static NpgsqlConnection _connection;

        public static void Initialize(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
            Console.WriteLine("Подключение к базе данных установлено");
        }

        public static NpgsqlCommand CreateCommand(string sql)
        {
            return new NpgsqlCommand(sql, _connection);
        }
    }

}
