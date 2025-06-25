using WizardsServer.GameLogic.PlayerLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer.GameLogic.CardLogic;

public class EffectContext
{
    public readonly Player Owner;
    public readonly Args Args;

    public EffectContext(Player owner, Args args)
    {
        Owner = owner;
        Args = args;
    }
}
