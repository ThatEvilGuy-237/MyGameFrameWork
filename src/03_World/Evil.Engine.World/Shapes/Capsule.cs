using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Capsule : ShapeObject
{
    private readonly CapsuleMesh _mesh;

    public Capsule()
        : this(0.5f, 2f, Color.White)
    {
    }

    public Capsule(float radius, float height, Color color)
        : base(Checked(radius, height), color)
    {
        _mesh = (CapsuleMesh)Mesh;
    }

    public float Radius
    {
        get => _mesh.Radius;
        set => _mesh.Radius = value;
    }

    public float Height
    {
        get => _mesh.Height;
        set => _mesh.Height = value;
    }

    private static CapsuleMesh Checked(float radius, float height)
    {
        if (height < radius * 2f)
        {
            throw new ArgumentOutOfRangeException(nameof(height), $"A capsule's height ({height}) includes both round ends, so it needs at least twice the radius ({radius * 2f}).");
        }

        CapsuleMesh mesh = new CapsuleMesh();
        mesh.Radius = radius;
        mesh.Height = height;
        return mesh;
    }
}
