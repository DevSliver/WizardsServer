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

        client1.SetMatchInfo(this, Player1);
        client2.SetMatchInfo(this, Player2);

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
        }
    }
    private Player? GetPlayerByCleint(Client client)
    {
        if (Player1.Client == client) return Player1;
        if (Player2.Client == client) return Player2;
        return null;
    }

    public void OnPlayerDisconnect(Client disconectedClient)
    {
        Player1.Client.SetMatchInfo(null, null);
        Player1.Client.SetMatchInfo(null, null);

        CommandProcessor.ClearAllSubscriptions();

        Server.Instance.GameManager.RemoveMatch(Id);

        int playerNumber = (Player1.Client == disconectedClient) ? 1 : 2;

        Player1.Client.SendAsync($"match_end player_disconected {playerNumber}");
        Player2.Client.SendAsync($"match_end player_disconected {playerNumber}");
    }
}
