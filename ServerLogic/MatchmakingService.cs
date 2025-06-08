using WizardsServer.GameLogic;

namespace WizardsServer
{
    class MatchmakingService
    {
        private readonly Queue<Client> waitingPlayers = new();
        private readonly HashSet<Client> waitingSet = new();

        public MatchmakingService()
        {
            CommandProcessor.Instance.Subscribe("find_match", HandleFindMatch);
            CommandProcessor.Instance.Subscribe("cancel_match", HandleCancelMatch);
        }

        private void HandleFindMatch(string[] args, Client client)
        {
            if (client.UserId is not int)
            {
                client.SendAsync("matchmaking fail not_authenticated");
                return;
            }

            lock (waitingPlayers)
            {
                if (waitingSet.Contains(client))
                {
                    client.SendAsync("matchmaking fail already_waiting");
                    return;
                }

                waitingPlayers.Enqueue(client);
                waitingSet.Add(client);
            }

            client.SendAsync("matchmaking waiting");
            TryStartMatch();
        }

        private void HandleCancelMatch(string[] args, Client client)
        {
            lock (waitingPlayers)
            {
                if (waitingSet.Remove(client))
                {
                    var newQueue = new Queue<Client>(waitingPlayers.Where(p => p != client));
                    waitingPlayers.Clear();
                    foreach (var player in newQueue)
                        waitingPlayers.Enqueue(player);

                    client.SendAsync("matchmaking cancelled");
                }
                else
                {
                    client.SendAsync("matchmaking fail not_waiting");
                }
            }
        }

        private void TryStartMatch()
        {
            lock (waitingPlayers)
            {
                while (waitingPlayers.Count >= 2)
                {
                    var player1 = waitingPlayers.Dequeue();
                    waitingSet.Remove(player1);
                    var player2 = waitingPlayers.Dequeue();
                    waitingSet.Remove(player2);

                    StartMatch(player1, player2);
                }
            }
        }

        private void StartMatch(Client player1, Client player2)
        {
            var matchId = Guid.NewGuid();

            GameManager.Instance.CreateMatch(matchId, player1, player2);

            player1.SendAsync($"match_start {matchId} 1");
            player2.SendAsync($"match_start {matchId} 2");
        }
    }
}
