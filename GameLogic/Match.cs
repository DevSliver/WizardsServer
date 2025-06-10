using System.Numerics;
using WizardsServer.ServerLogic;

namespace WizardsServer.GameLogic;

public class Match
{
    public Guid Id { get; }
    public Player Player1 { get; }
    public Player Player2 { get; }
    Battlefield _battlefield;
    public Battlefield Battlefield => _battlefield;

    public Match(Guid id, Client client1, Client client2)
    {
        Id = id;
        Player1 = new Player(client1, this, 1);
        Player2 = new Player(client2, this, 2);
        Player1.Client.Player = Player1;
        Player2.Client.Player = Player2;
    }
    public void NotifyPlayerLoaded(Player player)
    {
        if (Player1.IsLoaded && Player2.IsLoaded)
        {
            Console.WriteLine($"Оба игрока загрузились (Матч ID: {Id})");
            BroadcastAsync("match start");
            StartMatch();
        }
    }
    private void StartMatch()
    {
        _battlefield = new Battlefield();
        _battlefield.PlaceUnit(new Permanent(Player1), new(0, 0));
        _battlefield.PlaceUnit(new Permanent(Player2), new(7, 7));
    }
    public void BroadcastAsync(string message)
    {
        Player1.Client.SendAsync(message);
        Player2.Client.SendAsync(message);
    }
    public void Disconnect(Player player)
    {
        BroadcastAsync($"match disconnect {(player == Player1 ? 1 : 2)}");
        Server.Instance.GameManager.RemoveMatch(Id);
    }
}