using System.Numerics;

namespace Evil.Engine.Mathematics;

public static class Triangulator
{
    public static int[] Triangulate(ReadOnlySpan<Vector2> points) =>
        points.Length < 3 ? [] : new EarClipper(points).Run();

    public static float SignedArea(ReadOnlySpan<Vector2> points)
    {
        if (points.Length < 3)
        {
            return 0f;
        }

        Vector2 anchor = points[0];
        float sum = 0f;
        for (int i = 1; i < points.Length - 1; i++)
        {
            sum += Cross(points[i] - anchor, points[i + 1] - anchor);
        }

        return sum / 2f;
    }

    internal static float Cross(Vector2 u, Vector2 v) => u.X * v.Y - u.Y * v.X;
}
