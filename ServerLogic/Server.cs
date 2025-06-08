namespace WizardsServer;

using System.Collections.Concurrent;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

public class Server : TcpServer
{
    private readonly ConcurrentDictionary<Guid, Client> _clients = new();
    private static Server? _instance;

    public static Server Instance => _instance ?? throw new InvalidOperationException("Server is not initialized");

    public Server(IPAddress address, int port) : base(address, port)
    {
        if (_instance != null)
            throw new InvalidOperationException("Server instance already created");

        _instance = this;
    }
    protected override TcpSession CreateSession()
    {
        var client = new Client(this);
        client.DisconnectedEvent += OnClientDisconnected;
        _clients[client.Id] = client;
        return client;
    }

    private void OnClientDisconnected(Client client)
    {
        _clients.TryRemove(client.Id, out _);
        client.DisconnectedEvent -= OnClientDisconnected;
    }

    public ConcurrentDictionary<Guid, Client> GetClients() => _clients;
}

public class Client : TcpSession
{
    public int? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
    public event Action<Client>? DisconnectedEvent;

    public Client(TcpServer server) : base(server) { }

    public void Authenticate(int userId)
    {
        UserId = userId;
    }
    protected override void OnConnected()
    {
        Console.WriteLine($"Клиент с ID {Id} подключен.");
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        Console.WriteLine($"Получено от клиента {Id}: {message}");
        CommandProcessor.Instance.ProcessCommand(message, this);
    }
    protected override void OnDisconnected()
    {
        DisconnectedEvent?.Invoke(this);
        Console.WriteLine($"Клиент с ID {Id} отключен.");
    }
    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Ошибка в сессии клиента {Id}: {error}");
    }
}
