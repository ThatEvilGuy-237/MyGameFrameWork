using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties;

internal static class RectAndTransformProperties
{
    [Test]
    public static void IntersectIsInsideBothAndAgreesWithIntersects()
    {
        Property.ForAll(
            nameof(IntersectIsInsideBothAndAgreesWithIntersects),
            random => (A: Gen.Rect(random, 50f), B: Gen.Rect(random, 50f)),
            input =>
            {
                RectF overlap = input.A.Intersect(input.B);
                Check.Equal(!overlap.IsEmpty, input.A.Intersects(input.B), "Intersect and Intersects agree");
                Check.Equal(overlap, input.B.Intersect(input.A), "the order does not matter");
                if (!overlap.IsEmpty)
                {
                    Check.True(input.A.Inflate(1e-4f).Contains(overlap) && input.B.Inflate(1e-4f).Contains(overlap), $"{overlap} sticks out");
                }
            });
    }

    [Test]
    public static void UnionHoldsBoth()
    {
        Property.ForAll(
            nameof(UnionHoldsBoth),
            random => (A: Gen.Rect(random, 50f), B: Gen.Rect(random, 50f)),
            input =>
            {
                RectF union = input.A.Union(input.B).Inflate(1e-4f);
                Check.True(union.Contains(input.A) && union.Contains(input.B), $"{union} misses a part");
                Check.Equal(input.A.Union(input.B), input.B.Union(input.A), "the order does not matter");
            });
    }

    [Test]
    public static void CornersAndCentersBuildTheSameRect()
    {
        Property.ForAll(
            nameof(CornersAndCentersBuildTheSameRect),
            random => (A: Gen.Point2(random, 50f), B: Gen.Point2(random, 50f), Grow: Gen.Number(random, -2f, 2f)),
            input =>
            {
                RectF rect = RectF.FromCorners(input.A, input.B);
                Check.Equal(rect, RectF.FromCorners(input.B, input.A), "either corner first");
                Check.Near((input.A + input.B) / 2f, rect.Center, "center", 1e-3f);
                Check.Near(rect.Center, RectF.FromCenter(rect.Center, rect.Size).Center, "from the center", 1e-3f);
                RectF back = rect.Inflate(input.Grow).Inflate(-input.Grow);
                Check.Near(rect.Position, back.Position, "inflate and back", 1e-3f);
                Check.Near(rect.Size, back.Size, "inflate and back", 1e-3f);
            });
    }

    [Test]
    public static void PopPutsTheStackBackExactly()
    {
        Property.ForAll(
            nameof(PopPutsTheStackBackExactly),
            random => (Before: RandomSteps(random), Inside: RandomSteps(random)),
            input =>
            {
                TransformStack stack = new TransformStack();
                Run(stack, input.Before);
                Matrix3x2 saved = stack.Current;
                using (stack.Group())
                {
                    Run(stack, input.Inside);
                }

                Check.Equal(saved, stack.Current, "after the group");
                Check.Equal(0, stack.Depth, "depth");
            });
    }

    [Test]
    public static void TheStackAppliesTheInnermostStepFirst()
    {
        Property.ForAll(
            nameof(TheStackAppliesTheInnermostStepFirst),
            random => (Move: Gen.Point2(random, 100f), Degrees: Gen.Number(random, -360f, 360f), Scale: NonZeroScale(random), Point: Gen.Point2(random, 50f)),
            input =>
            {
                TransformStack stack = new TransformStack();
                stack.Translate(input.Move);
                stack.Rotate(input.Degrees);
                stack.Scale(input.Scale);
                Vector2 expected = input.Move + Turn(input.Point * input.Scale, input.Degrees);
                Check.Near(expected, stack.ToWorld(input.Point), "move after turn after scale", 1e-3f * (1f + expected.Length()));
            });
    }

    [Test]
    public static void ToLocalUndoesToWorld()
    {
        Property.ForAll(
            nameof(ToLocalUndoesToWorld),
            random => (Steps: RandomSteps(random), Point: Gen.Point2(random, 50f)),
            input =>
            {
                TransformStack stack = new TransformStack();
                Run(stack, input.Steps);
                Vector2 back = stack.ToLocal(stack.ToWorld(input.Point));
                Check.Near(input.Point, back, "there and back", 1e-2f * (1f + input.Point.Length()));
            });
    }

    [Test]
    public static void ATransformTurnsAndScalesAroundItsOrigin()
    {
        Property.ForAll(
            nameof(ATransformTurnsAndScalesAroundItsOrigin),
            random => new Transform2D(Gen.Point2(random, 100f), Gen.Number(random, -360f, 360f), NonZeroScale(random), Gen.Point2(random, 20f)),
            transform =>
            {
                Matrix3x2 matrix = transform.ToMatrix();
                Check.Near(transform.Position, Vector2.Transform(transform.Origin, matrix), "the origin lands on the position", 1e-3f);
                Vector2 offset = new Vector2(3f, -2f);
                Vector2 expected = transform.Position + Turn(offset * transform.Scale, transform.Rotation);
                Check.Near(expected, Vector2.Transform(transform.Origin + offset, matrix), "a point next to the origin", 1e-3f * (1f + expected.Length()));

                TransformStack applied = new TransformStack();
                applied.Apply(transform);
                TransformStack steps = new TransformStack();
                steps.Translate(transform.Position);
                steps.Rotate(transform.Rotation);
                steps.Scale(transform.Scale);
                steps.Translate(-transform.Origin);
                Check.Near(steps.ToWorld(offset), applied.ToWorld(offset), "Apply is the same as the steps", 1e-3f * (1f + expected.Length()));
            });
    }

    private static Vector2 Turn(Vector2 point, float degrees)
    {
        double radians = degrees * Math.PI / 180.0;
        float cos = (float)Math.Cos(radians);
        float sin = (float)Math.Sin(radians);
        return new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos);
    }

    private static Vector2 NonZeroScale(Random random) =>
        new Vector2(Gen.Number(random, 0.2f, 3f) * (Gen.Chance(random, 0.2) ? -1f : 1f), Gen.Number(random, 0.2f, 3f));

    private static (int Kind, Vector2 Value)[] RandomSteps(Random random)
    {
        (int Kind, Vector2 Value)[] steps = new (int Kind, Vector2 Value)[random.Next(0, 6)];
        for (int i = 0; i < steps.Length; i++)
        {
            int kind = random.Next(3);
            Vector2 value = kind switch
            {
                0 => Gen.Point2(random, 100f),
                1 => new Vector2(Gen.Number(random, -360f, 360f), 0f),
                _ => NonZeroScale(random),
            };
            steps[i] = (kind, value);
        }

        return steps;
    }

    private static void Run(TransformStack stack, (int Kind, Vector2 Value)[] steps)
    {
        foreach ((int kind, Vector2 value) in steps)
        {
            switch (kind)
            {
                case 0:
                    stack.Translate(value);
                    break;
                case 1:
                    stack.Rotate(value.X);
                    break;
                default:
                    stack.Scale(value);
                    break;
            }
        }
    }
}
