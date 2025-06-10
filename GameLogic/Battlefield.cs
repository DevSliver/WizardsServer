using WizardsServer.ServerLogic;
using WizardsServer.Utils;

namespace WizardsServer.GameLogic;

public class Battlefield : ICommandProcessor
{
    private readonly object _lock = new object();

    public const int Width = 8;
    public const int Height = 8;

    private readonly Permanent?[,] _grid;

    public Battlefield()
    {
        _grid = new Permanent[Width, Height];
    }
    public bool PlaceUnit(Permanent unit, Vector2Int position)
    {
        lock (_lock)
        {
            if (!IsInBounds(position) || _grid[position.X, position.Y] != null)
                return false;

            unit.Position = position;
            _grid[position.X, position.Y] = unit;

            unit.Owner.Match.BroadcastAsync($"match battlefield place_unit {position}");
            return true;
        }
    }
    public void RemoveUnit(Vector2Int position)
    {
        lock (_lock)
        {
            if (!IsInBounds(position))
                return;

            Permanent? unit = _grid[position.X, position.Y];
            if (unit == null)
                return;
            _grid[position.X, position.Y] = null;
            unit.Owner.Match.BroadcastAsync($"match battlefield remove_unit {position}");
        }
    }
    public void MoveUnit(Permanent unit, Vector2Int position)
    {
        lock (_lock)
        {
            if (unit == null)
                return;
            if (!unit.CanMoveOn(position, this))
                return;

            Vector2Int oldPos = unit.Position;
            unit.Position = position;

            _grid[oldPos.X, oldPos.Y] = null;
            _grid[position.X, position.Y] = unit;

            unit.Owner.Match.BroadcastAsync($"match battlefield move_unit {oldPos} {position}");
        }
    }
    public Permanent? GetUnitAt(Vector2Int position)
    {
        lock (_lock)
        {
            return IsInBounds(position) ? _grid[position.X, position.Y] : null;
        }
    }
    public bool IsInBounds(Vector2Int pos) =>
        pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;

    public void Process(string[] args, Client client)
    {
        switch (CommandProcessor.ProcessCommand(args, out args))
        {
            case "move_unit":
                MoveUnit(_grid[int.Parse(args[0]), int.Parse(args[1])], new Vector2Int(int.Parse(args[2]), int.Parse(args[3])));
                break;
        }
    }
}