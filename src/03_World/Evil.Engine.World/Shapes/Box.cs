using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.World;

public sealed class Box : ShapeObject
{
    private readonly BoxMesh _mesh;

    public Box()
        : this(Vector3.One, Color.White)
    {
    }

    public Box(Vector3 size, Color color)
        : base(new BoxMesh(), color)
    {
        _mesh = (BoxMesh)Mesh;
        Size = size;
    }

    public Vector3 Size
    {
        get => _mesh.Size.ToEvil();
        set => _mesh.Size = value.ToGodot();
    }
}
