using Godot;

namespace Evil.Engine.Characters;

internal sealed class Wardrobe
{
    private readonly Dictionary<string, WornPiece> _worn = new Dictionary<string, WornPiece>(StringComparer.Ordinal);
    private readonly CharacterDescription _told;
    private readonly ICharacterFiles _files;
    private readonly CharacterLook _look;
    private readonly SkinBinder _binder;
    private readonly SkinPatches _patches;
    private readonly BodyShapes _shapes;

    public Wardrobe(CharacterDescription told, ICharacterFiles files, LoadedScene body, CharacterLook look)
    {
        _told = told;
        _files = files;
        _look = look;
        _binder = new SkinBinder(body.Skeleton);
        _patches = new SkinPatches(body.Mesh(told.Skin));
        _shapes = new BodyShapes(body.Meshes);
    }

    public IReadOnlyList<string> Worn
    {
        get
        {
            List<string> names = new List<string>();
            foreach (WornPiece piece in _worn.Values)
            {
                names.Add(piece.Name);
            }

            return names;
        }
    }

    public int SkinTriangles => _patches.Triangles;

    public void Wear(string name)
    {
        if (!_told.Clothing.TryGetValue(name, out ClothingDescription? told))
        {
            throw new ArgumentException($"{_told.Name} has no clothing named '{name}'. It has: {string.Join(", ", _told.Clothing.Keys)}.", nameof(name));
        }

        if (_worn.TryGetValue(told.Slot, out WornPiece? old))
        {
            Drop(old);
        }

        LoadedScene scene = new LoadedScene(_files.Scene(told.Piece.File), _files.Source);
        List<MeshInstance3D> meshes = new List<MeshInstance3D>();
        foreach (PartDescription part in told.Piece.Parts)
        {
            MeshInstance3D mesh = scene.Mesh(part.Name);
            _binder.Bind(mesh, scene.Skeleton);
            _look.Paint(mesh, part);
            meshes.Add(mesh);
        }

        scene.Root.Free();
        _worn[told.Slot] = new WornPiece(name, told, meshes);
        Settle();
    }

    public void TakeOff(string name)
    {
        foreach (WornPiece piece in _worn.Values)
        {
            if (piece.Name == name)
            {
                Drop(piece);
                Settle();
                return;
            }
        }

        throw new ArgumentException($"{_told.Name} is not wearing '{name}'.", nameof(name));
    }

    private void Drop(WornPiece piece)
    {
        foreach (MeshInstance3D mesh in piece.Meshes)
        {
            _look.Forget(mesh);
            mesh.GetParent().RemoveChild(mesh);
            mesh.Free();
        }

        _worn.Remove(piece.Told.Slot);
    }

    private void Settle()
    {
        HashSet<int> hidden = new HashSet<int>();
        Dictionary<string, float> shapes = new Dictionary<string, float>(StringComparer.Ordinal);
        foreach (WornPiece piece in _worn.Values)
        {
            hidden.UnionWith(piece.Told.Hides);
            foreach (KeyValuePair<string, float> shape in piece.Told.Shapes)
            {
                shapes[shape.Key] = shape.Value;
            }
        }

        _patches.Hide(hidden);
        _shapes.Show(shapes);
    }
}
