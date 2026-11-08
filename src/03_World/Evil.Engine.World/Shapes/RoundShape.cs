using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public abstract class RoundShape : ShapeObject
{
    private protected RoundShape(CylinderMesh mesh, float height, Color color)
        : base(mesh, color)
    {
        Round = mesh;
        Height = height;
    }

    public float Height
    {
        get => Round.Height;
        set => Round.Height = value;
    }

    private protected CylinderMesh Round { get; }
}
