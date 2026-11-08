using Evil.Engine.Mathematics;
using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;
using Matrix3x2 = System.Numerics.Matrix3x2;
using Vector2 = System.Numerics.Vector2;

namespace Evil.Engine.Screen;

internal sealed class GodotCanvasTarget(CanvasItem item) : ICanvasTarget
{
    private static Godot.Font Pick(Font? font) => font?.Native ?? ThemeDB.FallbackFont;

    public void SetTransform(Matrix3x2 matrix) =>
        item.DrawSetTransformMatrix(new Godot.Transform2D(
            new Godot.Vector2(matrix.M11, matrix.M12),
            new Godot.Vector2(matrix.M21, matrix.M22),
            new Godot.Vector2(matrix.M31, matrix.M32)));

    public void Rect(RectF rect, Color color, bool filled, float thickness) =>
        item.DrawRect(new Rect2(rect.X, rect.Y, rect.Width, rect.Height), color.ToGodot(), filled, filled ? -1f : thickness, true);

    public void Line(Vector2 from, Vector2 to, Color color, float thickness) =>
        item.DrawLine(from.ToGodot(), to.ToGodot(), color.ToGodot(), thickness, true);

    public void Circle(Vector2 center, float radius, Color color) =>
        item.DrawCircle(center.ToGodot(), radius, color.ToGodot());

    public void Polygon(ReadOnlySpan<Vector2> points, Color color)
    {
        var converted = new Godot.Vector2[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            converted[i] = points[i].ToGodot();
        }

        item.DrawColoredPolygon(converted, color.ToGodot());
    }

    public void Text(string text, Vector2 baseline, int size, Color color, TextAlign align, float width, Font? font) =>
        item.DrawString(Pick(font), baseline.ToGodot(), text, ToGodot(align), width, size, color.ToGodot());

    public void Paragraph(string text, Vector2 baseline, int size, Color color, float width, Font? font) =>
        item.DrawMultilineString(Pick(font), baseline.ToGodot(), text, HorizontalAlignment.Left, width, size, -1, color.ToGodot());

    public Vector2 Measure(string text, int size, Font? font) =>
        Pick(font).GetStringSize(text, HorizontalAlignment.Left, -1f, size).ToEvil();

    public Vector2 MeasureParagraph(string text, int size, float width, Font? font) =>
        Pick(font).GetMultilineStringSize(text, HorizontalAlignment.Left, width, size).ToEvil();

    public float Ascent(int size, Font? font) => Pick(font).GetAscent(size);

    private static HorizontalAlignment ToGodot(TextAlign align) => align switch
    {
        TextAlign.Center => HorizontalAlignment.Center,
        TextAlign.Right => HorizontalAlignment.Right,
        _ => HorizontalAlignment.Left,
    };
}
