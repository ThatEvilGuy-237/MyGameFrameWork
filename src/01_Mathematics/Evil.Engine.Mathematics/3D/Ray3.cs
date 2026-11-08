using System.Numerics;

namespace Evil.Engine.Mathematics;

public readonly record struct Ray3
{
    public Ray3(Vector3 origin, Vector3 direction)
    {
        Origin = Finite.Point(origin, "A ray's origin");
        Direction = Vector3.Normalize(Finite.Direction(direction, "A ray's direction"));
    }

    public Vector3 Origin { get; }

    public Vector3 Direction { get; }

    public Vector3 PointAt(float distance) => Origin + Direction * distance;

    public float? Intersect(Bounds3 box)
    {
        Interval inside = Interval.Ahead;
        for (int axis = 0; axis < 3; axis++)
        {
            inside = inside.Overlap(Slab(box, axis));
            if (inside.IsEmpty)
            {
                return null;
            }
        }

        return inside.Start;
    }

    public float? IntersectPlane(Vector3 pointOnPlane, Vector3 normal)
    {
        Finite.Point(pointOnPlane, "The plane's point");
        Finite.Direction(normal, "The plane's normal");
        float facing = Vector3.Dot(Direction, normal);
        if (MathF.Abs(facing) < 1e-9f)
        {
            return null;
        }

        float distance = Vector3.Dot(pointOnPlane - Origin, normal) / facing;
        return distance >= 0f ? distance : null;
    }

    public float? IntersectSphere(Vector3 center, float radius)
    {
        Finite.Point(center, "The sphere's center");
        if (Finite.Number(radius, "The sphere's radius") < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), radius, "A sphere's radius cannot be negative.");
        }

        Vector3 toCenter = center - Origin;
        float closestAlong = Vector3.Dot(toCenter, Direction);
        Vector3 gap = toCenter - Direction * closestAlong;
        float halfChordSquared = radius * radius - gap.LengthSquared();
        if (halfChordSquared < 0f)
        {
            return null;
        }

        float halfChord = MathF.Sqrt(halfChordSquared);
        float far = closestAlong + halfChord;
        if (far < 0f)
        {
            return null;
        }

        float near = closestAlong - halfChord;
        return near >= 0f ? near : 0f;
    }

    private Interval Slab(Bounds3 box, int axis)
    {
        float origin = Origin[axis];
        float direction = Direction[axis];
        if (MathF.Abs(direction) < 1e-9f)
        {
            bool between = origin >= box.Min[axis] && origin <= box.Max[axis];
            return between ? Interval.Everywhere : Interval.Nowhere;
        }

        return Interval.Between((box.Min[axis] - origin) / direction, (box.Max[axis] - origin) / direction);
    }
}
