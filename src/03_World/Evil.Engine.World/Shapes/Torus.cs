using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Torus : ShapeObject
{
    private readonly TorusMesh _mesh;

    public Torus()
        : this(0.4f, 0.6f, Color.White)
    {
    }

    public Torus(float innerRadius, float outerRadius, Color color)
        : base(Checked(innerRadius, outerRadius), color)
    {
        _mesh = (TorusMesh)Mesh;
    }

    public float InnerRadius
    {
        get => _mesh.InnerRadius;
        set => _mesh.InnerRadius = value;
    }

    public float OuterRadius
    {
        get => _mesh.OuterRadius;
        set => _mesh.OuterRadius = value;
    }

    private static TorusMesh Checked(float innerRadius, float outerRadius)
    {
        if (outerRadius <= innerRadius)
        {
            throw new ArgumentOutOfRangeException(nameof(outerRadius), $"A torus needs the outer radius ({outerRadius}) bigger than the inner one ({innerRadius}).");
        }

        TorusMesh mesh = new TorusMesh();
        mesh.InnerRadius = innerRadius;
        mesh.OuterRadius = outerRadius;
        return mesh;
    }
}
