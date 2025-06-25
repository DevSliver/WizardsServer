using WizardsServer.GameLogic.MatchLogic;
using WizardsServer.GameLogic.PlayerLogic;

namespace WizardsServer.GameLogic.TurnLogic;

public class TurnManager
{
    private MatchContext _context;
    private Player _currentPlayerTurn;
    private Player _currentPlayerPriority;
    private int _turnNumber = 0;

    internal TurnManager() { }
    internal void Start(MatchContext context)
    {
        _context = context;

        NextTurn();
    }
    private void NextTurn()
    {
        _turnNumber++;
        _currentPlayerTurn = _context.Players[_turnNumber % _context.Players.Length];
        _currentPlayerPriority = _currentPlayerTurn;
    }
    internal void Pass()
    {
        _currentPlayerPriority.Passed = true;
        bool allPassed = true;
        foreach (Player player in _context.Players)
        {
            if (player.Passed == false)
            {
                allPassed = false;
                break;
            }
        }
        if (allPassed)
        {
            if (_context.Stack.Empty)
            {
                NextTurn();
            }
            else
            {
                _currentPlayerPriority = _currentPlayerTurn;
                _context.Stack.Resolve(_context);
            }
        }
        else
        {
            NextPriority();
        }
    }
    internal void NextPriority()
    {
        _currentPlayerPriority = _context.Players[(_currentPlayerPriority.Id + 1) % _context.Players.Length];
    }
}
