namespace Evil.Engine.Sample;

internal sealed class OrbitScene : Scene
{
    private static readonly Color Orange = Color.FromHex("#FF9F1C");

    private readonly Polygon _star = Polygon.Star(Vector2.Zero, 60f, 26f, 5);
    private readonly Polygon _hexagon = Polygon.Regular(Vector2.Zero, 56f, 6);
    private IReadOnlyList<TextureRegion> _gemFrames = [];
    private float _angle;
    private float _speed = 100f;
    private bool _spinning = true;
    private bool _spinningBeforePause = true;
    private bool _starHovered;
    private int _screenshots;

    protected override void Enter()
    {
        _gemFrames = Game.Textures.Load("gem.png").Grid(16, 16);
    }

    protected override void Update(GameTime time)
    {
        var keyboard = Game.Keyboard;
        if (keyboard.WasPressed(Key.Escape))
        {
            Game.Exit();
        }

        if (keyboard.WasPressed(Key.Space))
        {
            _spinning = !_spinning;
        }

        if (keyboard.WasPressed(Key.Up))
        {
            _speed += 50f;
        }

        if (keyboard.WasPressed(Key.Down))
        {
            _speed = MathF.Max(0f, _speed - 50f);
        }

        if (keyboard.WasPressed(Key.P))
        {
            Game.Scenes.Push(new PauseScene());
        }

        if (keyboard.WasPressed(Key.F12))
        {
            _screenshots++;
            Game.CaptureFrame(Path.Combine(AppContext.BaseDirectory, "screenshots", $"orbit-{_screenshots:D3}.png"));
        }

        if (_spinning)
        {
            _angle = Angle.Wrap(_angle + _speed * time.DeltaSeconds);
        }
    }

    protected override void Draw(Canvas canvas)
    {
        using (canvas.Group())
        {
            canvas.Translate(canvas.Size / 2f);
            DrawRed(canvas);
            DrawGreen(canvas);
            DrawBlueSystem(canvas);
        }

        DrawSprites(canvas);
        DrawCursor(canvas);
    }

    protected override void Pause()
    {
        _spinningBeforePause = _spinning;
        _spinning = false;
    }

    protected override void Resume() => _spinning = _spinningBeforePause;

    private void DrawRed(Canvas canvas)
    {
        using var red = canvas.Group();
        canvas.Translate(-200f, -200f);
        canvas.Rotate(_angle);
        canvas.FillRect(new RectF(-50f, -50f, 100f, 100f), Color.Red);
    }

    private void DrawGreen(Canvas canvas)
    {
        using var green = canvas.Group();
        canvas.Translate(200f, -200f);
        canvas.Rotate(-_angle * 2f);
        canvas.FillRect(new RectF(-50f, -50f, 100f, 200f), Color.Green);
    }

    private void DrawBlueSystem(Canvas canvas)
    {
        using var blue = canvas.Group();
        canvas.Translate(0f, 100f);
        canvas.Rotate(-_angle);
        canvas.FillRect(new RectF(-25f, -25f, 50f, 50f), Color.Blue);

        using var yellow = canvas.Group();
        canvas.Translate(0f, 150f);
        canvas.Rotate(_angle);
        _starHovered = _star.Contains(canvas.ToLocal(Game.Mouse.Position));
        canvas.FillPolygon(_star, _starHovered ? Orange : Color.Yellow);

        using var moon = canvas.Group();
        canvas.Rotate(_angle * 3f);
        canvas.Translate(90f, 0f);
        canvas.FillCircle(Vector2.Zero, 14f, Color.White);
        canvas.DrawLine(Vector2.Zero, new Vector2(-90f, 0f), Color.White.WithAlpha(0.35f), 2f);
    }

    private void DrawSprites(Canvas canvas)
    {
        var frame = _gemFrames[(int)(Game.Time.TotalSeconds * 2.5) % _gemFrames.Count];
        canvas.Draw(frame, new RectF(24f, canvas.Size.Y - 88f, 64f, 64f));

        using var tile = canvas.Group();
        canvas.Translate(canvas.Size.X - 96f, canvas.Size.Y - 96f);
        canvas.Rotate(_angle / 2f);
        canvas.FillPolygon(_hexagon, _gemFrames[0], Color.White);
        canvas.DrawRect(new RectF(-60f, -60f, 120f, 120f), Color.White.WithAlpha(0.5f), 2f);
    }

    private void DrawCursor(Canvas canvas)
    {
        var mouse = Game.Mouse.Position;
        var color = _starHovered ? Orange : Color.White;
        canvas.DrawLine(mouse - new Vector2(10f, 0f), mouse + new Vector2(10f, 0f), color, 2f);
        canvas.DrawLine(mouse - new Vector2(0f, 10f), mouse + new Vector2(0f, 10f), color, 2f);
    }
}
