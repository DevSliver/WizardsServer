using WizardsServer.ServerLogic;
using WizardsServer.ServerLogic.CommandSystem;
using WizardsServer.Utils;

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
        if (!IsWithinBounds(position) || _grid[position.X, position.Y] != null)
            return false;

        perm.Position = position;
        _grid[position.X, position.Y] = perm;

        Command com = new("Match.PlacePermanent");
        com.Args.Add("x", position.X).Add("Y", position.Y).Add("Owner", perm.Owner.Number);
        _match.Broadcast(com);
        return true;
    }
    public void RemovePermanent(Vector2Int position)
    {
        if (!IsWithinBounds(position))
            return;

        Permanent? perm = _grid[position.X, position.Y];
        if (perm == null)
            return;
        _grid[position.X, position.Y] = null;
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

        _grid[oldPos.X, oldPos.Y] = null;
        _grid[position.X, position.Y] = perm;

        //perm.Owner.Match.BroadcastAsync($"match battlefield move_permanent {perm.Id} {position}");
    }
    public Permanent? GetPermanentAt(Vector2Int position)
    {
        return IsWithinBounds(position) ? _grid[position.X, position.Y] : null;
    }
    public bool IsWithinBounds(Vector2Int pos) =>
        pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
}