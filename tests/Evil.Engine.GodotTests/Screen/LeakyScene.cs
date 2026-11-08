using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Screen;

internal sealed class LeakyScene : Scene
{
    protected override void Draw(Canvas canvas) => canvas.Push();
}
