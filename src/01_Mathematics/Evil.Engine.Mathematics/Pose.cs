using System.Numerics;

namespace Evil.Engine.Mathematics;

public readonly record struct Pose(Vector3 Position, Quaternion Rotation, Vector3 Scale)
{
    public static Pose Identity => new Pose(Vector3.Zero, Quaternion.Identity, Vector3.One);

    public static Pose At(Vector3 position) => Identity with { Position = position };

    public static Pose At(float x, float y, float z) => At(new Vector3(x, y, z));

    public Vector3 Forward => Vector3.Transform(Rotations.Forward, Rotation);

    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);

    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);

    public Pose Turned(Quaternion rotation) => this with { Rotation = Quaternion.Normalize(rotation * Rotation) };

    public Pose TurnedY(float degrees) => Turned(Rotations.AroundAxis(Vector3.UnitY, degrees));

    public Pose Moved(Vector3 offset) => this with { Position = Position + offset };

    public Vector3 ToWorld(Vector3 local) => Position + Vector3.Transform(local * Scale, Rotation);

    public Vector3 ToLocal(Vector3 world)
    {
        if (Scale.X == 0f || Scale.Y == 0f || Scale.Z == 0f)
        {
            throw new InvalidOperationException($"A pose with a zero scale ({Scale}) squashes space flat, so there is no way back to its local space.");
        }

        return Vector3.Transform(world - Position, Quaternion.Inverse(Rotation)) / Scale;
    }

    public Pose ToWorld(Pose local)
    {
        if (!IsEvenlyScaled(Scale) && !IsUnturned(local.Rotation))
        {
            throw new InvalidOperationException(
                $"A parent scaled unevenly ({Scale}) skews a turned child, and a Pose cannot hold a skew. Scale the parent evenly, or use WorldObject.ToWorld for points.");
        }

        return new Pose(ToWorld(local.Position), Quaternion.Normalize(Rotation * local.Rotation), Scale * local.Scale);
    }

    private static bool IsEvenlyScaled(Vector3 scale)
    {
        float smallest = MathF.Min(MathF.Abs(scale.X), MathF.Min(MathF.Abs(scale.Y), MathF.Abs(scale.Z)));
        float largest = MathF.Max(MathF.Abs(scale.X), MathF.Max(MathF.Abs(scale.Y), MathF.Abs(scale.Z)));
        return largest - smallest <= 1e-4f * MathF.Max(1f, largest);
    }

    private static bool IsUnturned(Quaternion rotation) => MathF.Abs(MathF.Abs(rotation.W) - 1f) < 1e-6f;

    public Matrix4x4 ToMatrix() =>
        Matrix4x4.CreateScale(Scale)
        * Matrix4x4.CreateFromQuaternion(Rotation)
        * Matrix4x4.CreateTranslation(Position);
}
