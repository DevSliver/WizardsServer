namespace WizardsServer.ServerLogic;

using NetCoreServer;
using System.Net;
using WizardsServer.ServerLogic.CommandSystem;
using WizardsServer.services;

internal class Server : TcpServer
{
    private static Server? _instance;
    public static Server Instance => _instance ?? throw new InvalidOperationException("Server is not initialized");

    private readonly object _lock = new();
    private HashSet<Session> _sessions;
    private Dictionary<int, Session> _authedSessions;

    public GameManager GameManager { get; private set; }
    public AuthService AuthService { get; private set; }
    public NewsService NewsService { get; private set; }
    public MatchmackingService MatchmakingService { get; private set; }

    public Server(IPAddress address, int port) : base(address, port)
    {
        if (_instance != null)
            throw new InvalidOperationException("Server instance already created");

        _instance = this;

        _sessions = new();
        _authedSessions = new();

        GameManager = new();
        AuthService = new();
        NewsService = new();
        MatchmakingService = new();
    }
    public bool IsUserAuthed(int userId) =>
        _authedSessions.ContainsKey(userId);
    public void AuthUser(Session session, int userId)
    {
        if (!session.IsAuthed)
            session.Auth(userId);
        else
            _authedSessions[userId] = session;
    }
    protected override TcpSession CreateSession()
    {
        lock (_lock)
        {
            var session = new Session(this);
            _sessions.Add(session);
            return session;
        }
    }
    public void OnSessionDisconnected(Session session)
    {
        lock (_lock)
        {
            MatchmakingService.CancelSearching(session);
            session.Player?.Disconnect();
            _authedSessions.Remove(session.UserId);
            _sessions.Remove(session);
            Console.WriteLine($"{session.Id} - Сессия отключена.");
        }
    }
    protected override void OnConnected(TcpSession tcpSession)
    {
        Console.WriteLine($"{tcpSession.Id} - Сессия подключена.");
    }
}
