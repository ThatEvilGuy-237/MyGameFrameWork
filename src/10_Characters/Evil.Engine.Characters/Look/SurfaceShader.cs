using Godot;

namespace Evil.Engine.Characters;

internal sealed class SurfaceShader : IDisposable
{
    private const string File = "surface.gdshader";

    public SurfaceShader()
    {
        using Stream stream = typeof(SurfaceShader).Assembly.GetManifestResourceStream(File)
            ?? throw new InvalidOperationException($"The engine was built without its shader '{File}'.");
        using StreamReader reader = new StreamReader(stream);
        Native = new Shader();
        Native.Code = reader.ReadToEnd();
    }

    public Shader Native { get; }

    public void Dispose() => Native.Dispose();
}
