using System.Numerics;

namespace Evil.Engine.Mathematics;

public static class Rotations
{
    public static Vector3 Forward => -Vector3.UnitZ;

    public static Quaternion FromEulerDegrees(Vector3 degrees) =>
        Quaternion.CreateFromYawPitchRoll(
            Angle.ToRadians(degrees.Y),
            Angle.ToRadians(degrees.X),
            Angle.ToRadians(degrees.Z));

    public static Quaternion AroundAxis(Vector3 axis, float degrees)
    {
        if (axis.LengthSquared() < 1e-12f)
        {
            throw new ArgumentException("The axis to turn around has no length.", nameof(axis));
        }

        return Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), Angle.ToRadians(degrees));
    }

    public static Quaternion LookAlong(Vector3 forward)
    {
        if (forward.LengthSquared() < 1e-12f)
        {
            throw new ArgumentException("The direction to look along has no length.", nameof(forward));
        }

        var front = Vector3.Normalize(forward);
        var up = MathF.Abs(Vector3.Dot(front, Vector3.UnitY)) > 0.999f ? Forward : Vector3.UnitY;
        var back = -front;
        var right = Vector3.Normalize(Vector3.Cross(up, back));
        var top = Vector3.Cross(back, right);
        var basis = new Matrix4x4(
            right.X, right.Y, right.Z, 0f,
            top.X, top.Y, top.Z, 0f,
            back.X, back.Y, back.Z, 0f,
            0f, 0f, 0f, 1f);
        return Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(basis));
    }

    public static Vector3 Rotate(Vector3 vector, Quaternion rotation) => Vector3.Transform(vector, rotation);
}
