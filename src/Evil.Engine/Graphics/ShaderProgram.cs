using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace Evil.Engine.Graphics;

internal sealed class ShaderProgram : IDisposable
{
    private readonly int _handle;
    private readonly Dictionary<string, int> _uniforms = new(StringComparer.Ordinal);
    private bool _disposed;

    public ShaderProgram(string vertexSource, string fragmentSource)
    {
        var vertex = Compile(ShaderType.VertexShader, vertexSource);
        var fragment = Compile(ShaderType.FragmentShader, fragmentSource);
        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertex);
        GL.AttachShader(_handle, fragment);
        GL.LinkProgram(_handle);
        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out var linked);
        GL.DetachShader(_handle, vertex);
        GL.DetachShader(_handle, fragment);
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
        if (linked == 0)
        {
            var log = GL.GetProgramInfoLog(_handle);
            GL.DeleteProgram(_handle);
            throw new InvalidOperationException($"Shader program did not link: {log}");
        }

        GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out var count);
        for (var i = 0; i < count; i++)
        {
            GL.GetActiveUniform(_handle, i, 256, out _, out _, out _, out string name);
            _uniforms[name] = GL.GetUniformLocation(_handle, name);
        }
    }

    public static ShaderProgram FromEmbedded(string vertexName, string fragmentName) =>
        new(ReadEmbedded(vertexName), ReadEmbedded(fragmentName));

    public void Use() => GL.UseProgram(_handle);

    public void Set(string name, Vector2 value) => GL.Uniform2(Location(name), value.X, value.Y);

    public void Set(string name, int value) => GL.Uniform1(Location(name), value);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        GL.DeleteProgram(_handle);
    }

    private int Location(string name) =>
        _uniforms.TryGetValue(name, out var location)
            ? location
            : throw new InvalidOperationException($"The shader has no uniform '{name}'.");

    private static int Compile(ShaderType type, string source)
    {
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var compiled);
        if (compiled != 0)
        {
            return shader;
        }

        var log = GL.GetShaderInfoLog(shader);
        GL.DeleteShader(shader);
        throw new InvalidOperationException($"{type} did not compile: {log}");
    }

    private static string ReadEmbedded(string name)
    {
        var resource = $"Evil.Engine.Shaders.{name}";
        using var stream = typeof(ShaderProgram).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Embedded shader '{resource}' is missing from Evil.Engine.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
