using WizardsServer.GameLogic.CardLogic;
using WizardsServer.GameLogic.MatchLogic;

namespace WizardsServer.GameLogic.StackLogic;

public class StackElement
{
    private readonly Action<MatchContext, EffectContext> _effect;
    public StackElement(Action<MatchContext, EffectContext> effect)
    {
        _effect = effect;
    }
    public void Resolve(MatchContext matchContext, EffectContext effectContext)
        => _effect(matchContext, effectContext);
}