using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Cone : ShapeObject
{
    public Cone()
        : this(0.5f, 1f, Color.White)
    {
    }

    public Cone(float radius, float height, Color color)
        : base(new CylinderMesh { TopRadius = 0f }, color)
    {
        Radius = radius;
        Height = height;
    }

    public float Radius
    {
        get => ((CylinderMesh)Mesh).BottomRadius;
        set => ((CylinderMesh)Mesh).BottomRadius = value;
    }

    public float Height
    {
        get => ((CylinderMesh)Mesh).Height;
        set => ((CylinderMesh)Mesh).Height = value;
    }
}
