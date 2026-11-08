namespace OrbitDemo;

internal sealed class OrbitScene(SampleOptions options) : Scene
{
    private CameraControls _camera = null!;
    private SpinningShapes _shapes = null!;
    private Pawns _pawns = null!;
    private ActionBar _actionBar = null!;
    private PawnActions _actions = null!;
    private Hud _hud = null!;
    private bool _spinning = true;

    protected override void Enter()
    {
        _camera = new CameraControls(Camera);
        Atmosphere = Atmosphere.Dusk();
        Scenery.Build(World, Game.Content);
        _shapes = new SpinningShapes(World);
        _pawns = new Pawns(World);
        _actionBar = new ActionBar();
        _actions = new PawnActions(Game, _pawns, DemoSounds.Load(Game.Content));
        _hud = new Hud(Game.Content.Font("fonts/Lato-Bold.ttf"));
        StartMusic();
    }

    protected override void Update(GameTime time)
    {
        float seconds = time.DeltaSeconds;
        HandleKeys();
        _actions.Update();
        if (_spinning)
        {
            _shapes.Spin(seconds);
            _camera.Drift(seconds);
        }

        _camera.Steer(Game.Keyboard, Game.Mouse, seconds);
        _pawns.FollowSelection();
        _actionBar.Update(Game.ScreenSize, Game.Mouse.Position, _pawns.Selected is not null);
        if (_actionBar.Hovered is not null)
        {
            _pawns.ClearHover();
            ClickAction(_actionBar.Hovered);
            return;
        }

        PointAtPawns();
    }

    protected override void Draw(Canvas canvas)
    {
        _hud.Draw(canvas, Status(), _camera.Yaw);
        if (_pawns.Selected is not null)
        {
            _actionBar.Draw(canvas, _actions.IsBusy);
        }
    }

    private void StartMusic()
    {
        Game.Audio.SetVolume(AudioGroup.Music, 0.35f);
        Game.Audio.PlayMusic(Game.Content.Sound("sounds/music.wav"), 2f);
        if (options.RecordAudioPath is not null)
        {
            Game.Audio.StartRecording();
        }
    }

    private void HandleKeys()
    {
        Keyboard keys = Game.Keyboard;
        if (keys.WasPressed(Key.Escape))
        {
            Game.Exit();
        }

        if (keys.WasPressed(Key.Space))
        {
            ToggleSpinning();
        }

        if (keys.WasPressed(Key.M))
        {
            ToggleMusic();
        }

        if (keys.WasPressed(Key.F10))
        {
            Game.Display.Resize(1024, 640);
        }

        if (keys.WasPressed(Key.F11))
        {
            Game.Display.Fullscreen = !Game.Display.Fullscreen;
        }

        if (keys.WasPressed(Key.F12))
        {
            SaveScreenshot();
        }
    }

    private void ClickAction(string action)
    {
        if (Game.Mouse.WasPressed(MouseButton.Left) && !_actions.IsBusy)
        {
            _actions.Run(action, _pawns.Selected!);
        }
    }

    private void PointAtPawns()
    {
        Capsule? pointed = _pawns.Find(Pick(Game.Mouse.Position)?.Target);
        _pawns.Hover(pointed, Game.Mouse.Position);
        if (Game.Mouse.WasPressed(MouseButton.Left) && _pawns.Hovered is not null)
        {
            _pawns.Select(_pawns.Hovered);
        }
    }

    private string Status()
    {
        if (_pawns.Selected is null)
        {
            return "Click a pawn to select it";
        }

        string lastAction = _actions.Last is null ? string.Empty : $"   Last action: {_actions.Last}";
        return $"Selected: {_pawns.Selected.Tag}{lastAction}";
    }

    private void ToggleSpinning()
    {
        _spinning = !_spinning;
        Console.WriteLine($"spinning: {_spinning}");
    }

    private void ToggleMusic()
    {
        bool muted = !Game.Audio.IsMuted(AudioGroup.Music);
        Game.Audio.Mute(AudioGroup.Music, muted);
        Console.WriteLine($"music muted: {muted}");
    }

    private void SaveScreenshot()
    {
        if (options.RecordAudioPath is not null)
        {
            Game.Audio.SaveRecording(options.RecordAudioPath);
            Console.WriteLine($"recorded: {options.RecordAudioPath}");
        }

        if (options.CapturePath is not null)
        {
            Game.CaptureFrame(options.CapturePath);
            Game.Exit();
            return;
        }

        string path = Path.Combine(Environment.CurrentDirectory, $"orbit-{DateTime.Now:yyyyMMdd-HHmmss}.png");
        Game.CaptureFrame(path);
        Console.WriteLine($"screenshot: {path}");
    }
}
