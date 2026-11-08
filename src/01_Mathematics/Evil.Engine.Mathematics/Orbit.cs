using System.Numerics;

namespace Evil.Engine.Mathematics;

public static class Orbit
{
    public static Pose PoseFor(Vector3 target, float yawDegrees, float pitchDegrees, float distance)
    {
        Finite.Point(target, "The orbit's target");
        if (Finite.Number(distance, "The orbit's distance") < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(distance), distance, "An orbit's distance cannot be negative.");
        }

        Quaternion rotation = Rotations.FromEulerDegrees(new Vector3(-pitchDegrees, yawDegrees, 0f));
        Vector3 position = target + Vector3.Transform(new Vector3(0f, 0f, distance), rotation);
        return new Pose(position, rotation, Vector3.One);
    }
}
