using System.Numerics;

namespace Evil.Engine.Mathematics;

public static class Smoothing
{
    public static float Toward(float current, float goal, float sharpness, float seconds) =>
        sharpness <= 0f ? goal : goal + (current - goal) * Remaining(sharpness, seconds);

    public static Vector3 Toward(Vector3 current, Vector3 goal, float sharpness, float seconds) =>
        sharpness <= 0f ? goal : goal + (current - goal) * Remaining(sharpness, seconds);

    private static float Remaining(float sharpness, float seconds)
    {
        Finite.Number(sharpness, "Smoothing's sharpness");
        if (Finite.Number(seconds, "Smoothing's seconds") < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Time cannot run backwards: pass the seconds since the last frame.");
        }

        return MathF.Exp(-sharpness * seconds);
    }
}
