using System.Numerics;

namespace Evil.Engine.Mathematics;

public sealed class Polygon
{
    private readonly Vector2[] _points;
    private int[]? _triangles;

    public Polygon(params ReadOnlySpan<Vector2> points)
    {
        _points = points.ToArray();
        SignedArea = Triangulator.SignedArea(_points);
        Bounds = MeasureBounds(_points);
        Centroid = MeasureCentroid(_points, SignedArea);
        IsConvex = MeasureConvex(_points);
    }

    public IReadOnlyList<Vector2> Points => _points;

    public int Count => _points.Length;

    public RectF Bounds { get; }

    public float SignedArea { get; }

    public float Area => MathF.Abs(SignedArea);

    public Vector2 Centroid { get; }

    public bool IsConvex { get; }

    public IReadOnlyList<int> Triangles => _triangles ??= Triangulator.Triangulate(_points);

    public static Polygon From(IEnumerable<Vector2> points) => new Polygon([.. points]);

    public static Polygon Rectangle(RectF rect) =>
        new Polygon(rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft);

    public static Polygon Regular(Vector2 center, float radius, int sides, float rotationDegrees = 0f)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sides, 3);
        Vector2[] points = new Vector2[sides];
        float start = Angle.ToRadians(rotationDegrees);
        for (int i = 0; i < sides; i++)
        {
            float angle = start + MathF.Tau * i / sides;
            points[i] = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
        }

        return new Polygon(points);
    }

    public static Polygon Star(Vector2 center, float outerRadius, float innerRadius, int points, float rotationDegrees = -90f)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(points, 2);
        Vector2[] corners = new Vector2[points * 2];
        float start = Angle.ToRadians(rotationDegrees);
        for (int i = 0; i < corners.Length; i++)
        {
            float angle = start + MathF.PI * i / points;
            float radius = i % 2 == 0 ? outerRadius : innerRadius;
            corners[i] = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
        }

        return new Polygon(corners);
    }

    public bool Contains(Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = _points.Length - 1; i < _points.Length; j = i++)
        {
            Vector2 a = _points[i];
            Vector2 b = _points[j];
            if ((a.Y > point.Y) != (b.Y > point.Y) && point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    public Polygon Translate(Vector2 offset)
    {
        Vector2[] moved = new Vector2[_points.Length];
        for (int i = 0; i < moved.Length; i++)
        {
            moved[i] = _points[i] + offset;
        }

        return new Polygon(moved);
    }

    public Polygon Transform(Matrix3x2 matrix)
    {
        Vector2[] moved = new Vector2[_points.Length];
        for (int i = 0; i < moved.Length; i++)
        {
            moved[i] = Vector2.Transform(_points[i], matrix);
        }

        return new Polygon(moved);
    }

    public Polygon CenteredOnOrigin() => Translate(-Centroid);

    private static RectF MeasureBounds(ReadOnlySpan<Vector2> points)
    {
        if (points.IsEmpty)
        {
            return RectF.Empty;
        }

        Vector2 min = points[0];
        Vector2 max = points[0];
        foreach (Vector2 point in points)
        {
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }

        return RectF.FromCorners(min, max);
    }

    private static Vector2 MeasureCentroid(ReadOnlySpan<Vector2> points, float signedArea)
    {
        if (points.IsEmpty)
        {
            return Vector2.Zero;
        }

        if (MathF.Abs(signedArea) < 1e-6f)
        {
            Vector2 sum = Vector2.Zero;
            foreach (Vector2 point in points)
            {
                sum += point;
            }

            return sum / points.Length;
        }

        Vector2 anchor = points[0];
        Vector2 centroid = Vector2.Zero;
        for (int i = 1; i < points.Length - 1; i++)
        {
            Vector2 a = points[i] - anchor;
            Vector2 b = points[i + 1] - anchor;
            float cross = a.X * b.Y - b.X * a.Y;
            centroid += (a + b) * cross;
        }

        return anchor + centroid / (6f * signedArea);
    }

    private static bool MeasureConvex(ReadOnlySpan<Vector2> points)
    {
        if (points.Length < 3)
        {
            return false;
        }

        int sign = 0;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Length];
            Vector2 c = points[(i + 2) % points.Length];
            float cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
            if (MathF.Abs(cross) < 1e-6f)
            {
                continue;
            }

            int current = cross > 0f ? 1 : -1;
            if (sign != 0 && current != sign)
            {
                return false;
            }

            sign = current;
        }

        return sign != 0;
    }
}
