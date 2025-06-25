using WizardsServer.GameLogic.CardLogic;
using WizardsServer.GameLogic.MatchLogic;

namespace WizardsServer.TestContent.Cards;

public class TestSpell1 : Card
{
    public TestSpell1() : base("Damage5Everyone", 3) { }

    public override Action<MatchContext, EffectContext> GetResolveEffect() => (match, effect) =>
        {
            var opponents = match.Players.Where(pl => pl != effect.Owner);
            foreach (var player in opponents)
            {
                player.Health -= 5;
            }
        };
}
