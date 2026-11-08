using System.Numerics;

namespace Evil.Engine.Mathematics;

public static class Rotations
{
    public static Vector3 Forward => -Vector3.UnitZ;

    public static Quaternion FromEulerDegrees(Vector3 degrees)
    {
        Finite.Point(degrees, "Euler degrees");
        return Quaternion.CreateFromYawPitchRoll(
            Angle.ToRadians(degrees.Y),
            Angle.ToRadians(degrees.X),
            Angle.ToRadians(degrees.Z));
    }

    public static Quaternion AroundAxis(Vector3 axis, float degrees)
    {
        Finite.Direction(axis, "The axis to turn around");
        Finite.Number(degrees, "The degrees to turn");
        return Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), Angle.ToRadians(degrees));
    }

    public static Quaternion LookAlong(Vector3 forward)
    {
        Finite.Direction(forward, "The direction to look along");
        Vector3 front = Vector3.Normalize(forward);
        Vector3 up = MathF.Abs(Vector3.Dot(front, Vector3.UnitY)) > 0.999f ? Forward : Vector3.UnitY;
        Vector3 back = -front;
        Vector3 right = Vector3.Normalize(Vector3.Cross(up, back));
        Vector3 top = Vector3.Cross(back, right);
        Matrix4x4 basis = new Matrix4x4(
            right.X, right.Y, right.Z, 0f,
            top.X, top.Y, top.Z, 0f,
            back.X, back.Y, back.Z, 0f,
            0f, 0f, 0f, 1f);
        return Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(basis));
    }

    public static Vector3 Rotate(Vector3 vector, Quaternion rotation) => Vector3.Transform(vector, rotation);
}
