using System.Numerics;

namespace Evil.Engine.Mathematics;

public static class Smoothing
{
    public static float Toward(float current, float goal, float sharpness, float seconds) =>
        sharpness <= 0f ? goal : goal + (current - goal) * MathF.Exp(-sharpness * seconds);

    public static Vector3 Toward(Vector3 current, Vector3 goal, float sharpness, float seconds) =>
        sharpness <= 0f ? goal : goal + (current - goal) * MathF.Exp(-sharpness * seconds);
}
