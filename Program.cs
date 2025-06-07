using System.Net;

namespace WizardsServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
            var server = new Server(IPAddress.Any, Convert.ToInt32(port));
            server.Start();
            Console.WriteLine($"Сервер запущен на порту {port}. Нажмите Enter для остановки.");
            Console.ReadLine();
            server.Stop();
            Console.WriteLine("Сервер остановлен.");
        }
    }
}
