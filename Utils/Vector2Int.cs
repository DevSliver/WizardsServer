namespace WizardsServer.Utils;

public struct Vector2Int
{
    public int X { get; }
    public int Y { get; }

    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector2Int other)
            return X == other.X && Y == other.Y;
        return false;
    }

    public override int GetHashCode() => (X, Y).GetHashCode();

    public static bool operator ==(Vector2Int a, Vector2Int b) => a.Equals(b);
    public static bool operator !=(Vector2Int a, Vector2Int b) => !a.Equals(b);
}