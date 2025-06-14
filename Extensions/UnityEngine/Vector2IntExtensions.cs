using UnityEngine;
public static class Vector2IntExtensions
{
    public static Vector2Int Add(this Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x + b.x, a.y + b.y);
    }

    public static Vector2Int Subtract(this Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x - b.x, a.y - b.y);
    }

    public static int ManhattanDistance(this Vector2Int a, Vector2Int b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public static double Magnitude(this Vector2Int v)
    {
        return Math.Sqrt(v.x * v.x + v.y * v.y);
    }

    public static Vector2Int Scale(this Vector2Int v, int scalar)
    {
        return new Vector2Int(v.x * scalar, v.y * scalar);
    }
    public static string ToStringExt(this Vector2Int v) =>
        $"({v.x}, {v.y})";
}

