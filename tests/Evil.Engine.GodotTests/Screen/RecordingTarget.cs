using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Screen;

internal sealed class RecordingTarget : ICanvasTarget
{
    private readonly List<string> _steps = [];

    public List<DrawnText> Texts { get; } = new List<DrawnText>();

    public DrawnText OnlyText
    {
        get
        {
            Check.Equal(1, Texts.Count, "one text drawn");
            return Texts[0];
        }
    }

    public string Script => string.Join(" | ", _steps);

    public Canvas OpenCanvas()
    {
        Canvas canvas = new Canvas(this);
        canvas.Begin(new Vector2(800f, 600f));
        return canvas;
    }

    public void SetTransform(Matrix3x2 matrix) => _steps.Add($"transform {matrix.M31:0},{matrix.M32:0}");

    public void Rect(RectF rect, Color color, bool filled, float thickness) => _steps.Add("rect");

    public void Line(Vector2 from, Vector2 to, Color color, float thickness) => _steps.Add("line");

    public void Circle(Vector2 center, float radius, Color color) => _steps.Add("circle");

    public void Polygon(ReadOnlySpan<Vector2> points, Color color) => _steps.Add("polygon");

    public void Text(string text, Vector2 baseline, int size, Color color, TextAlign align, float width, Font? font)
    {
        _steps.Add("text");
        Texts.Add(new DrawnText(baseline, size, width, align, font));
    }

    public void Paragraph(string text, Vector2 baseline, int size, Color color, float width, Font? font) => _steps.Add("paragraph");

    public Vector2 Measure(string text, int size, Font? font) => new Vector2(60f, 20f);

    public Vector2 MeasureParagraph(string text, int size, float width, Font? font) => new Vector2(width, 60f);

    public float Ascent(int size, Font? font) => 15f;
}
