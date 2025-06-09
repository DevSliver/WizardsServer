namespace WizardsServer;

public class MatchmakingService
{
    private readonly Queue<Client> waitingPlayers = new();
    private readonly HashSet<Client> waitingSet = new();

    public MatchmakingService()
    {
        Server.Instance.CommandProcessor.Subscribe("matchmaking", ProcessCommand);
    }
    private void ProcessCommand(string[] args, Client client)
    {
        switch (args[0])
        {
            case "start_searching":
                StartSearching(args[1..], client);
                break;
            case "cancel_searching":
                CancelSearching(args[1..], client);
                break;
        }
    }

    private void StartSearching(string[] args, Client client)
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

        client.SendAsync("matchmaking waiting");
        TryStartMatch();
    }

    private void CancelSearching(string[] args, Client client)
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

    private void StartMatch(Client player1, Client player2)
    {
        Server.Instance.GameManager.CreateMatch(player1, player2);
    }
}
