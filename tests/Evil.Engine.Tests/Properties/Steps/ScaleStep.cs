using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties.Steps;

internal sealed record ScaleStep(Vector2 Factor) : TransformStep
{
    public static Vector2 RandomFactor(Random random) =>
        new Vector2(Gen.Number(random, 0.2f, 3f) * (Gen.Chance(random, 0.2) ? -1f : 1f), Gen.Number(random, 0.2f, 3f));

    public override void ApplyTo(TransformStack stack) => stack.Scale(Factor);
}
