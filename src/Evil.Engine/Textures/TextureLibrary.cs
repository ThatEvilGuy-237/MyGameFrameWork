namespace Evil.Engine.Textures;

public sealed class TextureLibrary : IDisposable
{
    private readonly Dictionary<string, Texture2D> _files =
        new(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    private readonly List<Texture2D> _created = [];

    internal TextureLibrary(string root, TextureFilter defaultFilter)
    {
        Root = root;
        DefaultFilter = defaultFilter;
    }

    public string Root { get; }

    public TextureFilter DefaultFilter { get; }

    public int Count => _files.Count + _created.Count;

    public Texture2D Load(string path) => Load(path, DefaultFilter);

    public Texture2D Load(string path, TextureFilter filter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(Path.Combine(Root, path));
        var key = $"{fullPath}|{filter}";
        if (_files.TryGetValue(key, out var cached) && !cached.IsDisposed)
        {
            return cached;
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Texture '{path}' was not found under {Root}.", fullPath);
        }

        DecodedImage image;
        using (var stream = File.OpenRead(fullPath))
        {
            image = ImageDecoder.Decode(stream);
        }

        var texture = Texture2D.Upload(path, image.Width, image.Height, image.Pixels, filter);
        _files[key] = texture;
        return texture;
    }

    public Texture2D Create(string name, int width, int height, ReadOnlySpan<byte> rgba) =>
        Create(name, width, height, rgba, DefaultFilter);

    public Texture2D Create(string name, int width, int height, ReadOnlySpan<byte> rgba, TextureFilter filter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var texture = Texture2D.Upload(name, width, height, rgba, filter);
        _created.Add(texture);
        return texture;
    }

    public void Unload(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        foreach (var key in _files.Where(pair => ReferenceEquals(pair.Value, texture)).Select(pair => pair.Key).ToList())
        {
            _files.Remove(key);
        }

        _created.Remove(texture);
        texture.Dispose();
    }

    public void Dispose()
    {
        foreach (var texture in _files.Values.Concat(_created))
        {
            texture.Dispose();
        }

        _files.Clear();
        _created.Clear();
    }
}
