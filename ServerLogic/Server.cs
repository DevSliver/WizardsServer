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

    private ConcurrentDictionary<TcpSession, Client> _clients;
    private CommandProcessor _commandProcessor;
    private GameManager _gameManager;
    private AuthService _authService;
    private NewsService _newsService;
    private MatchmakingService _matchmakingService;

    public ConcurrentDictionary<TcpSession, Client> Clients => _clients;
    public CommandProcessor CommandProcessor => _commandProcessor;
    public GameManager GameManager => _gameManager;
    public AuthService AuthService => _authService;
    public NewsService NewsService => _newsService;
    public MatchmakingService MatchmakingService => _matchmakingService;

    public Server(IPAddress address, int port) : base(address, port)
    {
        if (_instance != null)
            throw new InvalidOperationException("Server instance already created");

        _instance = this;

        _clients = new ConcurrentDictionary<TcpSession, Client>();

        // CommandProcessor must be first!
        _commandProcessor = new CommandProcessor();
        _gameManager = new GameManager();
        _authService = new AuthService();
        _newsService = new NewsService();
        _matchmakingService = new MatchmakingService();
    }
    protected override TcpSession CreateSession()
    {
        var session = new Session(this);
        var client = new Client(session);
        _clients.TryAdd(session, client);
        return session;
    }
    public void OnSessionError(TcpSession tcpSession, System.Net.Sockets.SocketError error)
    {
        Console.WriteLine($"Ошибка в сессии {tcpSession.Id}: {error}");
        // OnDisconnected(tcpSession);
    }
    protected override void OnDisconnected(TcpSession tcpSession)
    {
        Client client = Clients[tcpSession];
        Session session = client.Session;

        Console.WriteLine($"Сессия с ID {tcpSession.Id} отключена.");

        client.Match?.OnPlayerDisconnect(client);
        client.Dispose();
        _clients.TryRemove(tcpSession, out _);
    }
    protected override void OnConnected(TcpSession tcpSession)
    {
        Console.WriteLine($"Сессия с ID {tcpSession.Id} подключена.");
    }
}
public class Session : TcpSession
{
    private Server _server;
    public Client Client => _server.Clients[this];

    public Session(Server server) : base(server)
    {
        _server = server;
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        Console.WriteLine($"{Id} - Получено сообщение: {message}");
        Client.RecieveCommand(message);
    }
    protected override void OnError(System.Net.Sockets.SocketError error)
    {
        _server.OnSessionError(this, error);
    }
}
public class Client : IDisposable
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

        CommandProcessor.Global.Subscribe("server", OnServerCommand);
        CommandProcessor.Global.Subscribe("match", OnMatchCommand);
    }

    private void OnServerCommand(string[] args, Client client) =>
        Server.Instance.CommandProcessor.ProcessCommand(args, client);
    private void OnMatchCommand(string[] args, Client client) =>
        Match?.CommandProcessor.ProcessCommand(args, client);

    public void Authenticate(int userId)
    {
        UserId = userId;
    }
    public void RecieveCommand(string command)
    {
        var args = CommandProcessor.SplitCommandLine(command);
        CommandProcessor.Global.ProcessCommand(args, this);
    }
    public int sends = 0;
    public bool SendAsync(string message)
    {
        sends++;
        Console.WriteLine($"{Session.Id} - Отправлено сообщение {sends}: {message}");
        return Session.SendAsync(message);
    }

    public void SetMatchInfo(Match? match, Player? player)
    {
        Match = match;
        Player = player;
    }

    public void Dispose()
    {
        CommandProcessor.Global.Unsubscribe("Server", OnServerCommand);
        CommandProcessor.Global.Unsubscribe("Match", OnMatchCommand);
    }
}
