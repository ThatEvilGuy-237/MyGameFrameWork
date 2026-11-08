using System.Numerics;

namespace Evil.Engine.Mathematics;

public static class Triangulator
{
    private const float Epsilon = 1e-6f;

    public static int[] Triangulate(ReadOnlySpan<Vector2> points)
    {
        if (points.Length < 3)
        {
            return [];
        }

        var remaining = new List<int>(points.Length);
        for (var i = 0; i < points.Length; i++)
        {
            remaining.Add(i);
        }

        if (SignedArea(points) < 0f)
        {
            remaining.Reverse();
        }

        var triangles = new List<int>((points.Length - 2) * 3);
        while (remaining.Count > 3)
        {
            if (TryClipEar(points, remaining, triangles) || TryDropCollinear(points, remaining))
            {
                continue;
            }

            AddFan(remaining, triangles);
            return [.. triangles];
        }

        triangles.AddRange(remaining);
        return [.. triangles];
    }

    public static float SignedArea(ReadOnlySpan<Vector2> points)
    {
        if (points.Length < 3)
        {
            return 0f;
        }

        var anchor = points[0];
        var sum = 0f;
        for (var i = 1; i < points.Length - 1; i++)
        {
            sum += Cross(points[i] - anchor, points[i + 1] - anchor);
        }

        return sum / 2f;
    }

    private static bool TryClipEar(ReadOnlySpan<Vector2> points, List<int> remaining, List<int> triangles)
    {
        var count = remaining.Count;
        for (var i = 0; i < count; i++)
        {
            var previous = remaining[(i + count - 1) % count];
            var current = remaining[i];
            var next = remaining[(i + 1) % count];
            if (!IsEar(points, remaining, previous, current, next))
            {
                continue;
            }

            triangles.Add(previous);
            triangles.Add(current);
            triangles.Add(next);
            remaining.RemoveAt(i);
            return true;
        }

        return false;
    }

    private static bool IsEar(ReadOnlySpan<Vector2> points, List<int> remaining, int previous, int current, int next)
    {
        var a = points[previous];
        var b = points[current];
        var c = points[next];
        if (Cross(b - a, c - b) <= Epsilon)
        {
            return false;
        }

        foreach (var index in remaining)
        {
            if (index == previous || index == current || index == next)
            {
                continue;
            }

            var point = points[index];
            if (point == a || point == b || point == c)
            {
                continue;
            }

            if (InTriangle(point, a, b, c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryDropCollinear(ReadOnlySpan<Vector2> points, List<int> remaining)
    {
        var count = remaining.Count;
        for (var i = 0; i < count; i++)
        {
            var a = points[remaining[(i + count - 1) % count]];
            var b = points[remaining[i]];
            var c = points[remaining[(i + 1) % count]];
            if (MathF.Abs(Cross(b - a, c - b)) > Epsilon)
            {
                continue;
            }

            remaining.RemoveAt(i);
            return true;
        }

        return false;
    }

    private static void AddFan(List<int> remaining, List<int> triangles)
    {
        for (var i = 1; i < remaining.Count - 1; i++)
        {
            triangles.Add(remaining[0]);
            triangles.Add(remaining[i]);
            triangles.Add(remaining[i + 1]);
        }
    }

    private static bool InTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c) =>
        Cross(b - a, point - a) >= 0f && Cross(c - b, point - b) >= 0f && Cross(a - c, point - c) >= 0f;

    private static float Cross(Vector2 u, Vector2 v) => u.X * v.Y - u.Y * v.X;
}
