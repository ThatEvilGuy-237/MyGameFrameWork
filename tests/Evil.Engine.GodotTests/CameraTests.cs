namespace Evil.Engine.GodotTests;

internal static class CameraTests
{
    [Test]
    public static void TheMiddleOfTheScreenLooksForward()
    {
        using var stage = Stage.Open();
        var camera = OnStage(stage, new Vector3(0f, 5f, 10f));
        var middle = Middle();
        var ray = camera.ScreenToRay(middle);
        Check.Near(camera.Forward, ray.Direction, "ray along the view");
        Check.Near(Vector3.Zero, camera.PointOnGround(middle) ?? new Vector3(99f), "hits the ground under the middle", 0.01f);
        Check.Near(middle, camera.WorldToScreen(Vector3.Zero), "and back to the middle", 0.5f);
    }

    [Test]
    public static void CanSeeWhatIsInFront()
    {
        using var stage = Stage.Open();
        var camera = OnStage(stage, new Vector3(0f, 5f, 10f));
        Check.True(camera.CanSee(Vector3.Zero), "the target");
        Check.True(!camera.CanSee(new Vector3(0f, 5f, 20f)), "behind the camera");
    }

    [Test]
    public static void OrthographicRaysAreParallel()
    {
        using var stage = Stage.Open();
        var camera = OnStage(stage, new Vector3(0f, 5f, 10f));
        camera.Projection = CameraProjection.Orthographic;
        camera.OrthographicSize = 10f;
        Check.Equal(CameraProjection.Orthographic, camera.Projection, "projection");
        var left = camera.ScreenToRay(Middle() - new Vector2(100f, 0f));
        var right = camera.ScreenToRay(Middle() + new Vector2(100f, 0f));
        Check.Near(left.Direction, right.Direction, "same direction");
        Check.True(Vector3.Distance(left.Origin, right.Origin) > 0.1f, "different start points");
    }

    [Test]
    public static void ScreenCallsNeedTheWorld()
    {
        var camera = new Camera();
        Check.Throws<InvalidOperationException>(() => camera.ScreenToRay(Vector2.Zero), "needs the camera in a scene's world");
        camera.Destroy();
    }

    [Test]
    public static void OrbitPlacesTheCameraAndLooksAtTheTarget()
    {
        using var stage = Stage.Open();
        var camera = OnStage(stage, Vector3.Zero);
        var orbit = new OrbitCamera(camera) { Target = new Vector3(1f, 0f, 2f), Pitch = 45f, Distance = 10f };
        orbit.Snap();
        Check.Near(new Vector3(1f, 7.0711f, 9.0711f), camera.WorldPosition, "position");
        Check.Near(Vector3.Normalize(orbit.Target - camera.WorldPosition), camera.Forward, "facing the target");
    }

    [Test]
    public static void OrbitGlidesToNewSettings()
    {
        using var stage = Stage.Open();
        var camera = OnStage(stage, Vector3.Zero);
        var orbit = new OrbitCamera(camera, new OrbitLimits(MinPitch: 0f)) { Pitch = 0f, Distance = 10f };
        orbit.Snap();
        orbit.Turn(90f);
        orbit.Update(0.05f);
        var partway = camera.WorldPosition;
        Check.True(partway.X > 0.5f && partway.X < 9.5f, $"partway round after one frame, got {partway}");
        orbit.Update(5f);
        Check.Near(new Vector3(10f, 0f, 0f), camera.WorldPosition, "arrived", 0.01f);
    }

    [Test]
    public static void OrbitStaysInsideItsLimits()
    {
        var camera = new Camera();
        var orbit = new OrbitCamera(camera) { Distance = 1000f };
        Check.Near(orbit.Limits.MaxDistance, orbit.Distance, "not further than the max");
        orbit.Zoom(0.0001f);
        Check.Near(orbit.Limits.MinDistance, orbit.Distance, "not closer than the min");
        orbit.Pitch = 120f;
        Check.Near(orbit.Limits.MaxPitch, orbit.Pitch, "not past straight down");
        orbit.Limits = orbit.Limits with { MinDistance = 5f };
        Check.Near(5f, orbit.Distance, "a new limit pulls the distance in");
        Check.Throws<ArgumentOutOfRangeException>(() => orbit.Zoom(0f), "factor above 0");
        Check.Throws<ArgumentException>(() => orbit.Limits = new OrbitLimits(MinDistance: 10f, MaxDistance: 5f), "distance range");
        Check.Throws<ArgumentException>(() => _ = new OrbitCamera(camera, new OrbitLimits(MaxPitch: 120f)), "pitch range");
        camera.Destroy();
    }

    [Test]
    public static void PanMovesAlongTheGroundTheWayTheCameraFaces()
    {
        var camera = new Camera();
        var orbit = new OrbitCamera(camera) { Yaw = 90f };
        orbit.Pan(new Vector2(0f, 1f));
        Check.Near(new Vector3(-1f, 0f, 0f), orbit.Target, "ahead is where the camera looks");
        orbit.Pan(new Vector2(1f, 0f));
        Check.Near(new Vector3(-1f, 0f, -1f), orbit.Target, "right is to the camera's right");
        camera.Destroy();
    }

    private static Camera OnStage(Stage stage, Vector3 position)
    {
        var camera = stage.Root.Add(new Camera { Position = position });
        camera.LookAt(Vector3.Zero);
        camera.MakeCurrent();
        return camera;
    }

    private static Vector2 Middle() => Stage.Host!.GetViewport().GetVisibleRect().Size.ToEvil() / 2f;
}
