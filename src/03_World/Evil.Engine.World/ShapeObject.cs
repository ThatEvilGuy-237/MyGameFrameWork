using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public abstract class ShapeObject : WorldObject
{
    private readonly StandardMaterial3D _material = new();

    private protected ShapeObject(PrimitiveMesh mesh, Color color)
        : base(new MeshInstance3D { Mesh = mesh })
    {
        Mesh = mesh;
        Instance.MaterialOverride = _material;
        Color = color;
    }

    public Color Color
    {
        get => _material.AlbedoColor.ToEvil();
        set
        {
            _material.AlbedoColor = value.ToGodot();
            _material.Transparency = value.A < 1f
                ? BaseMaterial3D.TransparencyEnum.Alpha
                : BaseMaterial3D.TransparencyEnum.Disabled;
        }
    }

    public bool CastsShadow
    {
        get => Instance.CastShadow != GeometryInstance3D.ShadowCastingSetting.Off;
        set => Instance.CastShadow = value
            ? GeometryInstance3D.ShadowCastingSetting.On
            : GeometryInstance3D.ShadowCastingSetting.Off;
    }

    private protected PrimitiveMesh Mesh { get; }

    private MeshInstance3D Instance => (MeshInstance3D)Node;
}
