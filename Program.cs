using System.Collections.Concurrent;
using System.Net;
using WizardsServer.ServerLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer;
internal class Program
{
    // Очередь для передачи действий в главный поток
    private static readonly ConcurrentQueue<Action> _mainThreadActions = new();
    private static readonly AutoResetEvent _newActionEvent = new(false);

    static async Task Main(string[] args)
    {
        var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
        var server = new Server(IPAddress.Any, Convert.ToInt32(port));
        server.Start();
        Console.WriteLine($"Сервер запущен на порту {port}.");

        string connectionString = $"Host={Environment.GetEnvironmentVariable("PGHOST")};" +
                       $"Port={Environment.GetEnvironmentVariable("PGPORT")};" +
                       $"Username={Environment.GetEnvironmentVariable("PGUSER")};" +
                       $"Password={Environment.GetEnvironmentVariable("PGPASSWORD")};" +
                       $"Database={Environment.GetEnvironmentVariable("PGDATABASE")};";
        Database.Initialize(connectionString);

        await RunMainLoop();
    }
    public static void EnqueueMainThreadAction(Action action)
    {
        _mainThreadActions.Enqueue(action);
        _newActionEvent.Set();
    }
    private static async Task RunMainLoop()
    {
        while (true)
        {
            _newActionEvent.WaitOne(50);
            while (_mainThreadActions.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при выполнении действия: {ex}");
                }
            }
            await Task.Yield();
        }
    }
}