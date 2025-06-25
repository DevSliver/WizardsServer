using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardsServer.GameLogic.PlayerLogic;

public partial class Player
{
    private int _health = 20;
    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
        }
    }
}