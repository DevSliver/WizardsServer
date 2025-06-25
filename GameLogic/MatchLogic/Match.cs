using WizardsServer.GameLogic.PlayerLogic;
using WizardsServer.GameLogic.StackLogic;
using WizardsServer.GameLogic.TurnLogic;
using WizardsServer.ServerLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer.GameLogic.MatchLogic;

internal class Match : IDisposable
{
    public readonly Guid Id;
    public readonly MatchContext Context;
    readonly TurnManager _turnManager;
    readonly Player[] _players;
    readonly Stack _stack;

    internal Match(Guid id, params Session[] sessions)
    {
        Id = id;
        _players = new Player[sessions.Length];
        _stack = new Stack();
        _turnManager = new TurnManager();

        for (int i = 0; i < sessions.Length; i++)
        {
            var newPlayer = new Player(sessions[i], this, i);
            _players[i] = newPlayer;
            sessions[i].TrySetPlayer(newPlayer);
        }
        Context = new(_players, _stack, _turnManager);
    }
    internal void PlayerLoaded(Player player)
    {
        if (_players.Any(pl => pl.Loaded == false))
            return;
        Console.WriteLine($"Все игроки загрузились в матч {Id}");
        Broadcast(new Command("Match.Start"));
        StartMatch();
    }
    private void StartMatch()
    {
        _turnManager.Start(Context);
    }
    internal void Disconnect(Player player)
    {
        Command com = new("Match.End");
        com.Args.Add("Reason", "disconnect");
        com.Args.Add("Loser", player.Id);
        Broadcast(com);
        Server.Instance.GameManager.RemoveMatch(Id);
    }
    public void Broadcast(Command command)
    {
        foreach (var player in _players)
        {
            player.Session.Send(command);
        }
    }

    public void Dispose()
    {
        foreach (var player in _players)
        {
            player.Session.TrySetPlayer(null);
        }
    }
}