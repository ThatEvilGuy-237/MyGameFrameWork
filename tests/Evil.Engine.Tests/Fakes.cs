using System.Numerics;
using Evil.Engine.Graphics;
using Evil.Engine.Textures;

namespace Evil.Engine.Tests;

internal sealed class RecordingSink : ITriangleSink
{
    public List<(Texture2D Texture, Vertex[] Vertices)> Batches { get; } = [];

    public Vertex[] Vertices => [.. Batches.SelectMany(batch => batch.Vertices)];

    public void Submit(Texture2D texture, ReadOnlySpan<Vertex> vertices) => Batches.Add((texture, vertices.ToArray()));
}

internal static class Fakes
{
    public static Texture2D Texture(int width, int height, string name = "fake", TextureFilter filter = TextureFilter.Nearest) =>
        new(0, width, height, name, filter, ownsHandle: false);

    public static (Canvas Canvas, RecordingSink Sink, Texture2D White) Canvas()
    {
        var sink = new RecordingSink();
        var white = Texture(1, 1, "white");
        var canvas = new Canvas(sink, white);
        canvas.Begin(new Vector2(800f, 600f));
        return (canvas, sink, white);
    }
}
