using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests;

internal static class ScreenTests
{
    [Test]
    public static void GroupsMoveWhatIsDrawn()
    {
        (Canvas? canvas, RecordingTarget? target) = Drawing();
        using (canvas.Group())
        {
            canvas.Translate(100f, 50f);
            canvas.FillRect(new RectF(0f, 0f, 10f, 10f), Color.Red);
            canvas.FillRect(new RectF(20f, 0f, 10f, 10f), Color.Red);
        }

        canvas.FillRect(new RectF(0f, 0f, 5f, 5f), Color.Blue);
        Check.Equal("transform 0,0 | transform 100,50 | rect | rect | transform 0,0 | rect", target.Script, "sent only when it changes");
    }

    [Test]
    public static void TextInABoxIsCentred()
    {
        (Canvas? canvas, RecordingTarget? target) = Drawing();
        canvas.DrawText("Move", new RectF(100f, 500f, 120f, 40f), 18f, Color.White, TextAlign.Center);
        (Vector2 Baseline, int Size, float Width, TextAlign Align, Font? Font) text = target.Texts.Single();
        Check.Near(new Vector2(100f, 525f), text.Baseline, "baseline: box top + (40 - 20) / 2 + ascent 15");
        Check.Equal(18, text.Size, "size in pixels");
        Check.Near(120f, text.Width, "aligned inside the box width");
        Check.Equal(TextAlign.Center, text.Align, "centred");
    }

    [Test]
    public static void TextAtAPointStartsAtItsTopLeft()
    {
        (Canvas? canvas, RecordingTarget? target) = Drawing();
        canvas.DrawText("hi", new Vector2(10f, 10f), 16f, Color.White);
        Check.Near(new Vector2(10f, 25f), target.Texts.Single().Baseline, "top + ascent");
    }

    [Test]
    public static void EachSceneAndFrameStartsWithTheBuiltInFont()
    {
        (Canvas? canvas, RecordingTarget? target) = Drawing();
        Font font = new Font(new Godot.FontFile(), "test");
        canvas.Font = font;
        canvas.DrawText("styled", new Vector2(0f, 0f), 16f, Color.White);
        canvas.ResetTransform();
        Check.True(canvas.Font is null, "reset between scenes");
        canvas.DrawText("plain", new Vector2(0f, 0f), 16f, Color.White);
        Check.True(ReferenceEquals(font, target.Texts[0].Font) && target.Texts[1].Font is null, "the font reaches the drawing");
        canvas.Font = font;
        canvas.End();
        canvas.Begin(new Vector2(800f, 600f));
        Check.True(canvas.Font is null, "reset every frame");
    }

    [Test]
    public static void TheCanvasOnlyDrawsInsideDraw()
    {
        RecordingTarget target = new RecordingTarget();
        Canvas canvas = new Canvas(target);
        Check.Throws<InvalidOperationException>(() => canvas.FillRect(new RectF(0f, 0f, 1f, 1f), Color.Red), "only draws inside a scene's Draw");
        Check.Near(new Vector2(60f, 20f), canvas.MeasureText("anything", 18f), "measuring works any time");
        canvas.Begin(new Vector2(800f, 600f));
        canvas.Push();
        Check.Throws<InvalidOperationException>(canvas.End, "1 Push() without a matching Pop()");
    }

    [Test]
    public static void ScenesDrawBottomUpFromAClearTransform()
    {
        using Stage stage = Stage.Open();
        List<string> log = new List<string>();
        SceneStack scenes = new SceneStack(null, stage.Root);
        scenes.Push(new DrawingScene("map", log, overlay: false, moveWithoutGroup: true));
        scenes.Push(new DrawingScene("menu", log, overlay: true, moveWithoutGroup: false));
        scenes.ApplyChanges();
        (Canvas? canvas, RecordingTarget? target) = Drawing();
        scenes.Draw(canvas);
        Check.Equal("map menu", string.Join(' ', log), "bottom first");
        Check.True(target.Script.EndsWith("transform 0,0 | rect", StringComparison.Ordinal), $"the menu starts clean, got {target.Script}");

        scenes.Push(new DrawingScene("battle", log, overlay: false, moveWithoutGroup: false));
        scenes.ApplyChanges();
        log.Clear();
        scenes.Draw(canvas);
        Check.Equal("battle", string.Join(' ', log), "a full scene hides the 2D below too");
    }

