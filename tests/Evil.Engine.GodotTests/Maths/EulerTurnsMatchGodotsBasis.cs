using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal sealed class EulerTurnsMatchGodotsBasis : Property<EulerProbe>
{
    protected override EulerProbe Generate(Random random) => new EulerProbe(Gen.Euler(random), Gen.Point3(random, 10f));

    protected override void Verify(EulerProbe sample)
    {
        Godot.Vector3 radians = new Godot.Vector3(Godot.Mathf.DegToRad(sample.Degrees.X), Godot.Mathf.DegToRad(sample.Degrees.Y), Godot.Mathf.DegToRad(sample.Degrees.Z));
        Vector3 godots = (Godot.Basis.FromEuler(radians) * sample.Point.ToGodot()).ToEvil();
        Vector3 ours = Rotations.Rotate(sample.Point, Rotations.FromEulerDegrees(sample.Degrees));
        Check.Near(godots, ours, "turned point", 1e-3f * (1f + sample.Point.Length()));
    }
}
