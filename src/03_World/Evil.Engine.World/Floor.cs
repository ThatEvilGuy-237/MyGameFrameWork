using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;
using Vector2 = System.Numerics.Vector2;

namespace Evil.Engine.World;

public sealed class Floor : ShapeObject
{
    public Floor()
        : this(new Vector2(10f, 10f), Color.Gray)
    {
    }

    public Floor(Vector2 size, Color color)
        : base(new PlaneMesh(), color) => Size = size;

    public Vector2 Size
    {
        get => ((PlaneMesh)Mesh).Size.ToEvil();
        set => ((PlaneMesh)Mesh).Size = value.ToGodot();
    }
}
