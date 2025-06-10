using WizardsServer.Utils;

namespace WizardsServer.GameLogic;

public abstract class Unit
{
    public Player Owner { get; }
    public Vector2Int Position { get; private set; }
    public int Health { get; private set; }
    public int AttackDamage { get; }
    public int MovementPoints { get; private set; }
    public int MaxMovementPoints { get; }

    protected Unit(Player owner, Vector2Int startPos, int health, int attackDamage, int maxMovementPoints)
    {
        Owner = owner;
        Position = startPos;
        Health = health;
        AttackDamage = attackDamage;
        MaxMovementPoints = maxMovementPoints;
        MovementPoints = maxMovementPoints;
        
    }

    public void ResetMovementPoints()
    {
        MovementPoints = MaxMovementPoints;
    }

    public bool CanMoveTo(Vector2Int target)
    {
        return MovementPoints > 0 && IsValidMove(target);
    }

    public void MoveTo(Vector2Int target)
    {
        if (!CanMoveTo(target))
            return;

        Position = target;
        MovementPoints -= 1;
    }

    public void ReceiveDamage(int damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;
    }
    public abstract bool IsValidMove(Vector2Int target);
    public abstract IEnumerable<Vector2Int> GetValidMoves();
}