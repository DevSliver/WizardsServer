namespace WizardsServer.GameLogic;

class Match
{
    public Guid MatchId { get; }

    public Player Player1 { get; }
    public Player Player2 { get; }

    public Match(Guid matchId, Client player1Conn, Client player2Conn)
    {
        MatchId = matchId;

        Player1 = new Player(player1Conn);
        Player2 = new Player(player2Conn);

        CommandProcessor.Instance.Subscribe("match_loaded", HandleMatchLoaded);
    }

    private void HandleMatchLoaded(string[] args, Client client)
    {
        var player = GetPlayerByContext(client);
        if (player == null)
        {
            Console.WriteLine("Получена команда match_loaded от неизвестного игрока.");
            return;
        }
        if (player.IsLoaded)
        {
            Console.WriteLine($"Игрок {player} уже загрузился ранее.");
            return;
        }

        Console.WriteLine($"Игрок {player} загрузился.");
        player.MarkLoaded();

        if (Player1.IsLoaded && Player2.IsLoaded)
        {
            Console.WriteLine($"Матч {MatchId}: оба игрока загрузились.");
            CommandProcessor.Instance.Unsubscribe("match_loaded", HandleMatchLoaded);

            Player1.MaxMana = 1;
            Player1.Mana = 1;
            Console.WriteLine("Установлена мана для Player1: MaxMana=1, Mana=1");

            Player2.MaxMana = 1;
            Player2.Mana = 1;
            Console.WriteLine("Установлена мана для Player2: MaxMana=1, Mana=1");
        }
    }
    private Player? GetPlayerByContext(Client client)
    {
        if (Player1.Client == client) return Player1;
        if (Player2.Client == client) return Player2;
        return null;
    }
}
