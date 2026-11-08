using System.Numerics;
using Evil.Engine.Graphics;
using Evil.Engine.Textures;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests;

internal static class TextureTests
{
    [Test]
    public static void GridCutsASpriteSheetRowByRow()
    {
        var sheet = Fakes.Texture(48, 32, "sheet");
        var cells = sheet.Grid(16, 16);
        Check.Equal(6, cells.Count, "cells");
        Check.Equal(new RectI(32, 0, 16, 16), cells[2].Source, "end of the first row");
        Check.Equal(new RectI(0, 16, 16, 16), cells[3].Source, "start of the second row");
    }

    [Test]
    public static void RegionOutsideTheTextureThrows()
    {
        var sheet = Fakes.Texture(32, 16, "sheet");
        Check.Throws<ArgumentOutOfRangeException>(() => sheet.Region(20, 0, 16, 16), "outside texture 'sheet'");
        Check.Throws<ArgumentOutOfRangeException>(() => sheet.Region(0, 0, 0, 16));
    }

    [Test]
    public static void RegionOfARegionIsRelative()
    {
        var sheet = Fakes.Texture(64, 64, "sheet");
        var quarter = sheet.Region(32, 32, 32, 32);
        Check.Equal(new RectI(40, 36, 8, 8), quarter.Region(new RectI(8, 4, 8, 8)).Source, "offset inside the parent");
        Check.Throws<ArgumentOutOfRangeException>(() => quarter.Region(new RectI(30, 0, 8, 8)));
    }

    [Test]
    public static void SmoothSheetsStayInsideTheirCell()
    {
        var smooth = Fakes.Texture(32, 16, "smooth", TextureFilter.Linear);
        var canvasAndSink = Fakes.Canvas();
        canvasAndSink.Canvas.Draw(smooth.Grid(16, 16)[1], new RectF(0f, 0f, 32f, 32f));
        var vertices = canvasAndSink.Sink.Vertices;
        Check.Near(new Vector2(16.5f / 32f, 0.5f / 16f), vertices[0].TexCoord, "top-left half a texel in");
        Check.Near(new Vector2(31.5f / 32f, 15.5f / 16f), vertices[4].TexCoord, "bottom-right half a texel in");
        canvasAndSink.Sink.Batches.Clear();
        canvasAndSink.Canvas.Draw(smooth, new RectF(0f, 0f, 32f, 16f));
        Check.Near(Vector2.Zero, canvasAndSink.Sink.Vertices[0].TexCoord, "the whole texture keeps its edges");
    }

    [Test]
    public static void ShadersAreEmbedded()
    {
        var names = typeof(Canvas).Assembly.GetManifestResourceNames();
        Check.True(names.Contains("Evil.Engine.Shaders.sprite.vert.glsl"), "vertex shader");
        Check.True(names.Contains("Evil.Engine.Shaders.sprite.frag.glsl"), "fragment shader");
    }

    [Test]
    public static void ColorFromHexAndBack()
    {
        var orange = Color.FromHex("#FF8000");
        Check.Near(1f, orange.R, "red");
        Check.Near(128f / 255f, orange.G, "green");
        Check.Near(1f, orange.A, "alpha defaults to opaque");
        Check.Equal("#FF8000FF", orange.ToHex(), "hex");
        Check.Near(0x44 / 255f, Color.FromHex("11223344").A, "alpha from hex");
        Check.Throws<FormatException>(() => Color.FromHex("#12345"), "#RRGGBB");
    }
}
