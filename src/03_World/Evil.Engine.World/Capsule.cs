using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Capsule : ShapeObject
{
    public Capsule()
        : this(0.5f, 2f, Color.White)
    {
    }

    public Capsule(float radius, float height, Color color)
        : base(Checked(radius, height), color)
    {
    }

    public float Radius
    {
        get => ((CapsuleMesh)Mesh).Radius;
        set => ((CapsuleMesh)Mesh).Radius = value;
    }

    public float Height
    {
        get => ((CapsuleMesh)Mesh).Height;
        set => ((CapsuleMesh)Mesh).Height = value;
    }

    private static CapsuleMesh Checked(float radius, float height)
    {
        if (height < radius * 2f)
        {
            throw new ArgumentOutOfRangeException(nameof(height), $"A capsule's height ({height}) includes both round ends, so it needs at least twice the radius ({radius * 2f}).");
        }

        return new CapsuleMesh { Radius = radius, Height = height };
    }
}
