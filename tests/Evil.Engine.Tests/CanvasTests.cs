using System.Numerics;
using Evil.Engine.Graphics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests;

internal static class CanvasTests
{
    [Test]
    public static void FillRectFollowsTheGroup()
    {
        var (canvas, sink, white) = Fakes.Canvas();
        using (canvas.Group())
        {
            canvas.Translate(100f, 50f);
            canvas.FillRect(new RectF(0f, 0f, 10f, 20f), Color.Red);
        }

        var vertices = sink.Vertices;
        Check.Equal(6, vertices.Length, "a rectangle is two triangles");
        Check.True(ReferenceEquals(white, sink.Batches[0].Texture), "plain shapes use the white texture");
        Check.Near(new Vector2(100f, 50f), vertices[0].Position, "top-left");
        Check.Near(new Vector2(110f, 70f), vertices[4].Position, "bottom-right");
        Check.Equal(Color.Red, vertices[0].Color, "color");
    }

    [Test]
    public static void RotationTurnsClockwiseOnScreen()
    {
        var (canvas, sink, _) = Fakes.Canvas();
        canvas.Rotate(90f);
        canvas.FillRect(new RectF(0f, 0f, 10f, 10f), Color.White);
        Check.Near(new Vector2(0f, 10f), sink.Vertices[1].Position, "top-right corner turns down");
    }

    [Test]
    public static void ShapesProduceWholeTriangles()
    {
        var (canvas, sink, _) = Fakes.Canvas();
        canvas.FillPolygon(Polygon.Star(Vector2.Zero, 60f, 26f, 5), Color.Yellow);
        Check.Equal(24, sink.Vertices.Length, "star");
        sink.Batches.Clear();
        canvas.DrawRect(new RectF(0f, 0f, 50f, 50f), Color.White, 2f);
        Check.Equal(24, sink.Vertices.Length, "outline is four bars");
        sink.Batches.Clear();
        canvas.FillCircle(Vector2.Zero, 10f, Color.White, 16);
        Check.Equal(48, sink.Vertices.Length, "circle with 16 slices");
        sink.Batches.Clear();
        canvas.DrawLine(Vector2.Zero, new Vector2(10f, 0f), Color.White, 4f);
        Check.Near(new Vector2(0f, 2f), sink.Vertices[0].Position, "line is thick");
    }

    [Test]
    public static void TextureRegionsSampleTheRightCell()
    {
        var (canvas, sink, _) = Fakes.Canvas();
        var sheet = Fakes.Texture(32, 16, "sheet");
        var second = sheet.Grid(16, 16)[1];
        canvas.Draw(second, new RectF(0f, 0f, 64f, 64f));
        var vertices = sink.Vertices;
        Check.True(ReferenceEquals(sheet, sink.Batches[0].Texture), "drawn with the sheet");
        Check.Near(new Vector2(0.5f, 0f), vertices[0].TexCoord, "top-left uv");
        Check.Near(new Vector2(1f, 1f), vertices[4].TexCoord, "bottom-right uv");
    }

    [Test]
    public static void TexturedPolygonStretchesOverItsBounds()
    {
        var (canvas, sink, _) = Fakes.Canvas();
        var sheet = Fakes.Texture(32, 16, "sheet");
        canvas.FillPolygon(Polygon.Regular(Vector2.Zero, 50f, 6), sheet.Grid(16, 16)[0], Color.White);
        var uvs = sink.Vertices.Select(vertex => vertex.TexCoord).ToList();
        Check.Near(0f, uvs.Min(uv => uv.X), "left edge");
        Check.Near(0.5f, uvs.Max(uv => uv.X), "right edge stays in the first cell");
        Check.Near(0f, uvs.Min(uv => uv.Y), "top edge");
        Check.Near(1f, uvs.Max(uv => uv.Y), "bottom edge");
    }

    [Test]
    public static void EndThrowsOnAMissingPop()
    {
        var (canvas, _, _) = Fakes.Canvas();
        canvas.Push();
        Check.Throws<InvalidOperationException>(canvas.End, "1 Push() without a matching Pop()");
        Check.Equal(0, canvas.Depth, "depth is reset for the next frame");
    }

    [Test]
    public static void DrawingADisposedTextureThrows()
    {
        var (canvas, _, _) = Fakes.Canvas();
        var texture = Fakes.Texture(8, 8);
        texture.Dispose();
        Check.Throws<ObjectDisposedException>(() => canvas.Draw(texture, Vector2.Zero));
    }

    [Test]
    public static void VertexMatchesTheShaderLayout()
    {
        Check.Equal(32, Vertex.SizeInBytes, "bytes per vertex");
    }
}
