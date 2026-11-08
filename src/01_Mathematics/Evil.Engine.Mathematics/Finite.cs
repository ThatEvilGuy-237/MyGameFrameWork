using System.Numerics;

namespace Evil.Engine.Mathematics;

internal static class Finite
{
    private const string Hint = "NaN and infinity usually come from a division by zero or from normalizing a zero-length vector earlier on.";

    public static float Number(float value, string what)
    {
        if (!float.IsFinite(value))
        {
            throw new ArgumentException($"{what} must be a finite number, got {value}. {Hint}", what);
        }

        return value;
    }

    public static Vector2 Point(Vector2 value, string what)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
        {
            throw new ArgumentException($"{what} must be finite, got {value}. {Hint}", what);
        }

        return value;
    }

    public static Vector3 Point(Vector3 value, string what)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || !float.IsFinite(value.Z))
        {
            throw new ArgumentException($"{what} must be finite, got {value}. {Hint}", what);
        }

        return value;
    }

    public static Quaternion Rotation(Quaternion value, string what)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || !float.IsFinite(value.Z) || !float.IsFinite(value.W))
        {
            throw new ArgumentException($"{what} must be finite, got {value}. {Hint}", what);
        }

        return value;
    }

    public static Vector3 Direction(Vector3 value, string what)
    {
        Point(value, what);
        if (value.LengthSquared() < 1e-12f)
        {
            throw new ArgumentException($"{what} has no length, so it points nowhere. Give a direction such as Vector3.UnitY.", what);
        }

        return value;
    }
}
