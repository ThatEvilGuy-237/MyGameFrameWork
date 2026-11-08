using System.Numerics;

namespace Evil.Engine.Mathematics;

public static class Orbit
{
    public static Pose PoseFor(Vector3 target, float yawDegrees, float pitchDegrees, float distance)
    {
        Quaternion rotation = Rotations.FromEulerDegrees(new Vector3(-pitchDegrees, yawDegrees, 0f));
        Vector3 position = target + Vector3.Transform(new Vector3(0f, 0f, distance), rotation);
        return new Pose(position, rotation, Vector3.One);
    }
}
