namespace Evil.Engine.Motion;

internal abstract class TweenGroup
{
    protected List<Tween> Tweens { get; } = new List<Tween>();

    public bool IsEmpty => Tweens.Count == 0;

    public bool IsBusy
    {
        get
        {
            foreach (Tween tween in Tweens)
            {
                if (!tween.IsDone)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public void Add(Tween tween) => Tweens.Add(tween);

    public void Cancel()
    {
        foreach (Tween tween in Tweens)
        {
            tween.Cancel();
        }

        Tweens.Clear();
    }

    public abstract void Advance(float seconds);
}
