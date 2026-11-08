using Evil.Engine.Cameras;
using Evil.Engine.Core;
using Evil.Engine.Hosting;
using Evil.Engine.World;

namespace Evil.Engine.Scenes;

public abstract class Scene
{
    private GameContext? _game;
    private WorldObject? _world;
    private Camera? _camera;

    public virtual bool IsOverlay => false;

    protected GameContext Game => _game ?? throw NotOnStack(nameof(Game));

    protected WorldObject World => _world ?? throw NotOnStack(nameof(World));

    protected Camera Camera => _camera ?? throw NotOnStack(nameof(Camera));

    internal WorldObject? Root => _world;

    internal Camera? View => _camera;

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

    internal void Attach(GameContext? game, WorldObject world, Camera camera)
    {
        _game = game;
        _world = world;
        _camera = camera;
    }

    internal void Detach()
    {
        _game = null;
        _world = null;
        _camera = null;
    }

    internal void RunEnter() => Enter();

    internal void RunExit() => Exit();

    internal void RunPause() => Pause();

    internal void RunResume() => Resume();

    internal void RunUpdate(GameTime time) => Update(time);

    private InvalidOperationException NotOnStack(string member) =>
        new($"{GetType().Name} is not on the scene stack; {member} is ready from Enter() on.");
}
