namespace Evil.Engine.Mathematics;

public static class Angle
{
    public static float ToRadians(float degrees) => degrees * (MathF.PI / 180f);

    public static float ToDegrees(float radians) => radians * (180f / MathF.PI);

    public static float Wrap(float degrees)
    {
        float wrapped = degrees % 360f;
        if (wrapped < 0f)
        {
            wrapped += 360f;
        }

        return wrapped >= 360f ? 0f : wrapped;
    }
}
