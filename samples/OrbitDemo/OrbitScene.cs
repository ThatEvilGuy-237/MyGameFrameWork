namespace OrbitDemo;

internal sealed class OrbitScene(SampleOptions options) : Scene
{
    private static readonly string[] Actions = ["Move", "Attack", "Wait"];

    private readonly List<Capsule> _pawns = [];
    private Vector2 _loggedScreen;
    private string? _lastAction;
    private int _hoveredAction = -1;
    private Box? _red;
    private Box? _green;
    private Box? _blue;
    private Sphere? _yellow;
    private OrbitCamera? _view;
    private Spotlight? _spot;
    private Torus? _ring;
    private Capsule? _hovered;
    private Capsule? _selected;
    private bool _spinning = true;

    protected override void Enter()
    {
        Camera.FieldOfView = 50f;
        _view = new OrbitCamera(Camera, new OrbitLimits(MinDistance: 6f, MaxDistance: 30f))
        {
            Target = new Vector3(0f, 0f, 2.2f),
            Distance = 13f,
            Pitch = 32f,
            Yaw = -15f,
        };
        _view.Snap();

        Atmosphere = Atmosphere.Dusk();
        World.Add(Sun.Golden());
        World.Add(new Floor(new Vector2(24f, 24f), Color.FromHex("#3C5A5A")));

        _red = World.Add(new Box(new Vector3(1.4f, 1.4f, 1.4f), Color.FromHex("#E5484D")));
        _red.Position = new Vector3(-5f, 0.7f, -2f);

        _green = World.Add(new Box(new Vector3(1.2f, 2.4f, 1.2f), Color.FromHex("#30A46C")));
        _green.Position = new Vector3(5f, 1.2f, -2f);

        _blue = World.Add(new Box(new Vector3(1f, 1f, 1f), Color.FromHex("#3E63DD")));
        _blue.Position = new Vector3(0f, 1f, 1f);

        _yellow = _blue.Add(new Sphere(0.55f, Color.FromHex("#FFC53D")));
        _yellow.Position = new Vector3(3f, 0f, 0f);

        var ring = _yellow.Add(new Torus(0.8f, 0.95f, Color.FromHex("#E8D9A8")));
        ring.Rotation = new Vector3(20f, 0f, 10f);

        var moon = _yellow.Add(new Sphere(0.22f, Color.White) { Material = Material.Glowing(Color.FromHex("#DDEBFF"), 1.5f) });
        moon.Position = new Vector3(1.4f, 0f, 0f);

        var stone = Material.Matte(Color.FromHex("#8C8A80"));
        foreach (var x in new[] { -7f, 7f })
        {
            var pillar = World.Add(new Cylinder(0.45f, 3f, Color.White) { Material = stone });
            pillar.Position = new Vector3(x, 1.5f, -5f);
            var flame = pillar.Add(new Sphere(0.18f, Color.White) { Material = Material.Glowing(Color.FromHex("#FFB45E"), 2.5f), CastsShadow = false });
            flame.Position = new Vector3(0f, 1.75f, 0f);
            flame.Add(Lamp.Warm(7f));
        }

        AddPawn("pawn-red", new Vector3(-2.5f, 0f, 3.5f), Color.FromHex("#E5484D"));
        AddPawn("pawn-blue", new Vector3(2.5f, 0f, 3.5f), Color.FromHex("#3E63DD"));

        _spot = World.Add(new Spotlight(Color.FromHex("#CFE3FF"), 14f, 24f) { Shadows = true, Intensity = 2f });
        _spot.Position = new Vector3(0f, 9f, 3.5f);

        _ring = World.Add(new Torus(0.5f, 0.65f, Color.White)
        {
            Material = Material.Glowing(Color.FromHex("#FFC53D"), 1.5f),
            CastsShadow = false,
            Pickable = false,
            Visible = false,
        });
    }

