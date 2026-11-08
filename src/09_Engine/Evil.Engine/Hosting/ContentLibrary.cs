using Godot;
using EvilFont = Evil.Engine.Screen.Font;
using EvilModel = Evil.Engine.World.Model;
using EvilTexture = Evil.Engine.World.Texture;

namespace Evil.Engine.Hosting;

public sealed class ContentLibrary
{
    private readonly Dictionary<string, EvilTexture> _textures = new Dictionary<string, EvilTexture>(StringComparer.Ordinal);
    private readonly Dictionary<string, EvilFont> _fonts = new Dictionary<string, EvilFont>(StringComparer.Ordinal);
    private readonly Dictionary<string, PackedScene> _scenes = new Dictionary<string, PackedScene>(StringComparer.Ordinal);

    internal ContentLibrary(string root) => Root = root;

    public string Root { get; }

    public EvilModel Model(string path)
    {
        string full = Resolve(path);
        string name = System.IO.Path.GetFileNameWithoutExtension(path);
        if (IsImported(full))
        {
            if (!_scenes.TryGetValue(full, out PackedScene? scene))
            {
                scene = ResourceLoader.Load<PackedScene>(full) ?? throw Unreadable(path, "a model");
                _scenes[full] = scene;
            }

            Node3D imported = scene.Instantiate() as Node3D ?? throw Unreadable(path, "a 3D model");
            return EvilModel.Wrap(imported, name, full);
        }

        RequireFile(path, full);
        GltfDocument document = new GltfDocument();
        GltfState state = new GltfState();
        Error error = document.AppendFromFile(full, state);
        if (error != Error.Ok)
        {
            throw new InvalidDataException($"Content: '{path}' could not be read as a glTF model ({error}).");
        }

        Node3D generated = document.GenerateScene(state) as Node3D ?? throw Unreadable(path, "a 3D model");
        return EvilModel.Wrap(generated, name, full);
    }

    public EvilTexture Texture(string path)
    {
        string full = Resolve(path);
        if (_textures.TryGetValue(full, out EvilTexture? cached))
        {
            return cached;
        }

        Texture2D native;
        if (IsImported(full))
        {
            native = ResourceLoader.Load<Texture2D>(full) ?? throw Unreadable(path, "a texture");
        }
        else
        {
            RequireFile(path, full);
            Image image = Image.LoadFromFile(full) ?? throw Unreadable(path, "an image");
            native = ImageTexture.CreateFromImage(image);
        }

        EvilTexture texture = new EvilTexture(native, path);
        _textures[full] = texture;
        return texture;
    }

    public EvilFont Font(string path)
    {
        string full = Resolve(path);
        if (_fonts.TryGetValue(full, out EvilFont? cached))
        {
            return cached;
        }

        Godot.Font native;
        if (IsImported(full))
        {
            native = ResourceLoader.Load<Godot.Font>(full) ?? throw Unreadable(path, "a font");
        }
        else
        {
            RequireFile(path, full);
            FontFile file = new FontFile();
            Error error = file.LoadDynamicFont(full);
            if (error != Error.Ok)
            {
                throw new InvalidDataException($"Content: '{path}' could not be read as a font ({error}).");
            }

            native = file;
        }

        EvilFont font = new EvilFont(native, path);
        _fonts[full] = font;
        return font;
    }

    internal static bool IsGodotPath(string path) =>
        path.StartsWith("res://", StringComparison.Ordinal) || path.StartsWith("user://", StringComparison.Ordinal);

    private static bool IsImported(string full) => full.StartsWith("res://", StringComparison.Ordinal) && ResourceLoader.Exists(full);

    private static void RequireFile(string path, string full)
    {
        if (!Godot.FileAccess.FileExists(full))
        {
            throw new FileNotFoundException($"Content: '{path}' was not found (looked at {full}).", full);
        }
    }

    private static InvalidDataException Unreadable(string path, string what) =>
        new InvalidDataException($"Content: '{path}' could not be loaded as {what}.");

    private string Resolve(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (IsGodotPath(path) || System.IO.Path.IsPathRooted(path))
        {
            return path;
        }

        return IsGodotPath(Root) ? $"{Root.TrimEnd('/')}/{path}" : System.IO.Path.Combine(Root, path);
    }
}
