namespace WizardsServer.services;

using System;
using WizardsServer.GameLogic.MatchLogic;
using WizardsServer.ServerLogic;

internal class GameManager
{
    private readonly object _lock = new();
    private readonly Dictionary<Guid, Match> _matches = new();

    internal void CreateMatch(Session session1, Session session2)
    {
        lock (_lock)
        {
            var id = Guid.NewGuid();
            var match = new Match(id, session1, session2);
            _matches[id] = match;
            Console.WriteLine($"Матч {id} создан");
        }
    }
    internal bool RemoveMatch(Guid id)
    {
        lock (_lock)
        {
            if (!_matches.Remove(id, out var match))
            {
                Console.WriteLine($"Матч {id} не найден для удаления");
                return false;
            }

            match.Dispose();
            Console.WriteLine($"Матч {id} удалён");
            return true;
        }
    }
}
