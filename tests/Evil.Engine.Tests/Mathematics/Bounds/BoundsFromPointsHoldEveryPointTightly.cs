using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Bounds;

internal sealed class BoundsFromPointsHoldEveryPointTightly : Property<Vector3[]>
{
    protected override Vector3[] Generate(Random random)
    {
        Vector3[] points = new Vector3[random.Next(1, 20)];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Gen.Point3(random, 50f);
        }

        return points;
    }

    protected override void Verify(Vector3[] sample)
    {
        Bounds3 box = Bounds3.FromPoints(sample);
        Vector3 lowest = sample[0];
        Vector3 highest = sample[0];
        foreach (Vector3 point in sample)
        {
            Check.True(box.Contains(point), $"{point} inside");
            lowest = Vector3.Min(lowest, point);
            highest = Vector3.Max(highest, point);
        }

        Check.Equal(lowest, box.Min, "tight below");
        Check.Equal(highest, box.Max, "tight above");
    }
}
