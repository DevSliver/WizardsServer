using WizardsServer.GameLogic.MatchLogic;
using WizardsServer.GameLogic.StackLogic;
using WizardsServer.GameLogic.TurnLogic;

namespace WizardsServer.GameLogic.CardLogic;

public abstract class Card
{
    public string Name { get; internal set; } = "";
    public int ManaCost { get; internal set; } = 0;
    public bool GivesPriority { get; internal set; } = true;
    protected Card(string name, int manaCost, bool givesPriority = true)
    {
        Name = name;
        ManaCost = manaCost;
        GivesPriority = givesPriority;
    }

    public abstract Action<MatchContext, EffectContext> GetResolveEffect();
}