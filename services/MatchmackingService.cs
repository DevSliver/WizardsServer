using WizardsServer.ServerLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer.services;

public class MatchmackingService
{
    private readonly HashSet<Session> waitingPlayers = new();

    public Args StartSearching(Session session)
    {
        if (waitingPlayers.Any(s => s.UserId == session.UserId))
            return new Args().Add("Message", "already waiting");

        waitingPlayers.Add(session);
        TryStartMatch();
        return new Args().Add("Message", "success");
    }

    public Args CancelSearching(Session session)
    {
        if (waitingPlayers.Remove(session))
            return new Args().Add("Message", "success");
        else
            return new Args().Add("Message", "not waiting");
    }
    private void TryStartMatch()
    {
        var player1 = waitingPlayers.First();
        waitingPlayers.Remove(player1);
        var player2 = waitingPlayers.First();
        waitingPlayers.Remove(player2);
        StartMatch(player1, player2);
    }
    private void StartMatch(Session session1, Session session2)
    {
        Server.Instance.GameManager.CreateMatch(session1, session2);
        Command start = new Command("Matchmacking.StartMatch");
        start.Args.Add("Number", 1);
        session1.Send(start);
        start.Args.Add("Number", 2);
        session2.Send(start);
    }
}
