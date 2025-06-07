using System.Net;

namespace WizardsServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
            var server = new Server(IPAddress.Any, Convert.ToInt32(port));
            server.Start();
            Console.WriteLine($"Сервер запущен на порту {port}. Нажмите Ctrl+C для остановки...");

            // Ожидаем завершения работы сервера
            await Task.Delay(-1);
        }
    }
}
