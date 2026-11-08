using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public abstract class ShapeObject : MeshObject
{
    private Material _material;

    private protected ShapeObject(PrimitiveMesh mesh, Color color)
        : base(Holding(mesh))
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

    private protected PrimitiveMesh Mesh { get; }

    private static MeshInstance3D Holding(PrimitiveMesh mesh)
    {
        MeshInstance3D instance = new MeshInstance3D();
        instance.Mesh = mesh;
        return instance;
    }
}
