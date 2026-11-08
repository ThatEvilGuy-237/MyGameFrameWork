using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Transforms;

internal sealed class ATransformTurnsAndScalesAroundItsOrigin : Property<Transform2D>
{
    protected override Transform2D Generate(Random random) =>
        new Transform2D(Gen.Point2(random, 100f), Gen.Number(random, -360f, 360f), ScaleStep.RandomFactor(random), Gen.Point2(random, 20f));

    protected override void Verify(Transform2D sample)
    {
        Matrix3x2 matrix = sample.ToMatrix();
        Check.Near(sample.Position, Vector2.Transform(sample.Origin, matrix), "the origin lands on the position", 1e-3f);
        Vector2 offset = new Vector2(3f, -2f);
        Vector2 expected = sample.Position + PlaneTurn.Apply(offset * sample.Scale, sample.Rotation);
        float tolerance = 1e-3f * (1f + expected.Length());
        Check.Near(expected, Vector2.Transform(sample.Origin + offset, matrix), "a point next to the origin", tolerance);
        Check.Near(Steps(sample).ToWorld(offset), Applied(sample).ToWorld(offset), "Apply is the same as the steps", tolerance);
    }

    private static TransformStack Applied(Transform2D transform)
    {
        TransformStack stack = new TransformStack();
        stack.Apply(transform);
        return stack;
    }

    private static TransformStack Steps(Transform2D transform)
    {
        TransformStack stack = new TransformStack();
        stack.Translate(transform.Position);
        stack.Rotate(transform.Rotation);
        stack.Scale(transform.Scale);
        stack.Translate(-transform.Origin);
        return stack;
    }
}
