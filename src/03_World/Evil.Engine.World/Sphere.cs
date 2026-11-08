using Evil.Engine.Mathematics;
using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Sphere : ShapeObject
{
    public Sphere()
        : this(0.5f, Color.White)
    {
    }

    public Sphere(float radius, Color color)
        : base(new SphereMesh(), color) => Radius = radius;

    public float Radius
    {
        get => ((SphereMesh)Mesh).Radius;
        set
        {
            SphereMesh mesh = (SphereMesh)Mesh;
            mesh.Radius = value;
            mesh.Height = value * 2f;
        }
    }

    private protected override float? LocalHit(Ray3 localRay) => localRay.IntersectSphere(System.Numerics.Vector3.Zero, Radius);
}
