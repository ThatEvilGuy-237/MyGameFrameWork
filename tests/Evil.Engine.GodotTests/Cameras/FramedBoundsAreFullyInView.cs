using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Cameras;

internal sealed class FramedBoundsAreFullyInView : Property<FramingProbe>
{
    private Stage? _stage;

    public override void Execute()
    {
        using Stage stage = Stage.Open();
        _stage = stage;
        base.Execute();
    }

    protected override FramingProbe Generate(Random random)
    {
        Vector3 size = new Vector3(Gen.Number(random, 0.2f, 30f), Gen.Number(random, 0f, 10f), Gen.Number(random, 0.2f, 30f));
        Bounds3 bounds = Bounds3.FromCenter(Gen.Point3(random, 20f), size);
        return new FramingProbe(bounds, Gen.Number(random, 5f, 85f), Gen.Number(random, -180f, 180f), Gen.Number(random, 30f, 100f), Gen.Chance(random, 0.25));
    }

    protected override void Verify(FramingProbe sample)
    {
        Camera camera = _stage!.AddCamera(new Vector3(0f, 0f, 10f));
        camera.FieldOfView = sample.FieldOfView;
        camera.Projection = sample.Orthographic ? CameraProjection.Orthographic : CameraProjection.Perspective;
        camera.Frame(sample.Bounds, sample.Pitch, sample.Yaw);
        Check.True(camera.CanSee(sample.Bounds.Center), "the middle is in view");
        foreach (Vector3 corner in Corners(sample.Bounds))
        {
            Check.True(camera.CanSee(corner), $"corner {corner} is in view");
        }

        camera.Destroy();
    }

    private static Vector3[] Corners(Bounds3 box) =>
    [
        new Vector3(box.Min.X, box.Min.Y, box.Min.Z), new Vector3(box.Max.X, box.Min.Y, box.Min.Z),
        new Vector3(box.Min.X, box.Max.Y, box.Min.Z), new Vector3(box.Max.X, box.Max.Y, box.Min.Z),
        new Vector3(box.Min.X, box.Min.Y, box.Max.Z), new Vector3(box.Max.X, box.Min.Y, box.Max.Z),
        new Vector3(box.Min.X, box.Max.Y, box.Max.Z), new Vector3(box.Max.X, box.Max.Y, box.Max.Z),
    ];
}
