using WizardsServer.ServerLogic;

namespace WizardsServer;

public class MatchmakingService : ICommandProcessor
{
    private readonly Queue<Client> waitingPlayers = new();
    private readonly HashSet<Client> waitingSet = new();

    public void Process(string[] args, Client client)
    {
        switch (CommandProcessor.ProcessCommand(args, out args))
        {
            case "start_searching":
                StartSearching(client);
                break;
            case "cancel_searching":
                CancelSearching(client);
                break;
        }
    }

    private void StartSearching(Client client)
    {
        if (!client.IsAuthenticated)
        {
            client.SendAsync("matchmaking error not_authenticated");
            return;
        }

        lock (waitingPlayers)
        {
            if (waitingSet.Contains(client))
            {
                client.SendAsync("matchmaking error already_waiting");
                return;
            }

            waitingPlayers.Enqueue(client);
            waitingSet.Add(client);
        }

        client.SendAsync("matchmaking status waiting");
        TryStartMatch();
    }

    public void CancelSearching(Client client)
    {
        lock (waitingPlayers)
        {
            if (waitingSet.Remove(client))
            {
                var newQueue = new Queue<Client>(waitingPlayers.Where(p => p != client));
                waitingPlayers.Clear();
                foreach (var player in newQueue)
                    waitingPlayers.Enqueue(player);

                client.SendAsync("matchmaking status cancelled");
            }
            else
            {
                client.SendAsync("matchmaking error not_waiting");
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
    private void StartMatch(Client client1, Client client2)
    {
        Server.Instance.GameManager.CreateMatch(client1, client2);
        client1.SendAsync($"matchmaking match_found 1");
        client2.SendAsync($"matchmaking match_found 2");
    }
}
