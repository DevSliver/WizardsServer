using WizardsServer.ServerLogic;

namespace WizardsServer.GameLogic;

public class Match
{
    public Guid Id { get; }
    public Player[] Players { get; }
    Battlefield _battlefield;
    public Battlefield Battlefield => _battlefield;

    public Match(Guid id, params Client[] clients)
    {
        Id = id;
        _battlefield = new Battlefield();
        Players = new Player[clients.Length];
        for (int i = 0; i < clients.Length; i++)
        {
            var newPlayer = new Player(clients[i], this, i);
            Players[i] = newPlayer;
            clients[i].Player = newPlayer;
        }
    }
    public void NotifyPlayerLoaded(Player player)
    {
        if (Players.Any(pl => pl.IsLoaded == false))
            return;
        Console.WriteLine($"Оба игрока загрузились (Матч ID: {Id})");
        BroadcastAsync("match start");
        StartMatch();
    }
    private void StartMatch()
    {
        _battlefield.PlacePermanent(new Permanent(Players[0]), new(0, 0));
        _battlefield.PlacePermanent(new Permanent(Players[1]), new(7, 7));
    }
    public void Disconnect(Player player)
    {
        BroadcastAsync($"match disconnect {player.Id}");
        Server.Instance.GameManager.RemoveMatch(Id);
    }
    public void BroadcastAsync(string message)
    {
        foreach (var player in Players)
        {
            player.Client.SendAsync(message);
        }
    }
}