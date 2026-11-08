using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Textures;

public sealed class TextureRegion
{
    internal TextureRegion(Texture2D texture, RectI source)
    {
        Texture = texture;
        Source = source;
        UvMin = new Vector2(source.Left / (float)texture.Width, source.Top / (float)texture.Height);
        UvMax = new Vector2(source.Right / (float)texture.Width, source.Bottom / (float)texture.Height);
    }

    public Texture2D Texture { get; }

    public RectI Source { get; }

    public int Width => Source.Width;

    public int Height => Source.Height;

    public Vector2 Size => new(Source.Width, Source.Height);

    internal Vector2 UvMin { get; }

    internal Vector2 UvMax { get; }

    public TextureRegion Region(RectI inner)
    {
        var placed = inner.Offset(Source.X, Source.Y);
        if (inner.IsEmpty || !Source.Contains(placed))
        {
            throw new ArgumentOutOfRangeException(nameof(inner), $"{inner} is outside this {Width}x{Height} region of '{Texture.Name}'.");
        }

        return new TextureRegion(Texture, placed);
    }
}
