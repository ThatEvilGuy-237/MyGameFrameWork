using Evil.Engine.Core;

namespace Evil.Engine.Motion;

public abstract class Tween
{
    private readonly TaskCompletionSource<bool> _finished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    private protected Tween(float seconds, Easing easing)
    {
        if (!float.IsFinite(seconds) || seconds < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), $"A motion takes 0 seconds or more, got {seconds}.");
        }

        Duration = seconds;
        Easing = easing;
    }

    public float Duration { get; }

    public Easing Easing { get; }

    public float Elapsed { get; private set; }

    public bool IsStarted { get; private set; }

    public bool IsDone => _finished.Task.IsCompleted;

    public bool WasCancelled => IsDone && !_finished.Task.Result;

    public float Progress => Ease.Apply(Easing, Duration <= 0f ? (IsStarted ? 1f : 0f) : Elapsed / Duration);

    public Task<bool> Finished => _finished.Task;

    public void Cancel() => _finished.TrySetResult(false);

    internal virtual bool TargetIsGone => false;

    internal float Advance(float seconds)
    {
        if (!IsStarted)
        {
            IsStarted = true;
            Start();
        }

        float total = Elapsed + seconds;
        Elapsed = Math.Min(Duration, total);
        Apply(Progress);
        if (total < Duration)
        {
            return 0f;
        }

        _finished.TrySetResult(true);
        return total - Duration;
    }

    internal void FinishNow()
    {
        if (IsDone)
        {
            return;
        }

        Advance(Duration - Elapsed);
        _finished.TrySetResult(true);
    }

    private protected abstract void Start();

    private protected abstract void Apply(float amount);
}
