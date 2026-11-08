using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

public sealed class MotionPlayer
{
    private readonly Dictionary<object, Queue<Tween>> _queues = new Dictionary<object, Queue<Tween>>(ReferenceEqualityComparer.Instance);
    private readonly List<Tween> _free = [];
    private float _speed = 1f;

    internal MotionPlayer()
    {
    }

    public float Speed
    {
        get => _speed;
        set => _speed = Math.Max(0f, value);
    }

    public bool IsBusy() =>
        _queues.Values.Any(queue => queue.Any(tween => !tween.IsDone)) || _free.Any(tween => !tween.IsDone);

    public bool IsBusy(WorldObject target) =>
        _queues.TryGetValue(target, out Queue<Tween>? queue) && queue.Any(tween => !tween.IsDone);

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
        _free.Add(tween);
        return tween;
    }

    public void Stop(WorldObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (_queues.Remove(target, out Queue<Tween>? queue))
        {
            foreach (Tween tween in queue)
            {
                tween.Cancel();
            }
        }
    }

    public void Complete(WorldObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (_queues.Remove(target, out Queue<Tween>? queue))
        {
            foreach (Tween tween in queue)
            {
                if (!tween.TargetIsGone)
                {
                    tween.FinishNow();
                }
            }
        }
    }

    public void StopAll()
    {
        foreach (Tween? tween in _queues.Values.SelectMany(queue => queue).Concat(_free))
        {
            tween.Cancel();
        }

        _queues.Clear();
        _free.Clear();
    }

    internal void Update(float seconds)
    {
        float step = seconds * _speed;
        foreach (object? key in _queues.Keys.ToList())
        {
            Queue<Tween> queue = _queues[key];
            float left = step;
            while (queue.TryPeek(out Tween? tween))
            {
                if (tween.IsDone || tween.TargetIsGone)
                {
                    tween.Cancel();
                    queue.Dequeue();
                    continue;
                }

                if (left <= 0f && tween.Duration > 0f)
                {
                    break;
                }

                left = tween.Advance(left);
                if (!tween.IsDone)
                {
                    break;
                }

                queue.Dequeue();
            }

            if (queue.Count == 0)
            {
                _queues.Remove(key);
            }
        }

        for (int i = _free.Count - 1; i >= 0; i--)
        {
            Tween tween = _free[i];
            if (!tween.IsDone && step > 0f)
            {
                tween.Advance(step);
            }

            if (tween.IsDone)
            {
                _free.RemoveAt(i);
            }
        }
    }

    private static WorldObject Live(WorldObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        ObjectDisposedException.ThrowIf(target.IsDestroyed, target);
        return target;
    }

    private T Enqueue<T>(object key, T tween)
        where T : Tween
    {
        if (!_queues.TryGetValue(key, out Queue<Tween>? queue))
        {
            queue = new Queue<Tween>();
            _queues[key] = queue;
        }

        queue.Enqueue(tween);
        return tween;
    }
}
