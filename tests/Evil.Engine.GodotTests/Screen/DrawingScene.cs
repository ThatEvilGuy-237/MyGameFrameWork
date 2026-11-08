using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Screen;

internal sealed class DrawingScene(string name, List<string> log, bool overlay, bool moveWithoutGroup) : Scene
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
