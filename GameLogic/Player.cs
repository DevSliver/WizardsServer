namespace WizardsServer.GameLogic;

class Player
{
    public IConnectionContext Connection { get; }
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

    public Player(IConnectionContext connection)
    {
        Connection = connection;
    }

    public void MarkLoaded()
    {
        IsLoaded = true;
    }

    private void SendManaUpdate()
    {
        Connection.SendAsync($"mana_update {Mana} {MaxMana}");
    }
}