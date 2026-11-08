using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed class Triangulation
{
    private readonly Vector2[] _corners;

    public Triangulation(Vector2[] corners)
    {
        _corners = corners;
        Shape = new Polygon(corners);
    }

    public Polygon Shape { get; }

    public int Count => Shape.Triangles.Count / 3;

    public float CoveredArea
    {
        get
        {
            float covered = 0f;
            for (int i = 0; i < Count; i++)
            {
                covered += MathF.Abs(AreaOf(i));
            }

            return covered;
        }
    }

    public float SmallestArea
    {
        get
        {
            float smallest = float.MaxValue;
            for (int i = 0; i < Count; i++)
            {
                smallest = MathF.Min(smallest, AreaOf(i));
            }

            return smallest;
        }
    }

    public Vector2 BalancePoint
    {
        get
        {
            Vector2 weighted = Vector2.Zero;
            float total = 0f;
            for (int i = 0; i < Count; i++)
            {
                float area = MathF.Abs(AreaOf(i));
                weighted += (Corner(i, 0) + Corner(i, 1) + Corner(i, 2)) / 3f * area;
                total += area;
            }

            return weighted / total;
        }
    }

    public bool Covers(Vector2 point)
    {
        for (int i = 0; i < Count; i++)
        {
            float first = SignedArea(Corner(i, 0), Corner(i, 1), point);
            float second = SignedArea(Corner(i, 1), Corner(i, 2), point);
            float third = SignedArea(Corner(i, 2), Corner(i, 0), point);
            bool allLeft = first >= 0f && second >= 0f && third >= 0f;
            bool allRight = first <= 0f && second <= 0f && third <= 0f;
            if (allLeft || allRight)
            {
                return true;
            }
        }

        return false;
    }

    public float DistanceToEdges(Vector2 point)
    {
        float nearest = float.MaxValue;
        for (int i = 0; i < _corners.Length; i++)
        {
            Vector2 start = _corners[i];
            Vector2 edge = _corners[(i + 1) % _corners.Length] - start;
            float along = Math.Clamp(Vector2.Dot(point - start, edge) / MathF.Max(edge.LengthSquared(), 1e-12f), 0f, 1f);
            nearest = MathF.Min(nearest, Vector2.Distance(point, start + edge * along));
        }

        return nearest;
    }

    private Vector2 Corner(int triangle, int corner) => _corners[Shape.Triangles[triangle * 3 + corner]];

    private float AreaOf(int triangle) => SignedArea(Corner(triangle, 0), Corner(triangle, 1), Corner(triangle, 2));

    private static float SignedArea(Vector2 a, Vector2 b, Vector2 c) => ((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) / 2f;
}
