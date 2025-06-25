using WizardsServer.GameLogic.MatchLogic;
using WizardsServer.ServerLogic;

namespace WizardsServer.GameLogic.PlayerLogic;

public partial class Player
{
    internal readonly Session Session;
    private readonly Match _match;
    public readonly int Id;
    public bool Loaded { get; private set; } = false;
    public bool Passed { get; internal set; } = false;

    internal Player(Session session, Match match, int id)
    {
        Session = session;
        _match = match;
        Id = id;
        _deck = new();
    }
    public void Load()
    {
        Loaded = true;
        _match.PlayerLoaded(this);
    }
    public void Disconnect()
    {
        _match.Disconnect(this);
    }

    internal void StartTurn()
    {
    }
}