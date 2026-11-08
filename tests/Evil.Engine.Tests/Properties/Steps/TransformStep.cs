using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties.Steps;

internal abstract record TransformStep
{
    public abstract void ApplyTo(TransformStack stack);

    public static TransformStep Random(Random random)
    {
        int kind = random.Next(3);
        if (kind == 0)
        {
            return new MoveStep(Gen.Point2(random, 100f));
        }

        if (kind == 1)
        {
            return new TurnStep(Gen.Number(random, -360f, 360f));
        }

        return new ScaleStep(ScaleStep.RandomFactor(random));
    }

    public static TransformStep[] RandomSequence(Random random)
    {
        TransformStep[] steps = new TransformStep[random.Next(0, 6)];
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i] = Random(random);
        }

        return steps;
    }

    public static void ApplyAll(TransformStack stack, TransformStep[] steps)
    {
        foreach (TransformStep step in steps)
        {
            step.ApplyTo(stack);
        }
    }
}