    protected override void Update(GameTime time)
    {
        var keys = Game.Keyboard;
        var mouse = Game.Mouse;
        var step = time.DeltaSeconds;

        if (keys.WasPressed(Key.Escape))
        {
            Game.Exit();
        }

        if (keys.WasPressed(Key.F12) && options.CapturePath is not null)
        {
            Game.CaptureFrame(options.CapturePath);
            Game.Exit();
        }

        if (keys.WasPressed(Key.Space))
        {
            _spinning = !_spinning;
            Console.WriteLine($"spinning: {_spinning}");
        }

        if (_spinning)
        {
            _red!.RotateY(60f * step);
            _green!.RotateY(-90f * step);
            _blue!.RotateY(-50f * step);
            _yellow!.RotateY(160f * step);
            _view!.Turn(12f * step);
        }

        if (keys.WasPressed(Key.F11))
        {
            Game.Display.Fullscreen = !Game.Display.Fullscreen;
        }

        if (keys.WasPressed(Key.F10))
        {
            Game.Display.Resize(1024, 640);
        }

        SteerCamera(keys, mouse, step);
        LogScreen();
        _hoveredAction = ActionUnder(mouse.Position);
        if (_hoveredAction >= 0)
        {
            ClearHover();
            if (mouse.WasPressed(MouseButton.Left))
            {
                _lastAction = Actions[_hoveredAction];
                Console.WriteLine($"action: {_lastAction}");
            }

            return;
        }

        Hover(mouse);
        if (mouse.WasPressed(MouseButton.Left) && _hovered is not null)
        {
            Select(_hovered);
        }
    }

    protected override void Draw(Canvas canvas)
    {
        DrawPanel(canvas);
        DrawCompass(canvas);
        if (_selected is not null)
        {
            DrawActions(canvas);
        }
    }

    private void DrawPanel(Canvas canvas)
    {
        var panel = new RectF(16f, 16f, 380f, 112f);
        canvas.FillRect(panel, Color.Black.WithAlpha(0.45f));
        canvas.DrawRect(panel, Color.White.WithAlpha(0.25f));
        canvas.DrawText("Evil Engine orbit demo", new Vector2(30f, 26f), 22f, Color.White);
        var status = _selected is null ? "Click a pawn to select it" : $"Selected: {_selected.Tag}" + (_lastAction is null ? string.Empty : $"   Last action: {_lastAction}");
        canvas.DrawText(status, new Vector2(30f, 60f), 16f, Color.FromHex("#FFC53D"));
        canvas.DrawText("Space pause   Q/E turn   wheel zoom   F10 small   F11 full screen", new Vector2(30f, 92f), 13f, Color.White.WithAlpha(0.7f));
    }

    private void DrawCompass(Canvas canvas)
    {
        using var compass = canvas.Group();
        canvas.Translate(canvas.Size.X - 70f, 70f);
        canvas.FillCircle(Vector2.Zero, 46f, Color.Black.WithAlpha(0.45f));
        canvas.Rotate(_view!.Yaw);
        canvas.FillTriangle(new Vector2(0f, -32f), new Vector2(-9f, 0f), new Vector2(9f, 0f), Color.FromHex("#E5484D"));
        canvas.FillTriangle(new Vector2(0f, 32f), new Vector2(-9f, 0f), new Vector2(9f, 0f), Color.White.WithAlpha(0.8f));
        canvas.DrawText("N", new RectF(-10f, -52f, 20f, 16f), 13f, Color.White, TextAlign.Center);
    }

    private void DrawActions(Canvas canvas)
    {
        var buttons = ActionRects(canvas.Size);
        for (var i = 0; i < buttons.Length; i++)
        {
            var hovered = i == _hoveredAction;
            canvas.FillRect(buttons[i], hovered ? Color.FromHex("#3E63DD") : Color.Black.WithAlpha(0.55f));
            canvas.DrawRect(buttons[i], Color.White.WithAlpha(hovered ? 0.9f : 0.35f), 2f);
            canvas.DrawText(Actions[i], buttons[i], 18f, Color.White, TextAlign.Center);
        }
    }

