using WizardsServer.Utils;

namespace WizardsServer.GameLogic;

public class Battlefield
{
    private const int Width = 8;
    private const int Height = 8;

    private Unit?[,] grid = new Unit[Width, Height];

    public bool IsInside(Vector2Int pos)
    {
        return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
    }

    public bool IsOccupied(Vector2Int pos)
    {
        if (!IsInside(pos)) return false;
        return grid[pos.X, pos.Y] != null;
    }

    public Unit GetUnitAt(Vector2Int pos)
    {
        if (!IsInside(pos)) return null;
        return grid[pos.X, pos.Y];
    }

    public bool AddUnit(Unit unit, Vector2Int pos)
    {
        if (!IsInside(pos) || IsOccupied(pos))
            return false;

        grid[pos.X, pos.Y] = unit;
        return true;
    }

    public bool RemoveUnit(Vector2Int pos)
    {
        if (!IsInside(pos) || !IsOccupied(pos))
            return false;

        grid[pos.X, pos.Y] = null;
        return true;
    }

    // Перемещение юнита с одной клетки на другую (без логики проверки в Unit)
    public bool MoveUnit(Vector2Int from, Vector2Int to)
    {
        if (!IsInside(from) || !IsInside(to))
            return false;

        Unit unit = GetUnitAt(from);
        if (unit == null)
            return false;

        if (IsOccupied(to))
            return false;

        grid[to.X, to.Y] = unit;
        grid[from.X, from.Y] = null;

        return true;
    }
}