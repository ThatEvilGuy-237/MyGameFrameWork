using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Cameras;

internal static class CameraTests
{
    [Test]
    public static void TheMiddleOfTheScreenLooksForward()
    {
        using Stage stage = Stage.Open();
        Camera camera = stage.AddCamera(new Vector3(0f, 5f, 10f));
        Vector2 middle = Stage.ScreenMiddle;
        Ray3 ray = camera.ScreenToRay(middle);
        Check.Near(camera.Forward, ray.Direction, "ray along the view");
        Check.Near(Vector3.Zero, camera.PointOnGround(middle) ?? new Vector3(99f), "hits the ground under the middle", 0.01f);
        Check.Near(middle, camera.WorldToScreen(Vector3.Zero), "and back to the middle", 0.5f);
    }

    [Test]
    public static void CanSeeWhatIsInFront()
    {
        using Stage stage = Stage.Open();
        Camera camera = stage.AddCamera(new Vector3(0f, 5f, 10f));
        Check.True(camera.CanSee(Vector3.Zero), "the target");
        Check.True(!camera.CanSee(new Vector3(0f, 5f, 20f)), "behind the camera");
    }

    [Test]
    public static void OrthographicRaysAreParallel()
    {
        using Stage stage = Stage.Open();
        Camera camera = stage.AddCamera(new Vector3(0f, 5f, 10f));
        camera.Projection = CameraProjection.Orthographic;
        camera.OrthographicSize = 10f;
        Check.Equal(CameraProjection.Orthographic, camera.Projection, "projection");
        Ray3 left = camera.ScreenToRay(Stage.ScreenMiddle - new Vector2(100f, 0f));
        Ray3 right = camera.ScreenToRay(Stage.ScreenMiddle + new Vector2(100f, 0f));
        Check.Near(left.Direction, right.Direction, "same direction");
        Check.True(Vector3.Distance(left.Origin, right.Origin) > 0.1f, "different start points");
    }

    [Test]
    public static void ScreenCallsNeedTheWorld()
    {
        Camera camera = new Camera();
        void RayFromALooseCamera() => camera.ScreenToRay(Vector2.Zero);
        Check.Throws<InvalidOperationException>(RayFromALooseCamera, "needs the camera in a scene's world");
        camera.Destroy();
    }

    [Test]
    public static void OrbitPlacesTheCameraAndLooksAtTheTarget()
    {
        using Stage stage = Stage.Open();
        Camera camera = stage.AddCamera(Vector3.Zero);
        OrbitCamera orbit = new OrbitCamera(camera);
        orbit.Target = new Vector3(1f, 0f, 2f);
        orbit.Pitch = 45f;
        orbit.Distance = 10f;
        orbit.Snap();
        Check.Near(new Vector3(1f, 7.0711f, 9.0711f), camera.WorldPosition, "position");
        Check.Near(Vector3.Normalize(orbit.Target - camera.WorldPosition), camera.Forward, "facing the target");
    }

    [Test]
    public static void OrbitGlidesToNewSettings()
    {
        using Stage stage = Stage.Open();
        Camera camera = stage.AddCamera(Vector3.Zero);
        OrbitCamera orbit = new OrbitCamera(camera, new OrbitLimits(MinPitch: 0f));
        orbit.Pitch = 0f;
        orbit.Distance = 10f;
        orbit.Snap();
        orbit.Turn(90f);
        orbit.Update(0.05f);
        Vector3 partway = camera.WorldPosition;
        Check.True(partway.X > 0.5f && partway.X < 9.5f, $"partway round after one frame, got {partway}");
        orbit.Update(5f);
        Check.Near(new Vector3(10f, 0f, 0f), camera.WorldPosition, "arrived", 0.01f);
    }

    [Test]
    public static void OrbitStaysInsideItsLimits()
    {
        Camera camera = new Camera();
        OrbitCamera orbit = new OrbitCamera(camera);
        orbit.Distance = 1000f;
        Check.Near(orbit.Limits.MaxDistance, orbit.Distance, "not further than the max");
        orbit.Zoom(0.0001f);
        Check.Near(orbit.Limits.MinDistance, orbit.Distance, "not closer than the min");
        orbit.Pitch = 120f;
        Check.Near(orbit.Limits.MaxPitch, orbit.Pitch, "not past straight down");
        orbit.Limits = orbit.Limits with { MinDistance = 5f };
        Check.Near(5f, orbit.Distance, "a new limit pulls the distance in");
        void ZoomToNothing() => orbit.Zoom(0f);
        Check.Throws<ArgumentOutOfRangeException>(ZoomToNothing, "factor above 0");
        void SwapTheDistanceLimits() => orbit.Limits = new OrbitLimits(MinDistance: 10f, MaxDistance: 5f);
        Check.Throws<ArgumentException>(SwapTheDistanceLimits, "distance range");
        void TiltPastStraightUp() => _ = new OrbitCamera(camera, new OrbitLimits(MaxPitch: 120f));
        Check.Throws<ArgumentException>(TiltPastStraightUp, "pitch range");
        camera.Destroy();
    }

    [Test]
    public static void PanMovesAlongTheGroundTheWayTheCameraFaces()
    {
        Camera camera = new Camera();
        OrbitCamera orbit = new OrbitCamera(camera);
        orbit.Yaw = 90f;
        orbit.Pan(new Vector2(0f, 1f));
        Check.Near(new Vector3(-1f, 0f, 0f), orbit.Target, "ahead is where the camera looks");
        orbit.Pan(new Vector2(1f, 0f));
        Check.Near(new Vector3(-1f, 0f, -1f), orbit.Target, "right is to the camera's right");
        camera.Destroy();
    }
}
