using Evil.Engine.Cameras;
using Evil.Engine.Core;
using Evil.Engine.Hosting;
using Evil.Engine.Input;
using Evil.Engine.Lighting;
using Evil.Engine.Screen;
using Evil.Engine.World;

namespace Evil.Engine.Scenes;

public abstract class Scene
{
    private GameContext? _game;
    private WorldObject? _world;
    private Camera? _camera;
    private Atmosphere? _atmosphere;

    public virtual bool IsOverlay => false;

    protected GameContext Game => _game ?? throw NotOnStack(nameof(Game));

    protected WorldObject World => _world ?? throw NotOnStack(nameof(World));

    protected Camera Camera => _camera ?? throw NotOnStack(nameof(Camera));

    protected Atmosphere Atmosphere
    {
        get => _atmosphere ?? throw NotOnStack(nameof(Atmosphere));
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            Camera camera = _camera ?? throw NotOnStack(nameof(Atmosphere));
            _atmosphere = value;
            value.ShowThrough(camera);
        }
    }

    internal WorldObject? Root => _world;

    internal Camera? View => _camera;

    protected Hit? Pick(System.Numerics.Vector2 screenPoint) =>
        Picker.Pick(Camera, World, screenPoint);

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

    internal Atmosphere? Sky => _atmosphere;

    internal void Attach(GameContext? game, WorldObject world, Camera camera, Atmosphere atmosphere)
    {
        _game = game;
        _world = world;
        _camera = camera;
        _atmosphere = atmosphere;
        atmosphere.ShowThrough(camera);
    }

    internal void Detach()
    {
        _game = null;
        _world = null;
        _camera = null;
        _atmosphere = null;
    }

    internal void RunEnter() => Enter();

    internal void RunExit() => Exit();

    internal void RunPause() => Pause();

    internal void RunResume() => Resume();

    internal void RunUpdate(GameTime time) => Update(time);

    internal void RunDraw(Canvas canvas) => Draw(canvas);

    private InvalidOperationException NotOnStack(string member) =>
        new InvalidOperationException($"{GetType().Name} is not on the scene stack; {member} is ready from Enter() on.");
}
