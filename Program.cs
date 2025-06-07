using System.Net;

namespace WizardsServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Настройка сервера и запуск.
            var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
            var server = new Server(IPAddress.Any, Convert.ToInt32(port));
            server.Start();
            Console.WriteLine($"Сервер запущен на порту {port}.");

            // Настройка базы данных и подключение.
            string connectionString = $"Host={Environment.GetEnvironmentVariable("PGHOST")};" +
                           $"Port={Environment.GetEnvironmentVariable("PGPORT")};" +
                           $"Username={Environment.GetEnvironmentVariable("PGUSER")};" +
                           $"Password={Environment.GetEnvironmentVariable("PGPASSWORD")};" +
                           $"Database={Environment.GetEnvironmentVariable("PGDATABASE")};";
            Database.Initialize(connectionString);

            // Сервер не отключается сам.
            await Task.Delay(-1);
        }
    }
}
