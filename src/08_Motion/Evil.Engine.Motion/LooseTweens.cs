namespace Evil.Engine.Motion;

internal sealed class LooseTweens : TweenGroup
{
    public override void Advance(float seconds)
    {
        for (int i = Tweens.Count - 1; i >= 0; i--)
        {
            Tween tween = Tweens[i];
            if (!tween.IsDone && seconds > 0f)
            {
                tween.Advance(seconds);
            }

            if (tween.IsDone)
            {
                Tweens.RemoveAt(i);
            }
        }
    }
}
