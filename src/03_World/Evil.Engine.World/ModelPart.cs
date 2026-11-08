using Godot;

namespace Evil.Engine.World;

public sealed class ModelPart : MeshObject
{
    private Material? _material;

    internal ModelPart(MeshInstance3D node)
        : base(node, node.Name.ToString())
    {
    }

    public Material? Material
    {
        get => _material;
        set
        {
            _material = value;
            Instance.MaterialOverride = value?.Native;
        }
    }
}
