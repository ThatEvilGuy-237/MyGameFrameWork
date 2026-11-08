using System.Numerics;

namespace Evil.Engine.Mathematics;

public readonly record struct Pose(Vector3 Position, Quaternion Rotation, Vector3 Scale)
{
    public static Pose Identity => new(Vector3.Zero, Quaternion.Identity, Vector3.One);

    public static Pose At(Vector3 position) => Identity with { Position = position };

    public static Pose At(float x, float y, float z) => At(new Vector3(x, y, z));

    public Vector3 Forward => Vector3.Transform(Rotations.Forward, Rotation);

    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);

    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);

    public Pose Turned(Quaternion rotation) => this with { Rotation = Quaternion.Normalize(rotation * Rotation) };

    public Pose TurnedY(float degrees) => Turned(Rotations.AroundAxis(Vector3.UnitY, degrees));

    public Pose Moved(Vector3 offset) => this with { Position = Position + offset };

    public Vector3 ToWorld(Vector3 local) => Position + Vector3.Transform(local * Scale, Rotation);

    public Vector3 ToLocal(Vector3 world) => Vector3.Transform(world - Position, Quaternion.Inverse(Rotation)) / Scale;

    public Pose ToWorld(Pose local) => new(
        ToWorld(local.Position),
        Quaternion.Normalize(Rotation * local.Rotation),
        Scale * local.Scale);

    public Matrix4x4 ToMatrix() =>
        Matrix4x4.CreateScale(Scale)
        * Matrix4x4.CreateFromQuaternion(Rotation)
        * Matrix4x4.CreateTranslation(Position);
}
