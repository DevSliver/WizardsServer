using MessagePack;
using UnityEngine;

namespace WizardsServer.GameLogic;

public class Permanent
{
    public Player Owner { get; }
    public Vector2Int Position = new();
    public readonly Guid Id = Guid.NewGuid();

    public Permanent(Player owner)
    {
        Owner = owner;
    }

    public virtual List<Vector2Int> GetRelativeMovePattern() => new List<Vector2Int>();
    public List<Vector2Int> GetGlobalMovePattern()
    {
        var relative = GetRelativeMovePattern();
        var global = new List<Vector2Int>(relative.Count);

        for (int i = 0; i < relative.Count; i++)
        {
            global.Add(relative[i].Add(Position));
        }

        return global;
    }
    public bool CanMoveOn(Vector2Int position, Battlefield battlefield)
    {
        if (!battlefield.IsWithinBounds(position)) return false;
        if (battlefield.GetPermanentAt(position) != null) return false;

        return GetGlobalMovePattern().Contains(position);
    }
}