using System.Numerics;

namespace Evil.Engine.Mathematics;

public readonly record struct Transform2D(Vector2 Position, float Rotation, Vector2 Scale, Vector2 Origin)
{
    public static Transform2D Identity => new(Vector2.Zero, 0f, Vector2.One, Vector2.Zero);

    public static Transform2D At(Vector2 position) => Identity with { Position = position };

    public static Transform2D At(float x, float y) => At(new Vector2(x, y));

    public Transform2D Rotated(float degrees) => this with { Rotation = degrees };

    public Transform2D Scaled(float factor) => this with { Scale = new Vector2(factor) };

    public Transform2D Scaled(Vector2 factor) => this with { Scale = factor };

    public Transform2D Around(Vector2 origin) => this with { Origin = origin };

    public Matrix3x2 ToMatrix() =>
        Matrix3x2.CreateTranslation(-Origin)
        * Matrix3x2.CreateScale(Scale)
        * Matrix3x2.CreateRotation(Angle.ToRadians(Rotation))
        * Matrix3x2.CreateTranslation(Position);
}
