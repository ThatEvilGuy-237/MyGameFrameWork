using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties;

internal static class RotationAndPoseProperties
{
    [Test]
    public static void TurningAroundAnAxisMatchesRodrigues()
    {
        Property.ForAll(
            nameof(TurningAroundAnAxisMatchesRodrigues),
            random => (Axis: Gen.Direction(random), Degrees: Gen.Number(random, -720f, 720f), Point: Gen.Point3(random, 10f)),
            input =>
            {
                Quaternion turn = Rotations.AroundAxis(input.Axis, input.Degrees);
                Check.Near(Gen.Rodrigues(input.Point, input.Axis, input.Degrees), Rotations.Rotate(input.Point, turn), "turned point", 1e-3f * (1f + input.Point.Length()));
                Check.Near(input.Axis, Rotations.Rotate(input.Axis, turn), "the axis stays put", 1e-4f);
                Check.Near(1f, turn.Length(), "a unit quaternion", 1e-4f);
            });
    }

    [Test]
    public static void EulerTurnsRollThenPitchThenYaw()
    {
        Property.ForAll(
            nameof(EulerTurnsRollThenPitchThenYaw),
            random => (Degrees: Gen.Euler(random), Point: Gen.Point3(random, 10f)),
            input =>
            {
                Vector3 rolled = Gen.Rodrigues(input.Point, Vector3.UnitZ, input.Degrees.Z);
                Vector3 pitched = Gen.Rodrigues(rolled, Vector3.UnitX, input.Degrees.X);
                Vector3 expected = Gen.Rodrigues(pitched, Vector3.UnitY, input.Degrees.Y);
                Check.Near(expected, Rotations.Rotate(input.Point, Rotations.FromEulerDegrees(input.Degrees)), "turned point", 1e-3f * (1f + input.Point.Length()));
            });
    }

    [Test]
    public static void LookAlongFacesTheDirectionWithALevelHorizon()
    {
        Property.ForAll(
            nameof(LookAlongFacesTheDirectionWithALevelHorizon),
            random => Gen.Direction(random) * Gen.Number(random, 0.01f, 100f),
            direction =>
            {
                Quaternion look = Rotations.LookAlong(direction);
                Check.Near(1f, look.Length(), "a unit quaternion", 1e-4f);
                Check.Near(Vector3.Normalize(direction), Rotations.Rotate(Rotations.Forward, look), "forward", 1e-4f);
                if (MathF.Abs(Vector3.Normalize(direction).Y) < 0.99f)
                {
                    Check.Near(0f, Rotations.Rotate(Vector3.UnitX, look).Y, "right stays level", 1e-4f);
                    Check.True(Rotations.Rotate(Vector3.UnitY, look).Y > 0f, "up stays up");
                }
            });
    }

    [Test]
    public static void ToLocalUndoesToWorld()
    {
        Property.ForAll(
            nameof(ToLocalUndoesToWorld),
            random => (Pose: Gen.Placement(random, false), Point: Gen.Point3(random, 20f)),
            input => Check.Near(input.Point, input.Pose.ToLocal(input.Pose.ToWorld(input.Point)), "there and back", 1e-3f * (1f + input.Point.Length())));
    }

    [Test]
    public static void TheMatrixAgreesWithToWorld()
    {
        Property.ForAll(
            nameof(TheMatrixAgreesWithToWorld),
            random => (Pose: Gen.Placement(random, false), Point: Gen.Point3(random, 20f)),
            input =>
            {
                Vector3 expected = input.Pose.ToWorld(input.Point);
                Check.Near(expected, Vector3.Transform(input.Point, input.Pose.ToMatrix()), "matrix", 1e-3f * (1f + expected.Length()));
            });
    }

    [Test]
    public static void PosesComposeLikeTheirPoints()
    {
        Property.ForAll(
            nameof(PosesComposeLikeTheirPoints),
            random => (Parent: Gen.Placement(random, true), Child: Gen.Placement(random, false), Point: Gen.Point3(random, 10f)),
            input =>
            {
                Vector3 stepByStep = input.Parent.ToWorld(input.Child.ToWorld(input.Point));
                Vector3 combined = input.Parent.ToWorld(input.Child).ToWorld(input.Point);
                Check.Near(stepByStep, combined, "child in parent", 1e-3f * (1f + stepByStep.Length()));
            });
    }

    [Test]
    public static void ThePoseDirectionsAreTurnedAxes()
    {
        Property.ForAll(
            nameof(ThePoseDirectionsAreTurnedAxes),
            random => Gen.Placement(random, false),
            pose =>
            {
                Check.Near(Rotations.Rotate(-Vector3.UnitZ, pose.Rotation), pose.Forward, "forward", 1e-4f);
                Check.Near(0f, Vector3.Dot(pose.Right, pose.Up), "right and up are square", 1e-4f);
                Check.Near(-pose.Forward, Vector3.Cross(pose.Right, pose.Up), "right x up is back", 1e-4f);
            });
    }

    [Test]
    public static void TurnsAroundYAddUp()
    {
        Property.ForAll(
            nameof(TurnsAroundYAddUp),
            random => (Pose: Gen.Placement(random, true), A: Gen.Number(random, -360f, 360f), B: Gen.Number(random, -360f, 360f)),
            input =>
            {
                Pose twice = input.Pose.TurnedY(input.A).TurnedY(input.B);
                Pose once = input.Pose.TurnedY(input.A + input.B);
                Check.Near(once.Forward, twice.Forward, "forward", 1e-3f);
                Check.Near(once.Up, twice.Up, "up", 1e-3f);
            });
    }

    [Test]
    public static void AnOrbitSitsAtItsDistanceAndLooksAtTheTarget()
    {
        Property.ForAll(
            nameof(AnOrbitSitsAtItsDistanceAndLooksAtTheTarget),
            random => (Target: Gen.Point3(random, 50f), Yaw: Gen.Number(random, -360f, 360f), Pitch: Gen.Number(random, -89f, 89f), Distance: Gen.Number(random, 0.5f, 100f)),
            input =>
            {
                Pose eye = Orbit.PoseFor(input.Target, input.Yaw, input.Pitch, input.Distance);
                Check.Near(input.Distance, Vector3.Distance(eye.Position, input.Target), "distance", 1e-3f * input.Distance);
                Check.Near(Vector3.Normalize(input.Target - eye.Position), eye.Forward, "looks at the target", 1e-3f);
                Check.Near(input.Distance * MathF.Sin(Angle.ToRadians(input.Pitch)), eye.Position.Y - input.Target.Y, "height from the pitch", 1e-3f * input.Distance);
            });
    }
}
