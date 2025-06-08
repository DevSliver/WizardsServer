using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace WizardsServer.GameLogic
{
    class GameManager
    {
        public static GameManager Instance { get; } = new GameManager();

        private readonly ConcurrentDictionary<Guid, Match> activeMatches = new();

        private GameManager() { }

        public void CreateMatch(Guid matchId, Client player1, Client player2)
        {
            var match = new Match(matchId, player1, player2);
            if (!activeMatches.TryAdd(matchId, match))
            {
                Console.WriteLine($"Матч с ID {matchId} уже существует!");
                return;
            }
            Console.WriteLine($"Матч {matchId} создан между игроками.");
        }
        public Match? GetMatch(Guid matchId)
        {
            activeMatches.TryGetValue(matchId, out var match);
            return match;
        }

        public bool RemoveMatch(Guid matchId)
        {
            return activeMatches.TryRemove(matchId, out _);
        }
    }
}
