using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Turns;

internal sealed class TurningAroundAnAxisMatchesRodrigues : Property<AxisTurn>
{
    protected override AxisTurn Generate(Random random) =>
        new AxisTurn(Gen.Direction(random), Gen.Number(random, -720f, 720f), Gen.Point3(random, 10f));

    protected override void Verify(AxisTurn sample)
    {
        Quaternion turn = Rotations.AroundAxis(sample.Axis, sample.Degrees);
        Check.Near(Gen.Rodrigues(sample.Point, sample.Axis, sample.Degrees), Rotations.Rotate(sample.Point, turn), "turned point", 1e-3f * (1f + sample.Point.Length()));
        Check.Near(sample.Axis, Rotations.Rotate(sample.Axis, turn), "the axis stays put", 1e-4f);
        Check.Near(1f, turn.Length(), "a unit quaternion", 1e-4f);
    }
}
