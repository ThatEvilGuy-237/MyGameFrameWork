namespace OrbitDemo;

internal sealed class OrbitScene(SampleOptions options) : Scene
{
    private readonly List<Capsule> _pawns = [];
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

        SteerCamera(keys, mouse, step);
        Hover(mouse);
        if (mouse.WasPressed(MouseButton.Left) && _hovered is not null)
        {
            Select(_hovered);
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

        if (_hovered is not null)
        {
            _hovered.Material.Glow = Color.Black;
        }

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
