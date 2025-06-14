using UnityEngine;
using WizardsServer.ServerLogic;
using WizardsServer.ServerLogic.CommandSystem;

namespace WizardsServer.GameLogic;

public class Battlefield
{
    private readonly Match _match;

    public const int Width = 8;
    public const int Height = 8;
    private readonly Permanent?[,] _grid;

    public Battlefield(Match match)
    {
        _grid = new Permanent[Width, Height];
        _match = match;
    }
    public bool PlacePermanent(Permanent perm, Vector2Int position)
    {
        if (!IsWithinBounds(position) || _grid[position.x, position.y] != null)
            return false;

        perm.Position = position;
        _grid[position.x, position.y] = perm;

        Command com = new("Match.PlacePermanent");
        com.Args.Add("x", position.x).Add("Y", position.y).Add("Owner", perm.Owner.Number);
        _match.Broadcast(com);
        return true;
    }
    public void RemovePermanent(Vector2Int position)
    {
        if (!IsWithinBounds(position))
            return;

        Permanent? perm = _grid[position.x, position.y];
        if (perm == null)
            return;
        _grid[position.x, position.y] = null;
        //perm.Owner.Match.BroadcastAsync($"match battlefield remove_permanent {perm.Id}");
    }
    public void MovePermanent(Permanent perm, Vector2Int position)
    {
        if (perm == null)
            return;
        if (!perm.CanMoveOn(position, this))
            return;

        Vector2Int oldPos = perm.Position;
        perm.Position = position;

        _grid[oldPos.x, oldPos.y] = null;
        _grid[position.x, position.y] = perm;

        //perm.Owner.Match.BroadcastAsync($"match battlefield move_permanent {perm.Id} {position}");
    }
    public Permanent? GetPermanentAt(Vector2Int position)
    {
        return IsWithinBounds(position) ? _grid[position.x, position.y] : null;
    }
    public bool IsWithinBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;
}