using WizardsServer.GameLogic.CardLogic;

namespace WizardsServer.GameLogic.PlayerLogic;

public partial class Player
{
    private List<Card> _deck;
    public void Shuffle(Random? rng = null)
    {
        rng ??= new Random();
        int n = _deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (_deck[n], _deck[k]) = (_deck[k], _deck[n]);
        }
    }
    public Card Peek(int index)
    {
        if (index < 0 || index >= _deck.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Неверный индекс карты в колоде.");

        int internalIndex = _deck.Count - 1 - index;
        return _deck[internalIndex];
    }
    public Card Draw(int index = 0)
    {
        if (index < 0 || index >= _deck.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Неверный индекс карты в колоде.");

        int internalIndex = _deck.Count - 1 - index;
        Card card = _deck[internalIndex];
        _deck.RemoveAt(internalIndex);
        _hand.Add(card);
        return card;
    }
}
