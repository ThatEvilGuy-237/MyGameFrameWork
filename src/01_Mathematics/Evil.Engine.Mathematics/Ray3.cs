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
        float enter = 0f;
        float leave = float.PositiveInfinity;
        for (int axis = 0; axis < 3; axis++)
        {
            float origin = Component(Origin, axis);
            float direction = Component(Direction, axis);
            float min = Component(box.Min, axis);
            float max = Component(box.Max, axis);
            if (MathF.Abs(direction) < 1e-9f)
            {
                if (origin < min || origin > max)
                {
                    return null;
                }

                continue;
            }

            float near = (min - origin) / direction;
            float far = (max - origin) / direction;
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
        Vector3 toCenter = Origin - center;
        float b = Vector3.Dot(toCenter, Direction);
        float c = toCenter.LengthSquared() - radius * radius;
        float discriminant = b * b - c;
        if (discriminant < 0f)
        {
            return null;
        }

        float root = MathF.Sqrt(discriminant);
        float near = -b - root;
        float far = -b + root;
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
