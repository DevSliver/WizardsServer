namespace WizardsServer.ServerLogic;

using NetCoreServer;
using System.Collections.Concurrent;
using System.Net;
using WizardsServer.GameLogic;
public class Server : TcpServer, ICommandProcessor
{
    private static Server? _instance;
    public static Server Instance => _instance ?? throw new InvalidOperationException("Server is not initialized");

    private ConcurrentDictionary<TcpSession, Client> _clients;
    private GameManager _gameManager;
    private AuthService _authService;
    private NewsService _newsService;
    private MatchmakingService _matchmakingService;

    public ConcurrentDictionary<TcpSession, Client> Clients => _clients;
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
        OnClientDisconnected(tcpSession);
    }
    protected override void OnDisconnected(TcpSession tcpSession)
    {
        Console.WriteLine($"Сессия с ID {tcpSession.Id} отключена.");
        OnClientDisconnected(tcpSession);
    }
    private void OnClientDisconnected(TcpSession tcpSession)
    {
        _clients[tcpSession].Player?.Disconnect()
        _clients.TryRemove(tcpSession, out _);
    }
    protected override void OnConnected(TcpSession tcpSession)
    {
        Console.WriteLine($"Сессия с ID {tcpSession.Id} подключена.");
    }

    public void Process(string[] args, Client client)
    {
        switch (CommandProcessor.ProcessCommand(args, out args))
        {
            case "auth":
                AuthService.Process(args, client);
                break;
            case "news":
                NewsService.Process(args, client);
                break;
            case "matchmacking":
                MatchmakingService.Process(args, client);
                break;
        }
    }
}
