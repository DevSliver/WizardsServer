using System.Reflection;
using WizardsServer.GameLogic.CardLogic;

namespace WizardsServer.CardDB;

public static class CardRegistry
{
    private static Dictionary<string, Card> _cards = new();
    public static IReadOnlyDictionary<string, Card> Cards => _cards;

    public static void LoadCardsFromAssembly()
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        foreach (Type type in asm.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !typeof(Card).IsAssignableFrom(type))
                continue;

            var instance = Activator.CreateInstance(type) as Card;
            if (instance == null)
                continue;

            _cards.Add(instance.Name, instance);
        }
    }
}