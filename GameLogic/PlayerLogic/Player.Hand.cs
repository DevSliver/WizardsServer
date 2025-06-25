using WizardsServer.GameLogic.CardLogic;
using WizardsServer.GameLogic.MatchLogic;
using WizardsServer.GameLogic.StackLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer.GameLogic.PlayerLogic;

public partial class Player
{
    private readonly List<Card> _hand = new();

    public void AddToHand(Card card)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        _hand.Add(card);
    }
    public bool RemoveFromHand(Card card)
    {
        return _hand.Remove(card);
    }
    public Card GetFromHand(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return _hand.FirstOrDefault(c => c.Name == name)
            ?? throw new KeyNotFoundException($"Card with name '{name}' not found in hand.");
    }
    public Card TakeFromHand(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        var card = _hand.FirstOrDefault(c => c.Name == name)
            ?? throw new KeyNotFoundException($"Card with name '{name}' not found in hand.");
        _hand.Remove(card);
        return card;
    }
    public void PlayCard(string name, Args args)
    {
        var matchContext = _match.Context;
        var effectContext = new EffectContext(this, args);

        var card = TakeFromHand(name);
        var stackElement = new StackElement(card.GetResolveEffect());
        matchContext.Stack.Add(stackElement);
        if (card.GivesPriority)
        {
            matchContext.TurnManager.NextPriority();
        }
    }
}
