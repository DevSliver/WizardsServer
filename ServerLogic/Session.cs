using NetCoreServer;
using System.Xml.Linq;
using WizardsServer.GameLogic.PlayerLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer.ServerLogic;

internal class Session : TcpSession
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

    public int DeckId { get; private set; }
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
            (s, c) => Player?.Load();
    }
    private void AuthCommands()
    {
        _processor["Matchmaking.StartSearching"] =
            (s, c) => Send(Command.Response(c, Server.MatchmakingService.StartSearching(s)));
        _processor["Matchmaking.CancelSearching"] =
            (s, c) => Send(Command.Response(c, Server.MatchmakingService.CancelSearching(s)));
        _processor["Deck.Select"] = (s, c) =>
        {
            if (c.Args.TryGet("DeckId", out int deckId))
            {
                if (deckId == -1)
                {
                    DeckId = -1;
                    Send(Command.MsgResponse(c, "success"));
                    return;
                }
                string sql = "SELECT 1 FROM decks WHERE id = @deckId AND user_id = @userId LIMIT 1";

                using var command = Database.CreateCommand(sql);
                command.Parameters.AddWithValue("deckId", deckId);
                command.Parameters.AddWithValue("userId", UserId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    DeckId = deckId;
                    Send(Command.MsgResponse(c, "success"));
                }
                else
                {
                    Send(Command.MsgResponse(c, "deck not found"));
                }
            }
            else
                Send(Command.MsgResponse(c, "usage"));
        };
        _processor["Deck.Create"] = (s, c) =>
        {
            if (c.Args.TryGet("Name", out string name))
            {
                if (string.IsNullOrEmpty(name))
                {
                    Send(Command.MsgResponse(c, "name is nul or empty"));
                    return;
                }
                string sql = "INSERT INTO decks (name) VALUES (@name) RETURNING id;";

                using var command = Database.CreateCommand(sql);
                command.Parameters.AddWithValue("name", name);

                int newId = (int)command.ExecuteScalar()!;
                var cmd = Command.MsgResponse(c, "success");
                cmd.Args.Add("Id", newId);
                Send(cmd);
            }
            else
                Send(Command.MsgResponse(c, "usage"));
        };
        _processor["Deck.EditCards"] = (s, c) =>
        {
            if (c.Args.TryGet("DeckId", out int deckId) &&
                c.Args.TryGet("CardNames", out List<string> cards) &&
                c.Args.TryGet("CardQuantities", out List<int> quantities))
            {
                if (cards!.Count != quantities!.Count)
                {
                    Send(Command.MsgResponse(c, "names.Count != quantities.Count"));
                    return;
                }
                if (cards!.Count < 20)
                {
                    Send(Command.MsgResponse(c, "not enough cards (minimum 20)"));
                    return;
                }

                {
                    string sql = "DELETE FROM deck_cards WHERE deck_id = @deckId;";
                    using var command = Database.CreateCommand(sql);
                    command.Parameters.AddWithValue("deckId", deckId);
                    command.ExecuteNonQuery();
                }

                for (int i = 0; i < cards.Count; i++)
                {
                    string sql = "INSERT INTO deck_cards (deck_id, card_name, quantity) VALUES (@deckId, @name, @quantity);";
                    using var command = Database.CreateCommand(sql);
                    command.Parameters.AddWithValue("deckId", deckId);
                    command.Parameters.AddWithValue("name", cards[i]);
                    command.Parameters.AddWithValue("quantity", quantities[i]);
                    command.ExecuteNonQuery();
                }
                Send(Command.MsgResponse(c, "success"));
            }
            else
                Send(Command.MsgResponse(c, "usage"));
        };
        _processor["Deck.Get"] = (s, c) =>
        {
            if (c.Args.TryGet("DeckId", out int deckId))
            {
                string sql = "SELECT card_name, quantity FROM deck_cards WHERE deck_id = @deckId;";

                using var command = Database.CreateCommand(sql);
                command.Parameters.AddWithValue("deckId", deckId);

                using var reader = command.ExecuteReader();

                var names = new List<string>();
                var quantities = new List<int>();

                while (reader.Read())
                {
                    string cardName = reader.GetString(0);
                    int quantity = reader.GetInt32(1);

                    names.Add(cardName);
                    quantities.Add(quantity);
                }

                var cmd = Command.MsgResponse(c, "success");
                cmd.Args.Add("CardNames", names).Add("CardQuantities", quantities);
                Send(cmd);
            }
            else
                Send(Command.MsgResponse(c, "usage"));
        };
    }

    public void Auth(int userId)
    {
        if (!IsAuthed)
        {
            UserId = userId;
            AuthCommands();
            Server.AuthUser(this, userId);
        }
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