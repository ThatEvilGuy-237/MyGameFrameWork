using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.World;

public sealed class Box : ShapeObject
{
    public Box()
        : this(Vector3.One, Color.White)
    {
    }

    public Box(Vector3 size, Color color)
        : base(new BoxMesh(), color) => Size = size;

    public Vector3 Size
    {
        get => ((BoxMesh)Mesh).Size.ToEvil();
        set => ((BoxMesh)Mesh).Size = value.ToGodot();
    }
}
