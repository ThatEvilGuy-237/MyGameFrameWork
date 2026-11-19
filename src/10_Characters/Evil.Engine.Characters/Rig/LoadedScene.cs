using Godot;

namespace Evil.Engine.Characters;

internal sealed class LoadedScene
{
    private readonly Dictionary<string, MeshInstance3D> _meshes = new Dictionary<string, MeshInstance3D>(StringComparer.Ordinal);
    private readonly string _source;
    private Skeleton3D? _skeleton;

    public LoadedScene(Node3D root, string source)
    {
        Root = root;
        _source = source;
        Collect(root);
    }

    public Node3D Root { get; }

    public AnimationPlayer? Player { get; private set; }

    public Skeleton3D Skeleton => _skeleton ?? throw new InvalidDataException($"Character: a model file of {_source} has no rig.");

    public IReadOnlyCollection<MeshInstance3D> Meshes => _meshes.Values;

    public MeshInstance3D Mesh(string name) =>
        _meshes.TryGetValue(name, out MeshInstance3D? mesh) ? mesh : throw new InvalidDataException($"Character: {_source} describes the part '{name}', but its model file has no such mesh.");

    private void Collect(Node node)
    {
        if (node is MeshInstance3D mesh)
        {
            _meshes[mesh.Name.ToString()] = mesh;
        }

        _skeleton ??= node as Skeleton3D;
        Player ??= node as AnimationPlayer;
        foreach (Node child in node.GetChildren())
        {
            Collect(child);
        }
    }
}
