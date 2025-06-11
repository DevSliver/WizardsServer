using WizardsServer.ServerLogic;
using WizardsServer.Utils;

namespace WizardsServer.GameLogic;

public class Battlefield : ICommandProcessor
{
    private readonly object _lock = new object();

    public const int Width = 8;
    public const int Height = 8;

    private readonly Permanent?[,] _grid;
    private int _nextPermanentId = 0;

    public Battlefield()
    {
        _grid = new Permanent[Width, Height];
    }
    public bool PlacePermanent(Permanent perm, Vector2Int position)
    {
        lock (_lock)
        {
            if (!IsWithinBounds(position) || _grid[position.X, position.Y] != null)
                return false;

            perm.Position = position;
            perm.Id = _nextPermanentId++;
            _grid[position.X, position.Y] = perm;

            perm.Owner.Match.BroadcastAsync($"match battlefield place_permanent {perm.Id} {position} {perm.Owner.Id}");
            return true;
        }
    }
    public void RemovePermanent(Vector2Int position)
    {
        lock (_lock)
        {
            if (!IsWithinBounds(position))
                return;

            Permanent? perm = _grid[position.X, position.Y];
            if (perm == null)
                return;
            _grid[position.X, position.Y] = null;
            perm.Owner.Match.BroadcastAsync($"match battlefield remove_permanent {perm.Id}");
        }
    }
    public void MovePermanent(Permanent perm, Vector2Int position)
    {
        lock (_lock)
        {
            if (perm == null)
                return;
            if (!perm.CanMoveOn(position, this))
                return;

            Vector2Int oldPos = perm.Position;
            perm.Position = position;

            _grid[oldPos.X, oldPos.Y] = null;
            _grid[position.X, position.Y] = perm;

            perm.Owner.Match.BroadcastAsync($"match battlefield move_permanent {perm.Id} {position}");
        }
    }
    public Permanent? GetPermanentAt(Vector2Int position)
    {
        lock (_lock)
        {
            return IsWithinBounds(position) ? _grid[position.X, position.Y] : null;
        }
    }
    public bool IsWithinBounds(Vector2Int pos) =>
        pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;

    public void Process(string[] args, Client client)
    {
        switch (CommandProcessor.ProcessCommand(args, out args))
        {
            case "move_permanent":
                MovePermanent(_grid[int.Parse(args[0]), int.Parse(args[1])], new Vector2Int(int.Parse(args[2]), int.Parse(args[3])));
                break;
        }
    }
}