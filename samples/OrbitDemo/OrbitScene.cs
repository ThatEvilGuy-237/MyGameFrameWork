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
    private Font? _titleFont;
    private Tween? _busy;
    private string? _whenDone;

    protected override void Enter()
    {
        Camera.FieldOfView = 50f;
        _view = new OrbitCamera(Camera, new OrbitLimits(MinDistance: 6f, MaxDistance: 30f));
        _view.Target = new Vector3(0f, 0f, 2.2f);
        _view.Distance = 13f;
        _view.Pitch = 32f;
        _view.Yaw = -15f;
        _view.Snap();

        Atmosphere = Atmosphere.Dusk();
        World.Add(Sun.Golden());
        Floor floor = World.Add(new Floor(new Vector2(24f, 24f), Color.White));
        floor.Material.Texture = Game.Content.Texture("textures/tiles.png");
        floor.Material.TextureScale = new Vector2(12f, 12f);

        Model watchtower = World.Add(Game.Content.Model("models/tower.glb"));
        watchtower.Position = new Vector3(-5.5f, 0f, 3.2f);
        Model outpost = World.Add(Game.Content.Model("models/tower.glb"));
        outpost.Position = new Vector3(5.5f, 0f, 3.2f);
        outpost.RotateY(180f);
        outpost.Part("Flag")!.Material = Material.Matte(Color.FromHex("#E5484D"));

        _titleFont = Game.Content.Font("fonts/Lato-Bold.ttf");

        _red = World.Add(new Box(new Vector3(1.4f, 1.4f, 1.4f), Color.FromHex("#E5484D")));
        _red.Position = new Vector3(-5f, 0.7f, -2f);

        _green = World.Add(new Box(new Vector3(1.2f, 2.4f, 1.2f), Color.FromHex("#30A46C")));
        _green.Position = new Vector3(5f, 1.2f, -2f);

        _blue = World.Add(new Box(new Vector3(1f, 1f, 1f), Color.FromHex("#3E63DD")));
        _blue.Position = new Vector3(0f, 1f, 1f);

        _yellow = _blue.Add(new Sphere(0.55f, Color.FromHex("#FFC53D")));
        _yellow.Position = new Vector3(3f, 0f, 0f);

        Torus ring = _yellow.Add(new Torus(0.8f, 0.95f, Color.FromHex("#E8D9A8")));
        ring.Rotation = new Vector3(20f, 0f, 10f);

        Sphere moon = new Sphere(0.22f, Color.White);
        moon.Material = Material.Glowing(Color.FromHex("#DDEBFF"), 1.5f);
        _yellow.Add(moon);
        moon.Position = new Vector3(1.4f, 0f, 0f);

        Material stone = Material.Matte(Color.FromHex("#8C8A80"));
        foreach (float x in new[] { -7f, 7f })
        {
            Cylinder pillar = new Cylinder(0.45f, 3f, Color.White);
            pillar.Material = stone;
            World.Add(pillar);
            pillar.Position = new Vector3(x, 1.5f, -5f);
            Sphere flame = new Sphere(0.18f, Color.White);
            flame.Material = Material.Glowing(Color.FromHex("#FFB45E"), 2.5f);
            flame.CastsShadow = false;
            pillar.Add(flame);
            flame.Position = new Vector3(0f, 1.75f, 0f);
            flame.Add(Lamp.Warm(7f));
        }

        AddPawn("pawn-red", new Vector3(-2.5f, 0f, 3.5f), Color.FromHex("#E5484D"));
        AddPawn("pawn-blue", new Vector3(2.5f, 0f, 3.5f), Color.FromHex("#3E63DD"));

        Spotlight spot = new Spotlight(Color.FromHex("#CFE3FF"), 14f, 24f);
        spot.Shadows = true;
        spot.Intensity = 2f;
        _spot = World.Add(spot);
        _spot.Position = new Vector3(0f, 9f, 3.5f);

        Torus selectionRing = new Torus(0.5f, 0.65f, Color.White);
        selectionRing.Material = Material.Glowing(Color.FromHex("#FFC53D"), 1.5f);
        selectionRing.CastsShadow = false;
        selectionRing.Pickable = false;
        selectionRing.Visible = false;
        _ring = World.Add(selectionRing);
    }

    protected override void Update(GameTime time)
    {
        Keyboard keys = Game.Keyboard;
        Mouse mouse = Game.Mouse;
        float step = time.DeltaSeconds;

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
        FollowSelection();
        if (_busy is { IsDone: true })
        {
            Console.WriteLine(_whenDone);
            _busy = null;
        }

        _hoveredAction = ActionUnder(mouse.Position);
        if (_hoveredAction >= 0)
        {
            ClearHover();
            if (mouse.WasPressed(MouseButton.Left) && _busy is null)
            {
                _lastAction = Actions[_hoveredAction];
                Console.WriteLine($"action: {_lastAction}");
                Act(_lastAction, _selected!);
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
        RectF panel = new RectF(16f, 16f, 380f, 112f);
        canvas.FillRect(panel, Color.Black.WithAlpha(0.45f));
        canvas.DrawRect(panel, Color.White.WithAlpha(0.25f));
        canvas.Font = _titleFont;
        canvas.DrawText("Evil Engine orbit demo", new Vector2(30f, 24f), 24f, Color.White);
        canvas.Font = null;
        string status = _selected is null ? "Click a pawn to select it" : $"Selected: {_selected.Tag}" + (_lastAction is null ? string.Empty : $"   Last action: {_lastAction}");
        canvas.DrawText(status, new Vector2(30f, 60f), 16f, Color.FromHex("#FFC53D"));
        canvas.DrawText("Space pause   Q/E turn   wheel zoom   F10 small   F11 full screen", new Vector2(30f, 92f), 13f, Color.White.WithAlpha(0.7f));
    }

    private void DrawCompass(Canvas canvas)
    {
        using TransformScope compass = canvas.Group();
        canvas.Translate(canvas.Size.X - 70f, 70f);
        canvas.FillCircle(Vector2.Zero, 46f, Color.Black.WithAlpha(0.45f));
        canvas.Rotate(_view!.Yaw);
        canvas.FillTriangle(new Vector2(0f, -32f), new Vector2(-9f, 0f), new Vector2(9f, 0f), Color.FromHex("#E5484D"));
        canvas.FillTriangle(new Vector2(0f, 32f), new Vector2(-9f, 0f), new Vector2(9f, 0f), Color.White.WithAlpha(0.8f));
        canvas.DrawText("N", new RectF(-10f, -52f, 20f, 16f), 13f, Color.White, TextAlign.Center);
    }

    private void DrawActions(Canvas canvas)
    {
        RectF[] buttons = ActionRects(canvas.Size);
        bool waiting = _busy is not null;
        for (int i = 0; i < buttons.Length; i++)
        {
            bool hovered = i == _hoveredAction && !waiting;
            canvas.FillRect(buttons[i], hovered ? Color.FromHex("#3E63DD") : Color.Black.WithAlpha(waiting ? 0.25f : 0.55f));
            canvas.DrawRect(buttons[i], Color.White.WithAlpha(hovered ? 0.9f : 0.35f), 2f);
            canvas.DrawText(Actions[i], buttons[i], 18f, Color.White.WithAlpha(waiting ? 0.4f : 1f), TextAlign.Center);
        }
    }

    private static RectF[] ActionRects(Vector2 screen)
    {
        const float width = 150f;
        const float height = 46f;
        const float gap = 16f;
        float left = (screen.X - (width * Actions.Length + gap * (Actions.Length - 1))) / 2f;
        float top = screen.Y - height - 32f;
        return [.. Enumerable.Range(0, Actions.Length).Select(i => new RectF(left + i * (width + gap), top, width, height))];
    }

    private int ActionUnder(Vector2 point)
    {
        if (_selected is null)
        {
            return -1;
        }

        RectF[] buttons = ActionRects(Game.ScreenSize);
        return Array.FindIndex(buttons, button => button.Contains(point));
    }

    private void LogScreen()
    {
        Vector2 screen = Game.ScreenSize;
        if (screen == _loggedScreen || _selected is null)
        {
            return;
        }

        _loggedScreen = screen;
        RectF[] buttons = ActionRects(screen);
        string centres = string.Join(' ', Actions.Select((name, i) => $"{name} {(int)buttons[i].Center.X},{(int)buttons[i].Center.Y}"));
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
        OrbitCamera view = _view!;
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
        Capsule? pawn = PawnOf(Pick(mouse.Position)?.Target);
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
        _ring!.Visible = true;
        FollowSelection();
        Console.WriteLine($"selected: {pawn.Tag}");
    }

    private void FollowSelection()
    {
        if (_selected is null)
        {
            return;
        }

        Vector3 feet = _selected.WorldPosition with { Y = 0.05f };
        _ring!.Position = feet;
        _spot!.Position = feet + new Vector3(0f, 9f, 0f);
    }

    private void Act(string action, Capsule pawn)
    {
        MotionPlayer motion = Game.Motion;
        Vector3 start = pawn.Position;
        switch (action)
        {
            case "Move":
                float side = start.X < 0f ? 1f : -1f;
                _busy = motion.MoveAlong(pawn, [start + new Vector3(0f, 0f, -2f), start + new Vector3(2f * side, 0f, -2f)], 0.6f);
                _whenDone = $"arrived: {pawn.Tag} at {start.X + 2f * side:0},{start.Z - 2f:0}";
                break;
            case "Attack":
                Capsule enemy = _pawns.First(other => !ReferenceEquals(other, pawn));
                Vector3 reach = Vector3.Lerp(start, enemy.Position, 0.45f) with { Y = start.Y };
                Vector3 push = enemy.Position + Vector3.Normalize(enemy.Position - start) * 0.6f;
                motion.Face(pawn, enemy.Position, 0.25f);
                motion.Hop(pawn, reach, 1.2f, 0.45f);
                _busy = motion.Hop(pawn, start, 0.5f, 0.4f);
                motion.Wait(enemy, 0.7f);
                motion.Hop(enemy, push with { Y = enemy.Position.Y }, 0.35f, 0.2f);
                motion.MoveTo(enemy, enemy.Position, 0.35f);
                _whenDone = "done: attack";
                break;
            default:
                _busy = motion.Spin(pawn, 360f, 0.8f);
                _whenDone = "done: wait";
                break;
        }
    }

    private Capsule? PawnOf(WorldObject? target)
    {
        for (WorldObject? current = target; current is not null; current = current.Parent)
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
        Capsule pawn = new Capsule(0.35f, 1.4f, team);
        pawn.Name = name;
        pawn.Tag = name;
        World.Add(pawn);
        pawn.Position = place + new Vector3(0f, 0.7f, 0f);
        Cone hat = pawn.Add(new Cone(0.3f, 0.45f, Color.FromHex("#F0F0F0")));
        hat.Position = new Vector3(0f, 0.9f, 0f);
        _pawns.Add(pawn);
    }
}
