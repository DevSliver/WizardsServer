using WizardsServer.ServerLogic;

namespace WizardsServer.GameLogic;

public class Player : ICommandProcessor
{
    public Client Client { get; }
    public Match Match { get; }
    public int Id { get; }
    public bool IsLoaded { get; private set; }

    public Player(Client client, Match match, int id)
    {
        Client = client;
        Match = match;
        Id = id;
        IsLoaded = false;
    }
    public void Process(string[] args, Client client)
    {
        Console.WriteLine($"{client.Session.Id} - Process in Player");
        switch (args[0])
        {
            case "loaded":
                IsLoaded = true;
                Match.NotifyPlayerLoaded(this);
                break;
            case "battlefield":
                Match.Battlefield.Process(args, client);
                break;
            case "disconnect":
                Match.Disconnect(this);
                break;
        }
    }
}