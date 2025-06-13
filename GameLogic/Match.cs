using WizardsServer.ServerLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer.GameLogic;

public class Match : IDisposable
{
    public Guid Id { get; }
    public Player[] Players { get; }
    Battlefield _battlefield;
    public Battlefield Battlefield => _battlefield;

    public Match(Guid id, params Session[] sessions)
    {
        Id = id;
        _battlefield = new Battlefield(this);
        Players = new Player[sessions.Length];
        for (int i = 0; i < sessions.Length; i++)
        {
            var newPlayer = new Player(sessions[i], this, i);
            Players[i] = newPlayer;
            sessions[i].TrySetPlayer(newPlayer);
        }
    }
    public void PlayerLoaded(Player player)
    {
        if (Players.Any(pl => pl.IsLoaded == false))
            return;
        Console.WriteLine($"Все игроки загрузились в матч {Id}");
        Broadcast(new Command("Match.Start"));
        StartMatch();
    }
    private void StartMatch()
    {
        _battlefield.PlacePermanent(new Permanent(Players[0]), new(0, 0));
        _battlefield.PlacePermanent(new Permanent(Players[1]), new(7, 7));
    }
    public void Disconnect(Player player)
    {
        Command com = new("Match.End");
        com.Args.Add("Reason", "disconnect");
        com.Args.Add("Loser", player.Number);
        Broadcast(com);
        Server.Instance.GameManager.RemoveMatch(Id);
    }
    public void Broadcast(Command command)
    {
        foreach (var player in Players)
        {
            player.Session.Send(command);
        }
    }

    public void Dispose()
    {
        foreach (var player in Players)
        {
            player.Session.TrySetPlayer(null);
        }
    }
}