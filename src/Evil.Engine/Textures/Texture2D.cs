using System.Runtime.InteropServices;
using Evil.Engine.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Evil.Engine.Textures;

public sealed class Texture2D : IDisposable
{
    private readonly bool _ownsHandle;

    internal Texture2D(int handle, int width, int height, string name, TextureFilter filter, bool ownsHandle)
    {
        Handle = handle;
        Width = width;
        Height = height;
        Name = name;
        Filter = filter;
        _ownsHandle = ownsHandle;
        Full = new TextureRegion(this, new RectI(0, 0, width, height));
    }

    public int Width { get; }

    public int Height { get; }

    public string Name { get; }

    public TextureFilter Filter { get; }

    public TextureRegion Full { get; }

    public bool IsDisposed { get; private set; }

    internal int Handle { get; }

    public TextureRegion Region(RectI source)
    {
        if (source.IsEmpty || !new RectI(0, 0, Width, Height).Contains(source))
        {
            throw new ArgumentOutOfRangeException(nameof(source), $"{source} is outside texture '{Name}' ({Width}x{Height}).");
        }

        return new TextureRegion(this, source);
    }

    public TextureRegion Region(int x, int y, int width, int height) => Region(new RectI(x, y, width, height));

    public IReadOnlyList<TextureRegion> Grid(int cellWidth, int cellHeight)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(cellWidth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(cellHeight, 1);
        var cells = new List<TextureRegion>();
        for (var y = 0; y + cellHeight <= Height; y += cellHeight)
        {
            for (var x = 0; x + cellWidth <= Width; x += cellWidth)
            {
                cells.Add(new TextureRegion(this, new RectI(x, y, cellWidth, cellHeight)));
            }
        }

        return cells;
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        if (_ownsHandle)
        {
            GL.DeleteTexture(Handle);
        }
    }

    internal static Texture2D Upload(string name, int width, int height, ReadOnlySpan<byte> rgba, TextureFilter filter)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        if (rgba.Length != width * height * 4)
        {
            throw new ArgumentException($"Texture '{name}' needs {width * height * 4} bytes of RGBA, got {rgba.Length}.", nameof(rgba));
        }

        var handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba8,
            width,
            height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            ref MemoryMarshal.GetReference(rgba));
        var sampling = filter == TextureFilter.Nearest ? (int)TextureMinFilter.Nearest : (int)TextureMinFilter.Linear;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, sampling);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, sampling);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        return new Texture2D(handle, width, height, name, filter, ownsHandle: true);
    }
}
