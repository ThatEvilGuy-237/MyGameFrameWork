using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Screen;

internal static class ScreenTests
{
    [Test]
    public static void GroupsMoveWhatIsDrawn()
    {
        RecordingTarget target = new RecordingTarget();
        Canvas canvas = target.OpenCanvas();
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
        RecordingTarget target = new RecordingTarget();
        Canvas canvas = target.OpenCanvas();
        canvas.DrawText("Move", new RectF(100f, 500f, 120f, 40f), 18f, Color.White, TextAlign.Center);
        DrawnText text = target.OnlyText;
        Check.Near(new Vector2(100f, 525f), text.Baseline, "baseline: box top + (40 - 20) / 2 + ascent 15");
        Check.Equal(18, text.Size, "size in pixels");
        Check.Near(120f, text.Width, "aligned inside the box width");
        Check.Equal(TextAlign.Center, text.Align, "centred");
    }

    [Test]
    public static void TextAtAPointStartsAtItsTopLeft()
    {
        RecordingTarget target = new RecordingTarget();
        Canvas canvas = target.OpenCanvas();
        canvas.DrawText("hi", new Vector2(10f, 10f), 16f, Color.White);
        Check.Near(new Vector2(10f, 25f), target.OnlyText.Baseline, "top + ascent");
    }

    [Test]
    public static void EachSceneAndFrameStartsWithTheBuiltInFont()
    {
        RecordingTarget target = new RecordingTarget();
        Canvas canvas = target.OpenCanvas();
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
        void DrawOutsideDraw() => canvas.FillRect(new RectF(0f, 0f, 1f, 1f), Color.Red);
        Check.Throws<InvalidOperationException>(DrawOutsideDraw, "only draws inside a scene's Draw");
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
        RecordingTarget target = new RecordingTarget();
        Canvas canvas = target.OpenCanvas();
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
        Canvas canvas = new RecordingTarget().OpenCanvas();
        void DrawWithALeakyScene() => scenes.Draw(canvas);
        Check.Throws<InvalidOperationException>(DrawWithALeakyScene, "LeakyScene.Draw left 1 Push()");
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
        void BlankTheTitle() => display.Title = " ";
        Check.Throws<ArgumentException>(BlankTheTitle);
        void ResizeToNothing() => display.Resize(0, 600);
        Check.Throws<ArgumentOutOfRangeException>(ResizeToNothing, "positive size");
    }
}
