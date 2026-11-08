namespace Evil.Engine.Sample;

internal sealed class PauseScene : Scene
{
    public override bool IsOverlay => true;

    protected override void Update(GameTime time)
    {
        if (Game.Keyboard.WasPressed(Key.P) || Game.Keyboard.WasPressed(Key.Escape))
        {
            Game.Scenes.Pop();
        }
    }

    protected override void Draw(Canvas canvas)
    {
        canvas.FillRect(canvas.Bounds, Color.Black.WithAlpha(0.55f));
        var center = canvas.Size / 2f;
        canvas.FillRect(new RectF(center.X - 30f, center.Y - 40f, 20f, 80f), Color.White);
        canvas.FillRect(new RectF(center.X + 10f, center.Y - 40f, 20f, 80f), Color.White);
    }
}