    private static RectF[] ActionRects(Vector2 screen)
    {
        const float width = 150f;
        const float height = 46f;
        const float gap = 16f;
        var left = (screen.X - (width * Actions.Length + gap * (Actions.Length - 1))) / 2f;
        var top = screen.Y - height - 32f;
        return [.. Enumerable.Range(0, Actions.Length).Select(i => new RectF(left + i * (width + gap), top, width, height))];
    }

    private int ActionUnder(Vector2 point)
    {
        if (_selected is null)
        {
            return -1;
        }

        var buttons = ActionRects(Game.ScreenSize);
        return Array.FindIndex(buttons, button => button.Contains(point));
    }

    private void LogScreen()
    {
        var screen = Game.ScreenSize;
        if (screen == _loggedScreen || _selected is null)
        {
            return;
        }

        _loggedScreen = screen;
        var buttons = ActionRects(screen);
        var centres = string.Join(' ', Actions.Select((name, i) => $"{name} {(int)buttons[i].Center.X},{(int)buttons[i].Center.Y}"));
        Console.WriteLine($"screen: {(int)screen.X}x{(int)screen.Y}");
        Console.WriteLine($"buttons: {centres}");
    }

    private void ClearHover()
    {
        if (_hovered is not null)
        {
            _hovered.Material.Glow = Color.Black;
            _hovered = null;
        }
    }

    private void SteerCamera(Keyboard keys, Mouse mouse, float step)
    {
        var view = _view!;
        if (keys.IsDown(Key.Q))
        {
            view.Turn(-60f * step);
        }

        if (keys.IsDown(Key.E))
        {
            view.Turn(60f * step);
        }

        if (mouse.Scroll != 0f)
        {
            view.Zoom(MathF.Pow(0.85f, mouse.Scroll));
            Console.WriteLine($"zoom: {view.Distance:F1}");
        }

        if (mouse.IsDown(MouseButton.Right))
        {
            view.Turn(-mouse.Delta.X * 0.3f);
            view.Tilt(mouse.Delta.Y * 0.2f);
        }

        view.Update(step);
    }

    private void Hover(Mouse mouse)
    {
        var pawn = PawnOf(Pick(mouse.Position)?.Target);
        if (ReferenceEquals(pawn, _hovered))
        {
            return;
        }

        ClearHover();
        _hovered = pawn;
        if (pawn is not null)
        {
            pawn.Material.Glow = pawn.Color;
            pawn.Material.GlowStrength = 0.35f;
        }

        Console.WriteLine($"hover: {pawn?.Tag ?? "nothing"} at {(int)mouse.Position.X},{(int)mouse.Position.Y}");
    }

    private void Select(Capsule pawn)
    {
        _selected = pawn;
        var feet = pawn.WorldPosition with { Y = 0.05f };
        _ring!.Position = feet;
        _ring.Visible = true;
        _spot!.Position = feet + new Vector3(0f, 9f, 0f);
        Console.WriteLine($"selected: {pawn.Tag}");
    }

    private Capsule? PawnOf(WorldObject? target)
    {
        for (var current = target; current is not null; current = current.Parent)
        {
            if (current is Capsule pawn && _pawns.Contains(pawn))
            {
                return pawn;
            }
        }

        return null;
    }

    private void AddPawn(string name, Vector3 place, Color team)
    {
        var pawn = World.Add(new Capsule(0.35f, 1.4f, team) { Name = name, Tag = name });
        pawn.Position = place + new Vector3(0f, 0.7f, 0f);
        var hat = pawn.Add(new Cone(0.3f, 0.45f, Color.FromHex("#F0F0F0")));
        hat.Position = new Vector3(0f, 0.9f, 0f);
        _pawns.Add(pawn);
    }
}
