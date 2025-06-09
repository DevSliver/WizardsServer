namespace WizardsServer.GameLogic;

using System;
using System.Collections.Concurrent;

public class GameManager
{
    private readonly ConcurrentDictionary<Guid, Match> _matches = new();

    public void CreateMatch(Client client1, Client client2)
    {
        var id = Guid.NewGuid();
        var match = new Match(id, client1, client2);
        _matches.TryAdd(id, match);
        Console.WriteLine($"Матч {id} создан");

        client1.SendAsync($"matchmaking creating_match {id} 1");
        client2.SendAsync($"matchmaking creating_match {id} 2");
    }
    public bool RemoveMatch(Guid id)
    {
        if (_matches.TryRemove(id, out var match))
        {
            Console.WriteLine($"Матч {id} удалён");
            return true;
        }

        Console.WriteLine($"Матч {id} не найден для удаления");
        return false;
    }
    public Match? GetMatch(Guid id)
    {
        _matches.TryGetValue(id, out var match);
        return match;
    }
    public Match[] GetAllMatches()
    {
        return _matches.Values.ToArray();
    }
}
