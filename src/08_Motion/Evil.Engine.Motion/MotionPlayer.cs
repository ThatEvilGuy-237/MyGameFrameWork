using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

public sealed class MotionPlayer
{
    private readonly Dictionary<object, TweenQueue> _queues = new Dictionary<object, TweenQueue>(ReferenceEqualityComparer.Instance);
    private readonly LooseTweens _loose = new LooseTweens();
    private float _speed = 1f;

    internal MotionPlayer()
    {
    }

    public float Speed
    {
        get => _speed;
        set => _speed = Math.Max(0f, value);
    }

    public bool IsBusy()
    {
        foreach (TweenQueue queue in _queues.Values)
        {
            if (queue.IsBusy)
            {
                return true;
            }
        }

        return _loose.IsBusy;
    }

    public bool IsBusy(WorldObject target) => _queues.TryGetValue(target, out TweenQueue? queue) && queue.IsBusy;

    public Tween MoveTo(WorldObject target, Vector3 position, float seconds, Easing easing = Easing.InOutCubic) =>
        Enqueue(target, new MoveTween(Live(target), position, 0f, seconds, easing));

    public Tween Hop(WorldObject target, Vector3 position, float height, float seconds, Easing easing = Easing.Linear) =>
        Enqueue(target, new MoveTween(Live(target), position, height, seconds, easing));

    public Tween MoveAlong(WorldObject target, IReadOnlyList<Vector3> path, float secondsPerStep, bool face = true)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Count == 0)
        {
            throw new ArgumentException("MoveAlong needs at least one point.", nameof(path));
        }

        Tween? last = null;
        foreach (Vector3 point in path)
        {
            if (face)
            {
                Enqueue(target, new FaceTween(Live(target), point, secondsPerStep * 0.25f, Easing.OutQuad));
            }

            last = Enqueue(target, new MoveTween(Live(target), point, 0f, secondsPerStep, Easing.Linear));
        }

        return last!;
    }

    public Tween TurnTo(WorldObject target, float yawDegrees, float seconds, Easing easing = Easing.InOutCubic) =>
        Enqueue(target, new TurnTween(Live(target), Rotations.AroundAxis(Vector3.UnitY, yawDegrees), seconds, easing));

    public Tween Face(WorldObject target, Vector3 point, float seconds, Easing easing = Easing.InOutCubic) =>
        Enqueue(target, new FaceTween(Live(target), point, seconds, easing));

    public Tween Spin(WorldObject target, float degrees, float seconds, Easing easing = Easing.InOutCubic) =>
        Enqueue(target, new SpinTween(Live(target), degrees, seconds, easing));

    public Tween ScaleTo(WorldObject target, Vector3 scale, float seconds, Easing easing = Easing.InOutCubic) =>
        Enqueue(target, new ScaleTween(Live(target), scale, seconds, easing));

    public Tween Wait(WorldObject target, float seconds) => Enqueue(target, new WaitTween(Live(target), seconds));

    public Tween ColorTo(Material material, Color color, float seconds, Easing easing = Easing.InOutCubic)
    {
        ArgumentNullException.ThrowIfNull(material);
        return Enqueue(material, new ColorTween(material, color, seconds, easing));
    }

    public ValueTween Value(float from, float to, float seconds, Easing easing = Easing.InOutCubic)
    {
        ValueTween tween = new ValueTween(from, to, seconds, easing);
        _loose.Add(tween);
        return tween;
    }

    public void Stop(WorldObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (_queues.Remove(target, out TweenQueue? queue))
        {
            queue.Cancel();
        }
    }

    public void Complete(WorldObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (_queues.Remove(target, out TweenQueue? queue))
        {
            queue.FinishNow();
        }
    }

    public void StopAll()
    {
        foreach (TweenQueue queue in _queues.Values)
        {
            queue.Cancel();
        }

        _queues.Clear();
        _loose.Cancel();
    }

    internal void Update(float seconds)
    {
        float step = seconds * _speed;
        AdvanceQueues(step);
        _loose.Advance(step);
    }

    private static WorldObject Live(WorldObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.RequireAlive();
        return target;
    }

    private T Enqueue<T>(object key, T tween)
        where T : Tween
    {
        if (!_queues.TryGetValue(key, out TweenQueue? queue))
        {
            queue = new TweenQueue();
            _queues[key] = queue;
        }

        queue.Add(tween);
        return tween;
    }

    private void AdvanceQueues(float step)
    {
        List<object> finished = new List<object>();
        foreach (KeyValuePair<object, TweenQueue> entry in _queues)
        {
            entry.Value.Advance(step);
            if (entry.Value.IsEmpty)
            {
                finished.Add(entry.Key);
            }
        }

        foreach (object key in finished)
        {
            _queues.Remove(key);
        }
    }
}
