using WizardsServer.GameLogic;

namespace WizardsServer
{
    class MatchmakingService
    {
        private readonly Queue<IConnectionContext> waitingPlayers = new();
        private readonly HashSet<IConnectionContext> waitingSet = new();

        public MatchmakingService()
        {
            CommandProcessor.Instance.Subscribe("find_match", HandleFindMatch);
            CommandProcessor.Instance.Subscribe("cancel_match", HandleCancelMatch);
        }

        private void HandleFindMatch(string[] args, IConnectionContext context)
        {
            if (context is not ConnectionContext ctx || ctx.Client.UserId is not int)
            {
                context.SendAsync("matchmaking fail not_authenticated");
                return;
            }

            lock (waitingPlayers)
            {
                if (waitingSet.Contains(context))
                {
                    context.SendAsync("matchmaking fail already_waiting");
                    return;
                }

                waitingPlayers.Enqueue(context);
                waitingSet.Add(context);
            }

            context.SendAsync("matchmaking waiting");
            TryStartMatch();
        }

        private void HandleCancelMatch(string[] args, IConnectionContext context)
        {
            lock (waitingPlayers)
            {
                if (waitingSet.Remove(context))
                {
                    var newQueue = new Queue<IConnectionContext>(waitingPlayers.Where(p => p != context));
                    waitingPlayers.Clear();
                    foreach (var player in newQueue)
                        waitingPlayers.Enqueue(player);

                    context.SendAsync("matchmaking cancelled");
                }
                else
                {
                    context.SendAsync("matchmaking fail not_waiting");
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

        private void StartMatch(IConnectionContext player1, IConnectionContext player2)
        {
            var matchId = Guid.NewGuid();

            GameManager.Instance.CreateMatch(matchId, player1, player2);

            player1.SendAsync($"match_start {matchId} 1");
            player2.SendAsync($"match_start {matchId} 2");
        }
    }
}
