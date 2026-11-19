namespace Evil.Engine.GodotTests.Characters;

internal sealed class NodeSearch
{
    private readonly Godot.Node _root;

    public NodeSearch(CharacterModel model) => _root = model.Node;

    public TNode First<TNode>()
        where TNode : Godot.Node => Under<TNode>(_root) ?? throw new CheckFailed($"the character holds no {typeof(TNode).Name}");

    public int Count<TNode>()
        where TNode : Godot.Node => Counted<TNode>(_root);

    public Godot.MeshInstance3D? Mesh(string name) => First<Godot.Skeleton3D>().GetNodeOrNull<Godot.MeshInstance3D>(name);

    public Godot.ShaderMaterial Material(string mesh) =>
        Mesh(mesh)?.GetSurfaceOverrideMaterial(0) as Godot.ShaderMaterial ?? throw new CheckFailed($"'{mesh}' is not painted");

    private static TNode? Under<TNode>(Godot.Node node)
        where TNode : Godot.Node
    {
        if (node is TNode match)
        {
            return match;
        }

        foreach (Godot.Node child in node.GetChildren())
        {
            if (Under<TNode>(child) is TNode found)
            {
                return found;
            }
        }

        return null;
    }

    private static int Counted<TNode>(Godot.Node node)
        where TNode : Godot.Node
    {
        int count = node is TNode ? 1 : 0;
        foreach (Godot.Node child in node.GetChildren())
        {
            count += Counted<TNode>(child);
        }

        return count;
    }
}
