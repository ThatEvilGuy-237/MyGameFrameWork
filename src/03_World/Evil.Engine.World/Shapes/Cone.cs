using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Cone : RoundShape
{
    public Cone()
        : this(0.5f, 1f, Color.White)
    {
    }

    public Cone(float radius, float height, Color color)
        : base(PointedMesh(), height, color) => Radius = radius;

    public float Radius
    {
        get => Round.BottomRadius;
        set => Round.BottomRadius = value;
    }

    private static CylinderMesh PointedMesh()
    {
        CylinderMesh mesh = new CylinderMesh();
        mesh.TopRadius = 0f;
        return mesh;
    }
}
