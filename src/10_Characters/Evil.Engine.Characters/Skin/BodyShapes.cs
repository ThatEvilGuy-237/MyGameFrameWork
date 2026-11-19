using Godot;

namespace Evil.Engine.Characters;

internal sealed class BodyShapes
{
    private readonly IReadOnlyCollection<MeshInstance3D> _meshes;
    private readonly HashSet<string> _shown = new HashSet<string>(StringComparer.Ordinal);

    public BodyShapes(IReadOnlyCollection<MeshInstance3D> meshes) => _meshes = meshes;

    public void Show(IReadOnlyDictionary<string, float> shapes)
    {
        foreach (string name in _shown)
        {
            if (!shapes.ContainsKey(name))
            {
                Set(name, 0f);
            }
        }

        _shown.Clear();
        foreach (KeyValuePair<string, float> shape in shapes)
        {
            Set(shape.Key, shape.Value);
            _shown.Add(shape.Key);
        }
    }

    private void Set(string name, float amount)
    {
        foreach (MeshInstance3D mesh in _meshes)
        {
            int shape = mesh.FindBlendShapeByName(name);
            if (shape >= 0)
            {
                mesh.SetBlendShapeValue(shape, amount);
            }
        }
    }
}
