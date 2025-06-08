namespace WizardsServer.GameLogic;

class Match
{
    public Guid MatchId { get; }

    public Player Player1 { get; }
    public Player Player2 { get; }

    public Match(Guid matchId, IConnectionContext player1Conn, IConnectionContext player2Conn)
    {
        MatchId = matchId;

        Player1 = new Player(player1Conn);
        Player2 = new Player(player2Conn);

        CommandProcessor.Instance.Subscribe("match_loaded", HandleMatchLoaded);
    }

    private void HandleMatchLoaded(string[] args, IConnectionContext context)
    {
        var player = GetPlayerByContext(context);
        if (player == null || player.IsLoaded)
            return;

        player.MarkLoaded();

        if (Player1.IsLoaded && Player2.IsLoaded)
        {
            CommandProcessor.Instance.Unsubscribe("match_loaded", HandleMatchLoaded);
            Console.WriteLine($"Матч {MatchId}: оба игрока загрузились.");

            Player1.MaxMana = 1;
            Player1.Mana = 1;

            Player2.MaxMana = 1;
            Player2.Mana = 1;
        }
    }

    private Player? GetPlayerByContext(IConnectionContext context)
    {
        if (Player1.Connection == context) return Player1;
        if (Player2.Connection == context) return Player2;
        return null;
    }
}
