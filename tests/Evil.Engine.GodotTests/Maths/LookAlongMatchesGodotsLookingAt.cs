using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal sealed class LookAlongMatchesGodotsLookingAt : Property<Vector3>
{
    protected override Vector3 Generate(Random random) => Gen.Direction(random);

    protected override void Verify(Vector3 sample)
    {
        if (MathF.Abs(sample.Y) > 0.99f)
        {
            return;
        }

        Godot.Basis godots = Godot.Basis.LookingAt(sample.ToGodot(), Godot.Vector3.Up);
        Quaternion ours = Rotations.LookAlong(sample);
        Check.Near((godots * new Godot.Vector3(0f, 0f, -1f)).ToEvil(), Rotations.Rotate(Rotations.Forward, ours), "forward", 1e-4f);
        Check.Near((godots * Godot.Vector3.Right).ToEvil(), Rotations.Rotate(Vector3.UnitX, ours), "right", 1e-4f);
    }
}
