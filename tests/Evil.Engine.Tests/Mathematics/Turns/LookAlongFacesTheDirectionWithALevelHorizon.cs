using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Turns;

internal sealed class LookAlongFacesTheDirectionWithALevelHorizon : Property<Vector3>
{
    protected override Vector3 Generate(Random random) =>
        Gen.Direction(random) * Gen.Number(random, 0.01f, 100f);

    protected override void Verify(Vector3 sample)
    {
        Quaternion look = Rotations.LookAlong(sample);
        Check.Near(1f, look.Length(), "a unit quaternion", 1e-4f);
        Check.Near(Vector3.Normalize(sample), Rotations.Rotate(Rotations.Forward, look), "forward", 1e-4f);
        if (MathF.Abs(Vector3.Normalize(sample).Y) < 0.99f)
        {
            Check.Near(0f, Rotations.Rotate(Vector3.UnitX, look).Y, "right stays level", 1e-4f);
            Check.True(Rotations.Rotate(Vector3.UnitY, look).Y > 0f, "up stays up");
        }
    }
}
