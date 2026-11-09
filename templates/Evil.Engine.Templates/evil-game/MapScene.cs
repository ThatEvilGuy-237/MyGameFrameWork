namespace MyGame;

internal sealed class MapScene : Scene
{
    private Box? _marker;

    protected override void Enter()
    {
        Atmosphere = Atmosphere.Daylight();
        World.Add(Sun.Noon());
        World.Add(new Floor(new Vector2(20f, 20f), Color.FromHex("#4A6B5A")));
        _marker = World.Add(new Box(Vector3.One, Color.FromHex("#3E63DD")));
        _marker.Position = new Vector3(0f, 0.5f, 0f);
        Camera.Position = new Vector3(0f, 8f, 10f);
        Camera.LookAt(Vector3.Zero);
    }

    protected override void Update(GameTime time)
    {
        if (Game.Keyboard.WasPressed(Key.Escape))
        {
            Game.Exit();
        }

        _marker!.RotateY(45f * time.DeltaSeconds);
    }

    protected override void Draw(Canvas canvas)
    {
        canvas.DrawText("My Game", new Vector2(20f, 20f), 24f, Color.White);
    }
}
