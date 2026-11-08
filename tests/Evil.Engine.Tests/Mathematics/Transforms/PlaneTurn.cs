using System.Numerics;

namespace Evil.Engine.Tests.Mathematics.Transforms;

internal static class PlaneTurn
{
    public static Vector2 Apply(Vector2 point, float degrees)
    {
        double radians = degrees * Math.PI / 180.0;
        float cos = (float)Math.Cos(radians);
        float sin = (float)Math.Sin(radians);
        return new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos);
    }
}
