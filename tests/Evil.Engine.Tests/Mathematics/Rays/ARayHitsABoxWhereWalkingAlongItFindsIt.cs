using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rays;

internal sealed class ARayHitsABoxWhereWalkingAlongItFindsIt : Property<RayAtBox>
{
    protected override RayAtBox Generate(Random random)
    {
        Bounds3 box = Gen.Box(random, 10f);
        Vector3 inside = box.Center + Gen.Point3(random, 1f) * box.Size / 2f;
        return new RayAtBox(box, RayAim.Toward(random, Gen.Point3(random, 30f), inside));
    }

    protected override void Verify(RayAtBox sample)
    {
        float? hit = sample.Ray.Intersect(sample.Box);
        float? walked = RayAim.FirstInside(sample.Ray, sample.Box.Expand(-0.02f), Step, Reach);
        if (hit is not float distance)
        {
            Check.True(walked is null, $"the box was missed, but walking found it {walked} along");
            return;
        }

        Check.True(sample.Box.Expand(1e-3f).Contains(sample.Ray.PointAt(distance)), $"the hit at {distance} is not on the box");
        Check.True(walked is not float inside || distance <= inside + Step, $"the hit at {distance} comes after walking found the box at {walked}");
    }

    private const float Step = 0.01f;
    private const float Reach = 120f;
}
