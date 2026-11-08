using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Validators;

internal static class MathValidatorTests
{
    private static readonly Vector3 Broken = new Vector3(float.NaN, 0f, 0f);
    private static readonly Vector3 Endless = new Vector3(0f, float.PositiveInfinity, 0f);

    [Test]
    public static void NaNAndInfinityAreRefusedWithAHint()
    {
        ExpectHint(Check.Throws<ArgumentException>(RayFromBrokenOrigin, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(RayTowardInfinity, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(TurnAroundBrokenAxis, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(TurnByNaNDegrees, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(LookTowardInfinity, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(TurnByBrokenEuler, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(WrapNaN, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(WrapMinusInfinity, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(SmoothWithNaNSharpness, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(SmoothForEndlessTime, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(OrbitBrokenTarget, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(OrbitNaNDistance, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(PolygonWithBrokenCorner, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(BoundsAroundInfinity, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(BoundsAroundBrokenCenter, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(SphereAtBrokenCenter, "finite"));
        Check.Throws<ArgumentException>(EaseNaN, "zero duration");
    }

    [Test]
    public static void ARefusedStepLeavesTheTransformStackAsItWas()
    {
        TransformStack stack = new TransformStack();
        void MoveByNaN() => stack.Translate(new Vector2(float.NaN, 0f));
        void TurnForever() => stack.Rotate(float.PositiveInfinity);
        void ScaleByNaN() => stack.Scale(float.NaN);
        ExpectHint(Check.Throws<ArgumentException>(MoveByNaN, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(TurnForever, "finite"));
        ExpectHint(Check.Throws<ArgumentException>(ScaleByNaN, "finite"));
        Check.Equal(0, stack.Depth, "nothing was half done");
        Check.Equal(Matrix3x2.Identity, stack.Current, "a refused step changes nothing");
    }

    [Test]
    public static void ImpossibleValuesAreRefusedWithTheReason()
    {
        Check.Throws<ArgumentException>(RayWithoutDirection, "no length");
        Check.Throws<ArgumentException>(TurnAroundNothing, "no length");
        Check.Throws<ArgumentException>(PlaneWithoutNormal, "no length");
        Check.Throws<ArgumentOutOfRangeException>(SphereWithNegativeRadius, "cannot be negative");
        Check.Throws<ArgumentOutOfRangeException>(OrbitAtNegativeDistance, "cannot be negative");
        Check.Throws<ArgumentOutOfRangeException>(SmoothBackInTime, "cannot run backwards");
        Check.Throws<InvalidOperationException>(BackThroughAFlatScale, "zero scale");
        Check.Throws<InvalidOperationException>(TurnedChildInAStretchedParent, "skew");
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

    private static void ExpectHint(ArgumentException refused) =>
        Check.True(refused.Message.Contains("division by zero", StringComparison.Ordinal), $"says where NaN comes from: {refused.Message}");

    private static void RayFromBrokenOrigin() => _ = new Ray3(Broken, Vector3.UnitX);

    private static void RayTowardInfinity() => _ = new Ray3(Vector3.Zero, Endless);

    private static void TurnAroundBrokenAxis() => _ = Rotations.AroundAxis(Broken, 10f);

    private static void TurnByNaNDegrees() => _ = Rotations.AroundAxis(Vector3.UnitY, float.NaN);

    private static void LookTowardInfinity() => _ = Rotations.LookAlong(Endless);

    private static void TurnByBrokenEuler() => _ = Rotations.FromEulerDegrees(Broken);

    private static void WrapNaN() => _ = Angle.Wrap(float.NaN);

    private static void WrapMinusInfinity() => _ = Angle.Wrap(float.NegativeInfinity);

    private static void SmoothWithNaNSharpness() => _ = Smoothing.Toward(0f, 1f, float.NaN, 0.1f);

    private static void SmoothForEndlessTime() => _ = Smoothing.Toward(Vector3.Zero, Vector3.One, 4f, float.PositiveInfinity);

    private static void OrbitBrokenTarget() => _ = Orbit.PoseFor(Broken, 0f, 30f, 10f);

    private static void OrbitNaNDistance() => _ = Orbit.PoseFor(Vector3.Zero, 0f, 30f, float.NaN);

    private static void PolygonWithBrokenCorner() => _ = new Polygon(Vector2.Zero, new Vector2(float.NaN, 1f), Vector2.One);

    private static void BoundsAroundInfinity() => _ = Bounds3.FromPoints([Vector3.Zero, Endless]);

    private static void BoundsAroundBrokenCenter() => _ = Bounds3.FromCenter(Broken, Vector3.One);

    private static void SphereAtBrokenCenter() => _ = new Ray3(Vector3.Zero, Vector3.UnitX).IntersectSphere(Broken, 1f);

    private static void EaseNaN() => _ = Ease.Apply(Easing.Linear, float.NaN);

    private static void RayWithoutDirection() => _ = new Ray3(Vector3.Zero, Vector3.Zero);

    private static void TurnAroundNothing() => _ = Rotations.AroundAxis(Vector3.Zero, 10f);

    private static void PlaneWithoutNormal() => _ = new Ray3(Vector3.Zero, Vector3.UnitX).IntersectPlane(Vector3.Zero, Vector3.Zero);

    private static void SphereWithNegativeRadius() => _ = new Ray3(Vector3.Zero, Vector3.UnitX).IntersectSphere(Vector3.Zero, -1f);

    private static void OrbitAtNegativeDistance() => _ = Orbit.PoseFor(Vector3.Zero, 0f, 30f, -2f);

    private static void SmoothBackInTime() => _ = Smoothing.Toward(0f, 1f, 4f, -0.1f);

    private static void BackThroughAFlatScale() => _ = new Pose(Vector3.Zero, Quaternion.Identity, new Vector3(1f, 0f, 1f)).ToLocal(Vector3.One);

    private static void TurnedChildInAStretchedParent()
    {
        Pose stretched = new Pose(Vector3.Zero, Quaternion.Identity, new Vector3(1f, 3f, 1f));
        Pose turned = new Pose(Vector3.One, Rotations.AroundAxis(Vector3.UnitZ, 30f), Vector3.One);
        _ = stretched.ToWorld(turned);
    }
}
