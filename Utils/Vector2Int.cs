using WizardsServer.ServerLogic;

namespace WizardsServer.Utils;

public struct Vector2Int : IEquatable<Vector2Int>
{
    public int X;
    public int Y;

    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }
    public Vector2Int()
    {
        X = 0;
        Y = 0;
    }

    public static Vector2Int Zero => new Vector2Int(0, 0);
    public static Vector2Int One => new Vector2Int(1, 1);

    public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.X + b.X, a.Y + b.Y);
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.X - b.X, a.Y - b.Y);
    public static bool operator ==(Vector2Int a, Vector2Int b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vector2Int a, Vector2Int b) => !(a == b);

    public override bool Equals(object? obj) => obj is Vector2Int other && this == other;
    public bool Equals(Vector2Int other) => this == other;
    public override int GetHashCode() => (X, Y).GetHashCode();
    public override string ToString() => $"{X} {Y}";
}