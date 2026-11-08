namespace Evil.Engine.Motion;

internal sealed class TweenQueue : TweenGroup
{
    public override void Advance(float seconds)
    {
        float left = seconds;
        while (Tweens.Count > 0)
        {
            Tween tween = Tweens[0];
            if (tween.IsDone || tween.TargetIsGone)
            {
                tween.Cancel();
                Tweens.RemoveAt(0);
                continue;
            }

            if (left <= 0f && tween.Duration > 0f)
            {
                return;
            }

            left = tween.Advance(left);
            if (!tween.IsDone)
            {
                return;
            }

            Tweens.RemoveAt(0);
        }
    }

    public void FinishNow()
    {
        foreach (Tween tween in Tweens)
        {
            if (!tween.TargetIsGone)
            {
                tween.FinishNow();
            }
        }

        Tweens.Clear();
    }
}
