using Evil.Engine.Cameras;
using Evil.Engine.Core;
using Evil.Engine.Hosting;
using Evil.Engine.Lighting;
using Evil.Engine.Screen;
using Evil.Engine.World;

namespace Evil.Engine.Scenes;

public sealed class SceneStack
{
    private readonly List<Scene> _scenes = [];
    private readonly Queue<SceneChange> _pending = new Queue<SceneChange>();
    private readonly GameContext? _game;
    private readonly WorldObject _worlds;

    internal SceneStack(GameContext? game, WorldObject worlds)
    {
        _game = game;
        _worlds = worlds;
    }

    public Scene? Current => _scenes.Count > 0 ? _scenes[^1] : null;

    public int Count => _scenes.Count;

    public IReadOnlyList<Scene> All => _scenes;

    public bool HasPendingChanges => _pending.Count > 0;

    public void Push(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _pending.Enqueue(new SceneChange(SceneChangeKind.Push, scene));
    }

    public void Pop() => _pending.Enqueue(new SceneChange(SceneChangeKind.Pop, null));

    public void Switch(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _pending.Enqueue(new SceneChange(SceneChangeKind.Switch, scene));
    }

    internal void ApplyChanges()
    {
        while (_pending.TryDequeue(out SceneChange change))
        {
            switch (change.Kind)
            {
                case SceneChangeKind.Push:
                    Current?.RunPause();
                    Add(change.Scene!);
                    break;
                case SceneChangeKind.Pop:
                    RemoveTop();
                    Current?.RunResume();
                    break;
                case SceneChangeKind.Switch:
                    if (Current is not null)
                    {
                        RemoveTop();
                    }

                    Add(change.Scene!);
                    break;
            }
        }
    }

    internal void Update(GameTime time) => Current?.RunUpdate(time);

    internal void Draw(Canvas canvas)
    {
        for (int i = FirstVisible(); i < _scenes.Count; i++)
        {
            Scene scene = _scenes[i];
            canvas.ResetTransform();
            scene.RunDraw(canvas);
            if (canvas.Depth != 0)
            {
                throw new InvalidOperationException($"{scene.GetType().Name}.Draw left {canvas.Depth} Push() without a matching Pop().");
            }
        }
    }

    internal void ExitAll()
    {
        _pending.Clear();
        for (int i = _scenes.Count - 1; i >= 0; i--)
        {
            _scenes[i].RunExit();
            _scenes[i].Detach();
        }

        _scenes.Clear();
    }

    private void Add(Scene scene)
    {
        if (_scenes.Contains(scene))
        {
            throw new InvalidOperationException($"{scene.GetType().Name} is already on the scene stack.");
        }

        WorldObject world = _worlds.Add(new WorldObject(scene.GetType().Name));
        Camera camera = new Camera();
        camera.Name = "Camera";
        world.Add(camera);
        scene.Attach(_game, world, camera, new Atmosphere());
        _scenes.Add(scene);
        RefreshViews();
        scene.RunEnter();
    }

    private void RemoveTop()
    {
        if (_scenes.Count == 0)
        {
            throw new InvalidOperationException("Scenes.Pop() with no scene on the stack.");
        }

        Scene top = _scenes[^1];
        _scenes.RemoveAt(_scenes.Count - 1);
        top.RunExit();
        top.Root!.Destroy();
        top.Detach();
        RefreshViews();
    }

    private void RefreshViews()
    {
        if (_scenes.Count == 0)
        {
            return;
        }

        int first = FirstVisible();
        for (int i = 0; i < _scenes.Count; i++)
        {
            _scenes[i].Root!.Visible = i >= first;
        }

        _scenes[first].View!.MakeCurrent();
    }

    private int FirstVisible()
    {
        if (_scenes.Count == 0)
        {
            return 0;
        }

        int first = _scenes.Count - 1;
        while (first > 0 && _scenes[first].IsOverlay)
        {
            first--;
        }

        return first;
    }

    private enum SceneChangeKind
    {
        Push,
        Pop,
        Switch,
    }

    private readonly record struct SceneChange(SceneChangeKind Kind, Scene? Scene);
}
