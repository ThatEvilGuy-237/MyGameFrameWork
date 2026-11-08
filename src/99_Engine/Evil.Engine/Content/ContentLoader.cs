using Godot;

namespace Evil.Engine.Hosting;

internal abstract class ContentLoader<TContent, TNative>
    where TContent : class
    where TNative : Resource
{
    private readonly Dictionary<string, TContent> _loaded = new Dictionary<string, TContent>(StringComparer.Ordinal);

    protected abstract string Kind { get; }

    public TContent Load(ContentLocation location)
    {
        if (_loaded.TryGetValue(location.Full, out TContent? cached))
        {
            return cached;
        }

        TNative native = location.IsImported ? LoadImported(location) : LoadFile(location);
        TContent content = Wrap(native, location);
        _loaded[location.Full] = content;
        return content;
    }

    public void Close()
    {
        foreach (TContent content in _loaded.Values)
        {
            NativeOf(content).Dispose();
        }

        _loaded.Clear();
    }

    protected abstract TNative ReadFile(ContentLocation location);

    protected abstract TContent Wrap(TNative native, ContentLocation location);

    protected abstract TNative NativeOf(TContent content);

    private TNative LoadImported(ContentLocation location) =>
        ResourceLoader.Load<TNative>(location.Full) ?? throw location.Unreadable(Kind);

    private TNative LoadFile(ContentLocation location)
    {
        location.RequireFile();
        return ReadFile(location);
    }
}
