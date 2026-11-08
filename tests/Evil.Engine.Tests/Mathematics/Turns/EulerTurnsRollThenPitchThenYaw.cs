using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Turns;

internal sealed class EulerTurnsRollThenPitchThenYaw : Property<EulerTurn>
{
    protected override EulerTurn Generate(Random random) =>
        new EulerTurn(Gen.Euler(random), Gen.Point3(random, 10f));

    protected override void Verify(EulerTurn sample)
    {
        Vector3 rolled = Gen.Rodrigues(sample.Point, Vector3.UnitZ, sample.Degrees.Z);
        Vector3 pitched = Gen.Rodrigues(rolled, Vector3.UnitX, sample.Degrees.X);
        Vector3 expected = Gen.Rodrigues(pitched, Vector3.UnitY, sample.Degrees.Y);
        Check.Near(expected, Rotations.Rotate(sample.Point, Rotations.FromEulerDegrees(sample.Degrees)), "turned point", 1e-3f * (1f + sample.Point.Length()));
    }
}
