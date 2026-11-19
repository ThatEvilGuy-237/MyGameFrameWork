using Godot;

namespace Evil.Engine.Characters;

internal sealed class ToonShaders : IDisposable
{
    public ToonShaders()
    {
        Toon = Load("toon.gdshader");
        Outline = Load("outline.gdshader");
    }

    public Shader Toon { get; }

    public Shader Outline { get; }

    public void Dispose()
    {
        Toon.Dispose();
        Outline.Dispose();
    }

    private static Shader Load(string name)
    {
        using Stream stream = typeof(ToonShaders).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"The engine was built without its shader '{name}'.");
        using StreamReader reader = new StreamReader(stream);
        Shader shader = new Shader();
        shader.Code = reader.ReadToEnd();
        return shader;
    }
}