    [Test]
    public static void ASceneThatForgetsPopIsNamed()
    {
        using Stage stage = Stage.Open();
        SceneStack scenes = new SceneStack(null, stage.Root);
        scenes.Push(new LeakyScene());
        scenes.ApplyChanges();
        (Canvas? canvas, RecordingTarget _) = Drawing();
        Check.Throws<InvalidOperationException>(() => scenes.Draw(canvas), "LeakyScene.Draw left 1 Push()");
    }

    [Test]
    public static void TheRealFontMeasures()
    {
        Godot.Node2D node = new Godot.Node2D();
        GodotCanvasTarget target = new GodotCanvasTarget(node);
        Vector2 hello = target.Measure("Hello", 16, null);
        Vector2 longer = target.Measure("Hello, turn-based world", 16, null);
        Check.True(hello.X > 0f && hello.Y > 0f, $"has a size, got {hello}");
        Check.True(longer.X > hello.X, "longer text is wider");
        Check.True(target.Measure("Hello", 32, null).Y > hello.Y, "bigger text is taller");
        Check.True(target.Ascent(16, null) > 0f, "ascent");
        Vector2 wrapped = target.MeasureParagraph("one two three four five six seven eight nine ten", 16, 80f, null);
        Check.True(wrapped.Y > hello.Y * 1.5f, $"a narrow paragraph wraps onto several lines, got {wrapped}");
        node.Free();
    }

    [Test]
    public static void DisplayChecksWhatItIsGiven()
    {
        Display display = new Display(new WindowSettings("Start"));
        Check.Equal("Start", display.Title, "title from the settings");
        display.Title = "Battle";
        Check.Equal("Battle", display.Title, "title changes");
        Check.Throws<ArgumentException>(() => display.Title = " ");
        Check.Throws<ArgumentOutOfRangeException>(() => display.Resize(0, 600), "positive size");
    }

    private static (Canvas Canvas, RecordingTarget Target) Drawing()
    {
        RecordingTarget target = new RecordingTarget();
        Canvas canvas = new Canvas(target);
        canvas.Begin(new Vector2(800f, 600f));
        return (canvas, target);
    }

    private sealed class DrawingScene(string name, List<string> log, bool overlay, bool moveWithoutGroup) : Scene
    {
        public override bool IsOverlay => overlay;

        protected override void Draw(Canvas canvas)
        {
            log.Add(name);
            if (moveWithoutGroup)
            {
                canvas.Translate(300f, 200f);
            }

            canvas.FillRect(new RectF(0f, 0f, 1f, 1f), Color.White);
        }
    }

    private sealed class LeakyScene : Scene
    {
        protected override void Draw(Canvas canvas) => canvas.Push();
    }

    private sealed class RecordingTarget : ICanvasTarget
    {
        private readonly List<string> _steps = [];

        public List<(Vector2 Baseline, int Size, float Width, TextAlign Align, Font? Font)> Texts { get; } = [];

        public string Script => string.Join(" | ", _steps);

        public void SetTransform(Matrix3x2 matrix) => _steps.Add($"transform {matrix.M31:0},{matrix.M32:0}");

        public void Rect(RectF rect, Color color, bool filled, float thickness) => _steps.Add("rect");

        public void Line(Vector2 from, Vector2 to, Color color, float thickness) => _steps.Add("line");

        public void Circle(Vector2 center, float radius, Color color) => _steps.Add("circle");

        public void Polygon(ReadOnlySpan<Vector2> points, Color color) => _steps.Add("polygon");

        public void Text(string text, Vector2 baseline, int size, Color color, TextAlign align, float width, Font? font)
        {
            _steps.Add("text");
            Texts.Add((baseline, size, width, align, font));
        }

        public void Paragraph(string text, Vector2 baseline, int size, Color color, float width, Font? font) => _steps.Add("paragraph");

        public Vector2 Measure(string text, int size, Font? font) => new Vector2(60f, 20f);

        public Vector2 MeasureParagraph(string text, int size, float width, Font? font) => new Vector2(width, 60f);

        public float Ascent(int size, Font? font) => 15f;
    }
}
