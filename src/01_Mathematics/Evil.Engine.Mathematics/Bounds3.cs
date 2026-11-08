using System.Numerics;

namespace Evil.Engine.Mathematics;

public readonly record struct Bounds3(Vector3 Min, Vector3 Max)
{
    public Vector3 Center => (Min + Max) / 2f;

    public Vector3 Size => Max - Min;

    public bool IsEmpty => Max.X < Min.X || Max.Y < Min.Y || Max.Z < Min.Z;

    public static Bounds3 FromCenter(Vector3 center, Vector3 size)
    {
        var half = Vector3.Abs(size) / 2f;
        return new Bounds3(center - half, center + half);
    }

    public static Bounds3 FromPoints(ReadOnlySpan<Vector3> points)
    {
        if (points.IsEmpty)
        {
            throw new ArgumentException("Bounds need at least one point.", nameof(points));
        }

        var min = points[0];
        var max = points[0];
        foreach (var point in points)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        return new Bounds3(min, max);
    }

    public bool Contains(Vector3 point) =>
        point.X >= Min.X && point.X <= Max.X
        && point.Y >= Min.Y && point.Y <= Max.Y
        && point.Z >= Min.Z && point.Z <= Max.Z;

    public bool Intersects(Bounds3 other) =>
        other.Min.X <= Max.X && other.Max.X >= Min.X
        && other.Min.Y <= Max.Y && other.Max.Y >= Min.Y
        && other.Min.Z <= Max.Z && other.Max.Z >= Min.Z;

    public Bounds3 Encapsulate(Vector3 point) => new(Vector3.Min(Min, point), Vector3.Max(Max, point));

    public Bounds3 Union(Bounds3 other) => new(Vector3.Min(Min, other.Min), Vector3.Max(Max, other.Max));

    public Bounds3 Expand(float amount) => new(Min - new Vector3(amount), Max + new Vector3(amount));
}
