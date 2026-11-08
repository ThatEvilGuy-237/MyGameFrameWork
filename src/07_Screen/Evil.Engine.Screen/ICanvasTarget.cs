using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Screen;

internal interface ICanvasTarget
{
    void SetTransform(Matrix3x2 matrix);

    void Rect(RectF rect, Color color, bool filled, float thickness);

    void Line(Vector2 from, Vector2 to, Color color, float thickness);

    void Circle(Vector2 center, float radius, Color color);

    void Polygon(ReadOnlySpan<Vector2> points, Color color);

    void Text(string text, Vector2 baseline, int size, Color color, TextAlign align, float width, Font? font);

    void Paragraph(string text, Vector2 baseline, int size, Color color, float width, Font? font);

    Vector2 Measure(string text, int size, Font? font);

    Vector2 MeasureParagraph(string text, int size, float width, Font? font);

    float Ascent(int size, Font? font);
}
