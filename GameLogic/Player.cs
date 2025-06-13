using WizardsServer.ServerLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer.GameLogic;

public class Player
{
    public Session Session { get; }
    public Match Match { get; }
    public int Number { get; }
    public bool IsLoaded { get; private set; } = false;

    public Player(Session session, Match match, int number)
    {
        Session = session;
        Match = match;
        Number = number;
    }
    public void Loaded()
    {
        IsLoaded = true;
        Match.PlayerLoaded(this);
    }
    public void Disconnect(Command command)
    {
        Match.Disconnect(this);
    }
}