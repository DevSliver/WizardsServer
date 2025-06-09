namespace WizardsServer;

using System.Collections.Concurrent;
using System;
using System.Net;
using System.Text;
using NetCoreServer;
using WizardsServer.GameLogic;

public class Server : TcpServer
{
    private static Server? _instance;
    public static Server Instance => _instance ?? throw new InvalidOperationException("Server is not initialized");

    private CommandProcessor _commandProcessor;
    private GameManager _gameManager;
    public CommandProcessor CommandProcessor => _commandProcessor;
    public GameManager GameManager => _gameManager;

    private ConcurrentDictionary<TcpSession, Client> _clients;
    public ConcurrentDictionary<TcpSession, Client> Clients => _clients;

    public Server(IPAddress address, int port) : base(address, port)
    {
        if (_instance != null)
            throw new InvalidOperationException("Server instance already created");

        _clients = new();
        _commandProcessor = new();
        _gameManager = new();

        _instance = this;
    }
    protected override TcpSession CreateSession()
    {
        var session = new Session(this);
        var client = new Client(session);
        _clients.TryAdd(session, client);
        return session;
    }
    protected override void OnDisconnected(TcpSession session)
    {
        _clients.TryRemove(session, out _);
    }
}
public class Session : TcpSession
{
    private Server _server;
    public Session(Server server) : base(server)
    {
        _server = server;
    }
    protected override void OnConnected()
    {
        Console.WriteLine($"Сессия с ID {Id} подключена.");
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        Console.WriteLine($"Получено от сессии {Id}: {message}");
        if(_server.Clients.TryGetValue(this, out var client))
        {
            client.RecieveCommand(message);
        }
    }
    protected override void OnDisconnected()
    {
        Console.WriteLine($"Сессия с ID {Id} отключена.");
    }
    protected override void OnError(System.Net.Sockets.SocketError error)
    {
        Console.WriteLine($"Ошибка в сессии {Id}: {error}");
    }
}
public class Client
{
    public int? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
    private Session _session;
    public Session Session => _session;

    public Match? Match { get; set; }
    public Player? Player { get; set; }
    public bool InMatch => Player != null;

    public Client(Session session)
    {
        _session = session;
    }

    public void Authenticate(int userId)
    {
        UserId = userId;
    }
    public void RecieveCommand(string command)
    {
        if (InMatch)
            Match?.CommandProcessor.ProcessCommand(command, this);
        else
            Server.Instance.CommandProcessor.ProcessCommand(command, this);
    }
    public bool SendAsync(string message)
    {
        return Session.SendAsync(message);
    }
}
