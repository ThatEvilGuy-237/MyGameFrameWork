using System.Numerics;

namespace Evil.Engine.Mathematics;

public readonly record struct Ray3
{
    public Ray3(Vector3 origin, Vector3 direction)
    {
        if (direction.LengthSquared() < 1e-12f)
        {
            throw new ArgumentException("A ray needs a direction with a length.", nameof(direction));
        }

        Origin = origin;
        Direction = Vector3.Normalize(direction);
    }

    public Vector3 Origin { get; }

    public Vector3 Direction { get; }

    public Vector3 PointAt(float distance) => Origin + Direction * distance;

    public float? Intersect(Bounds3 box)
    {
        var enter = 0f;
        var leave = float.PositiveInfinity;
        for (var axis = 0; axis < 3; axis++)
        {
            var origin = Component(Origin, axis);
            var direction = Component(Direction, axis);
            var min = Component(box.Min, axis);
            var max = Component(box.Max, axis);
            if (MathF.Abs(direction) < 1e-9f)
            {
                if (origin < min || origin > max)
                {
                    return null;
                }

                continue;
            }

            var near = (min - origin) / direction;
            var far = (max - origin) / direction;
            if (near > far)
            {
                (near, far) = (far, near);
            }

            enter = MathF.Max(enter, near);
            leave = MathF.Min(leave, far);
            if (enter > leave)
            {
                return null;
            }
        }

        return enter;
    }

    public float? IntersectPlane(Vector3 pointOnPlane, Vector3 normal)
    {
        var facing = Vector3.Dot(Direction, normal);
        if (MathF.Abs(facing) < 1e-9f)
        {
            return null;
        }

        var distance = Vector3.Dot(pointOnPlane - Origin, normal) / facing;
        return distance >= 0f ? distance : null;
    }

    public float? IntersectSphere(Vector3 center, float radius)
    {
        var toCenter = Origin - center;
        var b = Vector3.Dot(toCenter, Direction);
        var c = toCenter.LengthSquared() - radius * radius;
        var discriminant = b * b - c;
        if (discriminant < 0f)
        {
            return null;
        }

        var root = MathF.Sqrt(discriminant);
        var near = -b - root;
        var far = -b + root;
        if (far < 0f)
        {
            return null;
        }

        return near >= 0f ? near : 0f;
    }

    private static float Component(Vector3 vector, int axis) => axis switch
    {
        0 => vector.X,
        1 => vector.Y,
        _ => vector.Z,
    };
}
