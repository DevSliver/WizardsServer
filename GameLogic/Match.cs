namespace WizardsServer.GameLogic;

public class Match
{
    public Guid Id { get; }

    public Player Player1 { get; }
    public Player Player2 { get; }
    public CommandProcessor CommandProcessor { get; }

    public Match(Guid id, Client client1, Client client2)
    {
        Id = id;
        Player1 = new Player(client1);
        Player2 = new Player(client2);

        client1.Match = this;
        client2.Match = this;

        client1.Player = Player1;
        client2.Player = Player2;

        CommandProcessor = new CommandProcessor();

        CommandProcessor.Subscribe("match_loaded", HandleMatchLoaded);
    }

    private void HandleMatchLoaded(string[] args, Client client)
    {
        var player = GetPlayerByCleint(client);
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
            Console.WriteLine($"Матч {Id}: оба игрока загрузились.");
            CommandProcessor.Unsubscribe("match_loaded", HandleMatchLoaded);

            Player1.MaxMana = 1;
            Player1.Mana = 1;
            Console.WriteLine("Установлена мана для Player1: MaxMana=1, Mana=1");

            Player2.MaxMana = 1;
            Player2.Mana = 1;
            Console.WriteLine("Установлена мана для Player2: MaxMana=1, Mana=1");
        }
    }
    private Player? GetPlayerByCleint(Client client)
    {
        if (Player1.Client == client) return Player1;
        if (Player2.Client == client) return Player2;
        return null;
    }
}
