namespace WizardsServer.GameLogic;

public class Player
{
    public Client Client { get; }
    public int Id { get; }

    public bool IsLoaded { get; private set; }

    public Player(Client client, int id)
    {
        Client = client;
        Id = id;
    }

    public void MarkLoaded()
    {
        IsLoaded = true;
    }
}