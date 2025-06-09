namespace WizardsServer;

using Npgsql;
using System;

public static class Database
{
    private static string _connectionString;

    public static void Initialize(string connectionString)
    {
        _connectionString = connectionString;

        // Проверочное подключение, чтобы убедиться, что строка валидна
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        Console.WriteLine("Подключение к базе данных установлено");
    }

    public static NpgsqlCommand CreateCommand(string sql)
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = new NpgsqlCommand(sql, connection);

        // Чтобы соединение не пропало, привязываем его к команде и закрываем позже вручную
        return command;
    }
}


