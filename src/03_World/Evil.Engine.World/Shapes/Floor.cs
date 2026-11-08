using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;
using Vector2 = System.Numerics.Vector2;

namespace Evil.Engine.World;

public sealed class Floor : ShapeObject
{
    private readonly PlaneMesh _mesh;

    public Floor()
        : this(new Vector2(10f, 10f), Color.Gray)
    {
    }

    public Floor(Vector2 size, Color color)
        : base(new PlaneMesh(), color)
    {
        _mesh = (PlaneMesh)Mesh;
        Size = size;
    }

    public Vector2 Size
    {
        get => _mesh.Size.ToEvil();
        set => _mesh.Size = value.ToGodot();
    }
}
