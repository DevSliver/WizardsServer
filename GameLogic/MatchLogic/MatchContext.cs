using WizardsServer.GameLogic.PlayerLogic;
using WizardsServer.GameLogic.StackLogic;
using WizardsServer.GameLogic.TurnLogic;

namespace WizardsServer.GameLogic.MatchLogic;

public class MatchContext
{
    public readonly Player[] Players;
    public readonly Stack Stack;
    public readonly TurnManager TurnManager;

    public MatchContext(Player[] players, Stack stack, TurnManager turnManager)
    {
        Players = players;
        Stack = stack;
        TurnManager = turnManager;
    }
}
