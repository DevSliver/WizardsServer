using NetCoreServer;
using Npgsql;
using System.Diagnostics;
using WizardsServer.GameLogic;
using WizardsServer.ServerLogic.CommandSystem;
using WizardsServer.services;

namespace WizardsServer.ServerLogic;

public class Session : TcpSession
{
    new Server Server { get; }
    public int UserId { get; private set; } = -1;
    public bool IsAuthed => UserId != -1;

    private const long LengthPrefixSize = 4;
    private long _messageLength = 0;
    private long _messageBytesRead = 0;
    private List<byte> _messageBuffer = new List<byte>();

    private CommandProcessor _processor;
    private readonly object _lock = new();

    public Player? Player { get; private set; }
    public bool InMatch => Player != null;

    public Session(Server server) : base(server)
    {
        Server = server;
        _processor = new CommandProcessor();
        GeneralCommands();
        PlayerCommands();
    }
    private void GeneralCommands()
    {
        _processor["AuthService.Login"] = (s, c) =>
        {
            if (c.Args.TryGet("Username", out string username) &&
            c.Args.TryGet("Password", out string password))
                Send(Command.Response(c, Server.AuthService.Login(s, username, password)));
            else
                Send(Command.MsgResponse(c, "usage"));
        };
        _processor["AuthService.Register"] = (s, c) =>
        {
            if (c.Args.TryGet("Username", out string username) &&
            c.Args.TryGet("Password", out string password))
                Send(Command.Response(c, Server.AuthService.Register(s, username, password)));
            else
                Send(Command.MsgResponse(c, "usage"));
        };
        _processor["NewsService.Get"] = (s, c) =>
        {
            if (c.Args.TryGet("Number", out int number))
                Send(Command.Response(c, Server.NewsService.Get(number)));
            else
                Send(Command.MsgResponse(c, "usage"));
        };
    }
    private void PlayerCommands()
    {
        _processor["Player.Loaded"] =
            (s, c) => Player?.Loaded();
    }
    private void AuthCommands()
    {
        _processor["Matchmaking.StartSearching"] =
            (s, c) => Send(Command.Response(c, Server.MatchmakingService.StartSearching(s)));
        _processor["Matchmaking.CancelSearching"] =
            (s, c) => Send(Command.Response(c, Server.MatchmakingService.CancelSearching(s)));
    }
    public void Auth(int userId)
    {
        if (!IsAuthed)
            return;
        UserId = userId;
        Server.AuthUser(this, userId);
        AuthCommands();
    }
    public bool TrySetPlayer(Player? player)
    {
        if (player == null)
        {
            Player = null;
            return true;
        }
        if (player.Session == this)
        {
            Player = player;
            return true;
        }
        return false;
    }
    public void Send(Command? command)
    {
        if (command == null)
            return;
        try
        {
            byte[] payload = CommandProcessor.Serialize(command);
            int length = payload.Length;
            byte[] message = new byte[4 + length];
            BitConverter.GetBytes(length).CopyTo(message, 0);
            payload.CopyTo(message, 4);
            base.SendAsync(message);
            Console.WriteLine($"Отправлена команда.\n{command.ToString()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{Id} - Ошибка отправки команды {command.Id}. Путь: \"{command.Path}\":");
            Console.WriteLine(ex.ToString());
        }
    }
    public void HandleMessage(byte[] message)
    {
        try
        {
            var command = CommandProcessor.Deserialize(message);
            Console.WriteLine($"Получена команда.\n{command.ToString()}");
            Program.EnqueueMainThreadAction(() =>
            {
                try
                {
                    _processor.Process(this, command);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{Id} - Ошибка выполнения команды {command.Id}. Путь: \"{command.Path}\":");
                    Console.WriteLine(ex.ToString());
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{Id} - Ошибка десериализации команды:");
            Console.WriteLine(ex.ToString());

            Command command = new Command("Response.Deserialization");
            Send(command);
        }
    }
    protected override void OnDisconnected()
    {
        Server.OnSessionDisconnected(this);
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        lock (_lock)
        {
            while (offset < size)
            {
                if (_messageLength == 0)
                {
                    if (size - offset >= LengthPrefixSize)
                    {
                        _messageLength = BitConverter.ToInt32(buffer, (int)offset);
                        offset += LengthPrefixSize;
                    }
                    else
                    {
                        return;
                    }
                }
                long bytesToRead = Math.Min(_messageLength - _messageBytesRead, size - offset);
                _messageBuffer.AddRange(buffer[(int)offset..(int)(offset + bytesToRead)]);
                _messageBytesRead += bytesToRead;
                offset += bytesToRead;
                if (_messageBytesRead == _messageLength)
                {
                    HandleMessage(_messageBuffer.ToArray());
                    _messageBuffer.Clear();
                    _messageLength = 0;
                    _messageBytesRead = 0;
                }
            }
        }
    }
    protected override void OnError(System.Net.Sockets.SocketError error)
    {
        Console.WriteLine($"{Id} - Ошибка в сессии: {error}");
    }

    [Obsolete("Использование этого метода запрещено. Используйте другой способ.", true)]
    public override bool SendAsync(string text) => throw new InvalidOperationException("Метод SendAsync(string) запрещён к использованию.");

    [Obsolete("Использование этого метода запрещено. Используйте другой способ.", true)]
    public override long Send(string text) => throw new InvalidOperationException("Метод Send(string) запрещён к использованию.");
}