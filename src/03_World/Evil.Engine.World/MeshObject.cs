using Evil.Engine.Mathematics;
using Evil.Engine.Platform;
using Godot;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.World;

public abstract class MeshObject : WorldObject
{
    private protected MeshObject(MeshInstance3D node)
        : base(node)
    {
    }

    private protected MeshObject(MeshInstance3D node, string adoptedName)
        : base(node, adoptedName)
    {
    }

    public bool Pickable { get; set; } = true;

    public bool CastsShadow
    {
        get => Instance.CastShadow != GeometryInstance3D.ShadowCastingSetting.Off;
        set => Instance.CastShadow = value
            ? GeometryInstance3D.ShadowCastingSetting.On
            : GeometryInstance3D.ShadowCastingSetting.Off;
    }

    public Bounds3 LocalBounds
    {
        get
        {
            Aabb box = Instance.GetAabb();
            Vector3 min = box.Position.ToEvil();
            return new Bounds3(min, min + box.Size.ToEvil());
        }
    }

    internal MeshInstance3D Instance => (MeshInstance3D)Node;

    internal float? HitDistance(Ray3 worldRay)
    {
        Transform3D world = WorldTransform;
        Transform3D inverse = world.AffineInverse();
        Godot.Vector3 localDirection = inverse.Basis * worldRay.Direction.ToGodot();
        if (localDirection.LengthSquared() < 1e-12f)
        {
            return null;
        }

        Ray3 localRay = new Ray3((inverse * worldRay.Origin.ToGodot()).ToEvil(), localDirection.ToEvil());
        if (LocalHit(localRay) is not { } along)
        {
            return null;
        }

        Vector3 worldPoint = (world * localRay.PointAt(along).ToGodot()).ToEvil();
        return Vector3.Distance(worldRay.Origin, worldPoint);
    }

    private protected virtual float? LocalHit(Ray3 localRay) => localRay.Intersect(LocalBounds);
}
