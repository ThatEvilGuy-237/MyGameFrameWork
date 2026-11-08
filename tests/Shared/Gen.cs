using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Testing;

internal static class Gen
{
    private static readonly Vector3[] Axes = [Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ];

    public static float Number(Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);

    public static bool Chance(Random random, double odds) => random.NextDouble() < odds;

    public static T Pick<T>(Random random, IReadOnlyList<T> choices) => choices[random.Next(choices.Count)];

    public static Vector2 Point2(Random random, float range) =>
        new Vector2(Number(random, -range, range), Number(random, -range, range));

    public static Vector3 Point3(Random random, float range) =>
        new Vector3(Number(random, -range, range), Number(random, -range, range), Number(random, -range, range));

    public static Vector3 Direction(Random random)
    {
        if (Chance(random, 0.1))
        {
            return Pick(random, Axes);
        }

        while (true)
        {
            Vector3 candidate = Point3(random, 1f);
            float length = candidate.Length();
            if (length > 0.05f && length <= 1f)
            {
                return candidate / length;
            }
        }
    }

    public static Quaternion Turn(Random random)
    {
        float a = (float)random.NextDouble();
        float b = MathF.Tau * (float)random.NextDouble();
        float c = MathF.Tau * (float)random.NextDouble();
        float low = MathF.Sqrt(1f - a);
        float high = MathF.Sqrt(a);
        return new Quaternion(low * MathF.Sin(b), low * MathF.Cos(b), high * MathF.Sin(c), high * MathF.Cos(c));
    }

    public static Vector3 Euler(Random random) =>
        new Vector3(Number(random, -180f, 180f), Number(random, -180f, 180f), Number(random, -180f, 180f));

    public static Vector2[] StarShape(Random random)
    {
        int count = random.Next(3, 24);
        float[] angles = new float[count];
        for (int i = 0; i < count; i++)
        {
            angles[i] = MathF.Tau * (i + Number(random, 0.1f, 0.9f)) / count;
        }

        Vector2 center = Point2(random, 50f);
        List<Vector2> corners = new List<Vector2>(count + 1);
        foreach (float angle in angles)
        {
            corners.Add(center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Number(random, 0.3f, 10f));
        }

        if (Chance(random, 0.2))
        {
            int at = random.Next(count);
            corners.Insert(at + 1, (corners[at] + corners[(at + 1) % count]) / 2f);
        }

        if (Chance(random, 0.5))
        {
            corners.Reverse();
        }

        return [.. corners];
    }

    public static RectF Rect(Random random, float range) =>
        new RectF(Number(random, -range, range), Number(random, -range, range), Number(random, 0.01f, range), Number(random, 0.01f, range));

    public static Bounds3 Box(Random random, float range) =>
        Bounds3.FromCenter(Point3(random, range), new Vector3(Number(random, 0.05f, range), Number(random, 0.05f, range), Number(random, 0.05f, range)));

    public static Pose Placement(Random random, bool evenScale)
    {
        float even = Number(random, 0.2f, 3f);
        Vector3 scale = evenScale
            ? new Vector3(even)
            : new Vector3(Number(random, 0.2f, 3f), Number(random, 0.2f, 3f), Number(random, 0.2f, 3f));
        return new Pose(Point3(random, 20f), Turn(random), scale);
    }

    public static Vector3 Rodrigues(Vector3 vector, Vector3 axis, float degrees)
    {
        Vector3 unit = Vector3.Normalize(axis);
        double radians = degrees * Math.PI / 180.0;
        float cos = (float)Math.Cos(radians);
        float sin = (float)Math.Sin(radians);
        return vector * cos + Vector3.Cross(unit, vector) * sin + unit * Vector3.Dot(unit, vector) * (1f - cos);
    }
}
