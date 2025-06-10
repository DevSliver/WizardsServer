using WizardsServer.GameLogic;

namespace WizardsServer.ServerLogic;

public class Client
{
    public int? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
    private Session _session;

    public Match? Match { get; set; }
    public Player? Player { get; set; }
    public bool InMatch => Player != null;

    private CommandQueue _commandQueue = new();
    private readonly object _lock = new();
    private long _commandNumber = 1;

    public Client(Session session)
    {
        _session = session;
    }
    public void Authenticate(int userId)
    {
        UserId = userId;
    }
    public void ReceiveCommand(long number, string[] args)
    {
        lock (_lock)
        {
            _commandQueue.Enqueue(number, args);

            if (!_commandQueue.TryDequeue(out args))
                return;

            switch (CommandProcessor.ProcessCommand(args, out args))
            {
                case "server":
                    Server.Instance.Process(args, this);
                    break;
                case "match":
                    Player?.Process(args, this);
                    break;
            }
        }
    }
    public bool SendAsync(string message)
    {
        lock (_lock)
        {
            message = $"{_commandNumber++} {message}";
            Console.WriteLine($"{_session.Id} - Отправлено сообщение: {message}");
            return _session.SendAsync(message);
        }
    }
    public void SetMatchInfo(Match? match, Player? player)
    {
        Match = match;
        Player = player;
    }
}
