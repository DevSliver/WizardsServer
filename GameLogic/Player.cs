namespace WizardsServer.GameLogic;

public class Player
{
    public Client Client { get; }

    public bool IsLoaded { get; private set; }

    public Player(Client client)
    {
        Client = client;
    }

    public void MarkLoaded()
    {
        IsLoaded = true;
    }
}