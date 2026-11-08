using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal sealed class PosesMatchGodotsTransforms : Property<PoseProbe>
{
    protected override PoseProbe Generate(Random random) => new PoseProbe(Gen.Placement(random, false), Gen.Point3(random, 20f));

    protected override void Verify(PoseProbe sample)
    {
        Godot.Basis basis = new Godot.Basis(sample.Pose.Rotation.ToGodot()) * Godot.Basis.FromScale(sample.Pose.Scale.ToGodot());
        Godot.Transform3D transform = new Godot.Transform3D(basis, sample.Pose.Position.ToGodot());
        Vector3 godots = (transform * sample.Point.ToGodot()).ToEvil();
        Check.Near(godots, sample.Pose.ToWorld(sample.Point), "to world", 1e-3f * (1f + godots.Length()));
        Vector3 back = (transform.AffineInverse() * sample.Point.ToGodot()).ToEvil();
        Check.Near(back, sample.Pose.ToLocal(sample.Point), "to local", 1e-3f * (1f + sample.Point.Length()));
    }
}
