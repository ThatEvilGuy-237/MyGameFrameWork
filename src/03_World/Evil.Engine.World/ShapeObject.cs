using Evil.Engine.Mathematics;
using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.World;

public abstract class ShapeObject : WorldObject
{
    private Material _material;

    private protected ShapeObject(PrimitiveMesh mesh, Color color)
        : base(new MeshInstance3D { Mesh = mesh })
    {
        Mesh = mesh;
        _material = new Material(color);
        Instance.MaterialOverride = _material.Native;
    }

    public Material Material
    {
        get => _material;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _material = value;
            Instance.MaterialOverride = value.Native;
        }
    }

    public Color Color
    {
        get => _material.Color;
        set => _material.Color = value;
    }

    public bool CastsShadow
    {
        get => Instance.CastShadow != GeometryInstance3D.ShadowCastingSetting.Off;
        set => Instance.CastShadow = value
            ? GeometryInstance3D.ShadowCastingSetting.On
            : GeometryInstance3D.ShadowCastingSetting.Off;
    }

    public bool Pickable { get; set; } = true;

    private protected PrimitiveMesh Mesh { get; }

    private MeshInstance3D Instance => (MeshInstance3D)Node;

    internal float? HitDistance(Ray3 worldRay)
    {
        var world = WorldTransform;
        var inverse = world.AffineInverse();
        var localDirection = inverse.Basis * worldRay.Direction.ToGodot();
        if (localDirection.LengthSquared() < 1e-12f)
        {
            return null;
        }

        var localRay = new Ray3((inverse * worldRay.Origin.ToGodot()).ToEvil(), localDirection.ToEvil());
        if (LocalHit(localRay) is not { } along)
        {
            return null;
        }

        var worldPoint = (world * localRay.PointAt(along).ToGodot()).ToEvil();
        return Vector3.Distance(worldRay.Origin, worldPoint);
    }

    private protected virtual float? LocalHit(Ray3 localRay)
    {
        var box = Instance.GetAabb();
        var min = box.Position.ToEvil();
        return localRay.Intersect(new Bounds3(min, min + box.Size.ToEvil()));
    }
}
