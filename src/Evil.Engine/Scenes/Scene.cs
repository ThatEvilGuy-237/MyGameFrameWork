using Evil.Engine.Graphics;
using Evil.Engine.Hosting;

namespace Evil.Engine.Scenes;

public abstract class Scene
{
    private GameContext? _game;

    public virtual bool IsOverlay => false;

    protected GameContext Game =>
        _game ?? throw new InvalidOperationException($"{GetType().Name} is not on the scene stack; Game is ready from Enter() on.");

    protected virtual void Enter()
    {
    }

    protected virtual void Exit()
    {
    }

    protected virtual void Pause()
    {
    }

    protected virtual void Resume()
    {
    }

    protected virtual void Update(GameTime time)
    {
    }

    protected virtual void Draw(Canvas canvas)
    {
    }

    internal void Attach(GameContext? game) => _game = game;

    internal void RunEnter() => Enter();

    internal void RunExit() => Exit();

    internal void RunPause() => Pause();

    internal void RunResume() => Resume();

    internal void RunUpdate(GameTime time) => Update(time);

    internal void RunDraw(Canvas canvas) => Draw(canvas);
}
