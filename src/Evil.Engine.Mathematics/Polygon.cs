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

    public static Polygon From(IEnumerable<Vector2> points) => new([.. points]);

    public static Polygon Rectangle(RectF rect) =>
        new(rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft);

    public static Polygon Regular(Vector2 center, float radius, int sides, float rotationDegrees = 0f)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sides, 3);
        var points = new Vector2[sides];
        var start = Angle.ToRadians(rotationDegrees);
        for (var i = 0; i < sides; i++)
        {
            var angle = start + MathF.Tau * i / sides;
            points[i] = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
        }

        return new Polygon(points);
    }

    public static Polygon Star(Vector2 center, float outerRadius, float innerRadius, int points, float rotationDegrees = -90f)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(points, 2);
        var corners = new Vector2[points * 2];
        var start = Angle.ToRadians(rotationDegrees);
        for (var i = 0; i < corners.Length; i++)
        {
            var angle = start + MathF.PI * i / points;
            var radius = i % 2 == 0 ? outerRadius : innerRadius;
            corners[i] = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
        }

        return new Polygon(corners);
    }

    public bool Contains(Vector2 point)
    {
        var inside = false;
        for (int i = 0, j = _points.Length - 1; i < _points.Length; j = i++)
        {
            var a = _points[i];
            var b = _points[j];
            if ((a.Y > point.Y) != (b.Y > point.Y) && point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    public Polygon Translate(Vector2 offset)
    {
        var moved = new Vector2[_points.Length];
        for (var i = 0; i < moved.Length; i++)
        {
            moved[i] = _points[i] + offset;
        }

        return new Polygon(moved);
    }

    public Polygon Transform(Matrix3x2 matrix)
    {
        var moved = new Vector2[_points.Length];
        for (var i = 0; i < moved.Length; i++)
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

        var min = points[0];
        var max = points[0];
        foreach (var point in points)
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
            var sum = Vector2.Zero;
            foreach (var point in points)
            {
                sum += point;
            }

            return sum / points.Length;
        }

        var anchor = points[0];
        var centroid = Vector2.Zero;
        for (var i = 1; i < points.Length - 1; i++)
        {
            var a = points[i] - anchor;
            var b = points[i + 1] - anchor;
            var cross = a.X * b.Y - b.X * a.Y;
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

        var sign = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % points.Length];
            var c = points[(i + 2) % points.Length];
            var cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
            if (MathF.Abs(cross) < 1e-6f)
            {
                continue;
            }

            var current = cross > 0f ? 1 : -1;
            if (sign != 0 && current != sign)
            {
                return false;
            }

            sign = current;
        }

        return sign != 0;
    }
}
