using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties;

internal static class MathValidatorTests
{
    private static readonly Vector3 Broken = new Vector3(float.NaN, 0f, 0f);
    private static readonly Vector3 Endless = new Vector3(0f, float.PositiveInfinity, 0f);

    [Test]
    public static void NaNAndInfinityAreRefusedWithAHint()
    {
        TransformStack stack = new TransformStack();
        Action[] broken =
        [
            () => new Ray3(Broken, Vector3.UnitX),
            () => new Ray3(Vector3.Zero, Endless),
            () => Rotations.AroundAxis(Broken, 10f),
            () => Rotations.AroundAxis(Vector3.UnitY, float.NaN),
            () => Rotations.LookAlong(Endless),
            () => Rotations.FromEulerDegrees(Broken),
            () => Angle.Wrap(float.NaN),
            () => Angle.Wrap(float.NegativeInfinity),
            () => Smoothing.Toward(0f, 1f, float.NaN, 0.1f),
            () => Smoothing.Toward(Vector3.Zero, Vector3.One, 4f, float.PositiveInfinity),
            () => Orbit.PoseFor(Broken, 0f, 30f, 10f),
            () => Orbit.PoseFor(Vector3.Zero, 0f, 30f, float.NaN),
            () => new Polygon(Vector2.Zero, new Vector2(float.NaN, 1f), Vector2.One),
            () => Bounds3.FromPoints([Vector3.Zero, Endless]),
            () => Bounds3.FromCenter(Broken, Vector3.One),
            () => stack.Translate(new Vector2(float.NaN, 0f)),
            () => stack.Rotate(float.PositiveInfinity),
            () => stack.Scale(float.NaN),
            () => new Ray3(Vector3.Zero, Vector3.UnitX).IntersectSphere(Broken, 1f),
        ];
        for (int i = 0; i < broken.Length; i++)
        {
            ArgumentException refused = Check.Throws<ArgumentException>(broken[i], "finite");
            Check.True(refused.Message.Contains("division by zero", StringComparison.Ordinal), $"case {i} says where NaN comes from: {refused.Message}");
        }

        Check.Throws<ArgumentException>(() => Ease.Apply(Easing.Linear, float.NaN), "zero duration");
        Check.Equal(0, stack.Depth, "nothing was half done");
        Check.Equal(Matrix3x2.Identity, stack.Current, "a refused step changes nothing");
    }

    [Test]
    public static void ImpossibleValuesAreRefusedWithTheReason()
    {
        Ray3 ray = new Ray3(Vector3.Zero, Vector3.UnitX);
        Check.Throws<ArgumentException>(() => new Ray3(Vector3.Zero, Vector3.Zero), "no length");
        Check.Throws<ArgumentException>(() => Rotations.AroundAxis(Vector3.Zero, 10f), "no length");
        Check.Throws<ArgumentException>(() => ray.IntersectPlane(Vector3.Zero, Vector3.Zero), "no length");
        Check.Throws<ArgumentOutOfRangeException>(() => ray.IntersectSphere(Vector3.Zero, -1f), "cannot be negative");
        Check.Throws<ArgumentOutOfRangeException>(() => Orbit.PoseFor(Vector3.Zero, 0f, 30f, -2f), "cannot be negative");
        Check.Throws<ArgumentOutOfRangeException>(() => Smoothing.Toward(0f, 1f, 4f, -0.1f), "cannot run backwards");
        Check.Throws<InvalidOperationException>(() => new Pose(Vector3.Zero, Quaternion.Identity, new Vector3(1f, 0f, 1f)).ToLocal(Vector3.One), "zero scale");

        Pose stretched = new Pose(Vector3.Zero, Quaternion.Identity, new Vector3(1f, 3f, 1f));
        Pose turned = new Pose(Vector3.One, Rotations.AroundAxis(Vector3.UnitZ, 30f), Vector3.One);
        Check.Throws<InvalidOperationException>(() => stretched.ToWorld(turned), "skew");
    }

    [Test]
    public static void EdgeValuesThatMakeSenseStillWork()
    {
        Check.Equal(0f, Smoothing.Toward(0f, 1f, 4f, 0f), "no time passed, nothing moved");
        Check.Near(Vector3.Zero, Orbit.PoseFor(Vector3.Zero, 10f, 20f, 0f).Position, "an orbit of zero sits on the target");
        Check.Equal(0f, Angle.Wrap(-0f), "minus zero");
        Check.Equal(0f, Ease.Apply(Easing.Linear, float.NegativeInfinity), "minus infinity is before the start");
        Check.Equal(1f, Ease.Apply(Easing.Linear, float.PositiveInfinity), "infinity is after the end");
        Pose stretched = new Pose(Vector3.Zero, Quaternion.Identity, new Vector3(1f, 3f, 1f));
        Pose moved = new Pose(Vector3.One, Quaternion.Identity, new Vector3(2f, 1f, 1f));
        Check.Near(new Vector3(1f, 3f, 1f), stretched.ToWorld(moved).Position, "an unturned child in a stretched parent is fine");
        Check.Equal(0f, new Ray3(Vector3.Zero, Vector3.UnitX).IntersectSphere(Vector3.Zero, 0f), "a point sphere at the start");
        TransformStack stack = new TransformStack();
        stack.Scale(0f);
        Check.Near(Vector2.Zero, stack.ToWorld(new Vector2(5f, 5f)), "scale zero collapses, it is not an error");
    }
}
