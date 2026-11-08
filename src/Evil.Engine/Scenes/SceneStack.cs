using Evil.Engine.Graphics;
using Evil.Engine.Hosting;

namespace Evil.Engine.Scenes;

public sealed class SceneStack
{
    private readonly List<Scene> _scenes = [];
    private readonly Queue<SceneChange> _pending = new();
    private readonly GameContext? _game;

    internal SceneStack(GameContext? game) => _game = game;

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
        while (_pending.TryDequeue(out var change))
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
        if (_scenes.Count == 0)
        {
            return;
        }

        var first = _scenes.Count - 1;
        while (first > 0 && _scenes[first].IsOverlay)
        {
            first--;
        }

        for (var i = first; i < _scenes.Count; i++)
        {
            var scene = _scenes[i];
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
        while (_scenes.Count > 0)
        {
            RemoveTop();
        }
    }

    private void Add(Scene scene)
    {
        if (_scenes.Contains(scene))
        {
            throw new InvalidOperationException($"{scene.GetType().Name} is already on the scene stack.");
        }

        scene.Attach(_game);
        _scenes.Add(scene);
        scene.RunEnter();
    }

    private void RemoveTop()
    {
        if (_scenes.Count == 0)
        {
            throw new InvalidOperationException("Scenes.Pop() with no scene on the stack.");
        }

        var top = _scenes[^1];
        _scenes.RemoveAt(_scenes.Count - 1);
        top.RunExit();
        top.Attach(null);
    }

    private enum SceneChangeKind
    {
        Push,
        Pop,
        Switch,
    }

    private readonly record struct SceneChange(SceneChangeKind Kind, Scene? Scene);
}
