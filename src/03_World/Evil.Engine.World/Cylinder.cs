using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Cylinder : ShapeObject
{
    public Cylinder()
        : this(0.5f, 1f, Color.White)
    {
    }

    public Cylinder(float radius, float height, Color color)
        : base(new CylinderMesh(), color)
    {
        Radius = radius;
        Height = height;
    }

    public float Radius
    {
        get => ((CylinderMesh)Mesh).BottomRadius;
        set
        {
            var mesh = (CylinderMesh)Mesh;
            mesh.TopRadius = value;
            mesh.BottomRadius = value;
        }
    }

    public float Height
    {
        get => ((CylinderMesh)Mesh).Height;
        set => ((CylinderMesh)Mesh).Height = value;
    }
}
