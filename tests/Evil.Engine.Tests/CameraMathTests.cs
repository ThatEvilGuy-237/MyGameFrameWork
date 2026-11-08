using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests;

internal static class CameraMathTests
{
    [Test]
    public static void OrbitSitsAboveAndBehindLookingAtTheTarget()
    {
        Vector3 target = new Vector3(1f, 0f, 2f);
        Pose pose = Orbit.PoseFor(target, 0f, 45f, 10f);
        Check.Near(target + new Vector3(0f, 7.0711f, 7.0711f), pose.Position, "position");
        Check.Near(Vector3.Normalize(target - pose.Position), pose.Forward, "looks at the target");
    }

    [Test]
    public static void OrbitYawGoesAroundTheTarget()
    {
        Pose pose = Orbit.PoseFor(Vector3.Zero, 90f, 0f, 10f);
        Check.Near(new Vector3(10f, 0f, 0f), pose.Position, "a quarter turn puts the camera on +X");
        Check.Near(new Vector3(-1f, 0f, 0f), pose.Forward, "looking back at the target");
    }

    [Test]
    public static void SmoothingIsTheSameAtAnyFrameRate()
    {
        Check.Near(10f - 10f * MathF.Exp(-1f), Smoothing.Toward(0f, 10f, 10f, 0.1f), "one step");
        float twoSteps = Smoothing.Toward(Smoothing.Toward(0f, 10f, 10f, 0.05f), 10f, 10f, 0.05f);
        Check.Near(Smoothing.Toward(0f, 10f, 10f, 0.1f), twoSteps, "two half steps land in the same place");
        Check.Near(10f, Smoothing.Toward(0f, 10f, 0f, 0.1f), "sharpness 0 jumps");
        Check.Near(new Vector3(5f, 5f, 5f), Smoothing.Toward(Vector3.Zero, new Vector3(10f), 10f, MathF.Log(2f) / 10f), "vectors halfway");
    }
}
