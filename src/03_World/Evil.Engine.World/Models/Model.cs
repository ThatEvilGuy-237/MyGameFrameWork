using Godot;

namespace Evil.Engine.World;

public sealed class Model : WorldObject
{
    private Model(Node3D root, string name)
        : base(root, name)
    {
    }

    public string Source { get; private set; } = string.Empty;

    public IReadOnlyList<ModelPart> Parts => [.. Descendants<ModelPart>()];

    public ModelPart? Part(string name)
    {
        foreach (ModelPart part in Descendants<ModelPart>())
        {
            if (part.Name == name)
            {
                return part;
            }
        }

        return null;
    }

    public void Paint(Material? material)
    {
        foreach (ModelPart part in Parts)
        {
            part.Material = material;
        }
    }

    internal static Model Wrap(Node3D root, string name, string source)
    {
        Model model = new Model(root, name);
        model.Source = source;
        root.Name = name;
        AdoptChildren(model, root);
        return model;
    }

    private static void AdoptChildren(WorldObject parent, Node node)
    {
        foreach (Node? child in node.GetChildren())
        {
            WorldObject? wrapped = child switch
            {
                MeshInstance3D mesh => new ModelPart(mesh),
                Node3D group => new WorldObject(group, group.Name.ToString()),
                _ => null,
            };
            if (wrapped is null)
            {
                continue;
            }

            parent.Adopt(wrapped);
            AdoptChildren(wrapped, child);
        }
    }
}
