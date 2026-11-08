using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Screen;

public sealed class Canvas
{
    private readonly ICanvasTarget _target;
    private readonly TransformStack _transforms = new TransformStack();
    private Matrix3x2 _sent = Matrix3x2.Identity;
    private bool _drawing;

    internal Canvas(ICanvasTarget target) => _target = target;

    public Vector2 Size { get; private set; }

    public RectF Bounds => new RectF(0f, 0f, Size.X, Size.Y);

    public int Depth => _transforms.Depth;

    public Matrix3x2 Transform => _transforms.Current;

    public Font? Font { get; set; }

    public void Push() => _transforms.Push();

    public void Pop() => _transforms.Pop();

    public TransformScope Group() => _transforms.Group();

    public void Translate(float x, float y) => _transforms.Translate(x, y);

    public void Translate(Vector2 offset) => _transforms.Translate(offset);

    public void Rotate(float degrees) => _transforms.Rotate(degrees);

    public void Scale(float factor) => _transforms.Scale(factor);

    public void Scale(float x, float y) => _transforms.Scale(x, y);

    public void Apply(Transform2D transform) => _transforms.Apply(transform);

    public Vector2 ToWorld(Vector2 local) => _transforms.ToWorld(local);

    public Vector2 ToLocal(Vector2 screen) => _transforms.ToLocal(screen);

    public void FillRect(RectF rect, Color color) => Ready().Rect(rect, color, true, 0f);

    public void DrawRect(RectF rect, Color color, float thickness = 1f)
    {
        if (thickness > 0f)
        {
            Ready().Rect(rect, color, false, thickness);
        }
    }

    public void DrawLine(Vector2 from, Vector2 to, Color color, float thickness = 1f)
    {
        if (thickness > 0f)
        {
            Ready().Line(from, to, color, thickness);
        }
    }

    public void FillCircle(Vector2 center, float radius, Color color)
    {
        if (radius > 0f)
        {
            Ready().Circle(center, radius, color);
        }
    }

    public void FillTriangle(Vector2 a, Vector2 b, Vector2 c, Color color) => Ready().Polygon([a, b, c], color);

    public void FillPolygon(Polygon polygon, Color color)
    {
        ArgumentNullException.ThrowIfNull(polygon);
        if (polygon.Count >= 3)
        {
            Ready().Polygon(polygon.Points.ToArray(), color);
        }
    }

    public void DrawText(string text, Vector2 topLeft, float size, Color color)
    {
        ICanvasTarget target = Ready();
        int pixels = Pixels(size);
        target.Text(text, topLeft + new Vector2(0f, target.Ascent(pixels, Font)), pixels, color, TextAlign.Left, -1f, Font);
    }

    public void DrawText(string text, RectF box, float size, Color color, TextAlign align)
    {
        ICanvasTarget target = Ready();
        int pixels = Pixels(size);
        float height = target.Measure(text, pixels, Font).Y;
        float top = box.Y + (box.Height - height) / 2f;
        target.Text(text, new Vector2(box.X, top + target.Ascent(pixels, Font)), pixels, color, align, box.Width, Font);
    }

    public void DrawParagraph(string text, RectF box, float size, Color color)
    {
        ICanvasTarget target = Ready();
        int pixels = Pixels(size);
        target.Paragraph(text, new Vector2(box.X, box.Y + target.Ascent(pixels, Font)), pixels, color, box.Width, Font);
    }

    public Vector2 MeasureText(string text, float size, Font? font = null) => _target.Measure(text, Pixels(size), font);

    public Vector2 MeasureParagraph(string text, float size, float width, Font? font = null) =>
        _target.MeasureParagraph(text, Pixels(size), width, font);

    internal void Begin(Vector2 size)
    {
        Size = size;
        Font = null;
        _transforms.Reset();
        _sent = Matrix3x2.Identity;
        _target.SetTransform(_sent);
        _drawing = true;
    }

    internal void ResetTransform()
    {
        _transforms.Reset();
        Font = null;
    }

    internal void End()
    {
        _drawing = false;
        int depth = _transforms.Depth;
        _transforms.Reset();
        if (depth != 0)
        {
            throw new InvalidOperationException($"{depth} Push() without a matching Pop() in this frame.");
        }
    }

    private static int Pixels(float size) => Math.Max(1, (int)MathF.Round(size));

    private ICanvasTarget Ready()
    {
        if (!_drawing)
        {
            throw new InvalidOperationException("The canvas only draws inside a scene's Draw(Canvas).");
        }

        Matrix3x2 current = _transforms.Current;
        if (current != _sent)
        {
            _target.SetTransform(current);
            _sent = current;
        }

        return _target;
    }
}
