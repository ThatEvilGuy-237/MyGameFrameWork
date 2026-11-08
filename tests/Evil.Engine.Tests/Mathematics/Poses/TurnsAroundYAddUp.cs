using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Poses;

internal sealed class TurnsAroundYAddUp : Property<PoseTurns>
{
    protected override PoseTurns Generate(Random random) =>
        new PoseTurns(Gen.Placement(random, true), Gen.Number(random, -360f, 360f), Gen.Number(random, -360f, 360f));

    protected override void Verify(PoseTurns sample)
    {
        Pose twice = sample.Pose.TurnedY(sample.First).TurnedY(sample.Second);
        Pose once = sample.Pose.TurnedY(sample.First + sample.Second);
        Check.Near(once.Forward, twice.Forward, "forward", 1e-3f);
        Check.Near(once.Up, twice.Up, "up", 1e-3f);
    }
}
