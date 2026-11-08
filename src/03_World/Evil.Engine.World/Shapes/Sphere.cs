using Evil.Engine.Mathematics;
using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Sphere : ShapeObject
{
    private readonly SphereMesh _mesh;

    public Sphere()
        : this(0.5f, Color.White)
    {
    }

    public Sphere(float radius, Color color)
        : base(new SphereMesh(), color)
    {
        _mesh = (SphereMesh)Mesh;
        Radius = radius;
    }

    public float Radius
    {
        get => _mesh.Radius;
        set
        {
            _mesh.Radius = value;
            _mesh.Height = value * 2f;
        }
    }

    private protected override float? LocalHit(Ray3 localRay) => localRay.IntersectSphere(System.Numerics.Vector3.Zero, Radius);
}
