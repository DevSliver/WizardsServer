namespace WizardsServer.GameLogic;

class Player
{
    public Client Client { get; }
    public bool IsLoaded { get; private set; }

    private int maxMana;
    public int MaxMana
    {
        get => maxMana;
        set
        {
            if (maxMana != value)
            {
                maxMana = Math.Clamp(value, 0, 10);
                SendManaUpdate();
            }
        }
    }

    private int mana;
    public int Mana
    {
        get => mana;
        set
        {
            int clamped = Math.Clamp(value, 0, 10);
            if (mana != clamped)
            {
                mana = clamped;
                SendManaUpdate();
            }
        }
    }

    public Player(Client client)
    {
        Client = client;
    }

    public void MarkLoaded()
    {
        IsLoaded = true;
    }

    private void SendManaUpdate()
    {
        Client.SendAsync($"mana_update {Mana} {MaxMana}");
    }
}