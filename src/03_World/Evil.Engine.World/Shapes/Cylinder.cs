using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Cylinder : RoundShape
{
    public Cylinder()
        : this(0.5f, 1f, Color.White)
    {
    }

    public Cylinder(float radius, float height, Color color)
        : base(new CylinderMesh(), height, color) => Radius = radius;

    public float Radius
    {
        get => Round.BottomRadius;
        set
        {
            Round.TopRadius = value;
            Round.BottomRadius = value;
        }
    }
}
