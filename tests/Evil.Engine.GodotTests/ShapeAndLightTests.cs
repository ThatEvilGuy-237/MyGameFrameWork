namespace Evil.Engine.GodotTests;

internal static class ShapeAndLightTests
{
    [Test]
    public static void ShapesKeepTheirSizeAndColour()
    {
        Box box = new Box(new Vector3(1f, 2f, 3f), Color.Red);
        Check.Near(new Vector3(1f, 2f, 3f), box.Size, "box size");
        Check.Equal(Color.Red, box.Color, "box colour");
        box.Size = new Vector3(4f, 4f, 4f);
        Check.Near(new Vector3(4f, 4f, 4f), box.Size, "box resized");

        Sphere sphere = new Sphere(0.8f, Color.Blue);
        Check.Near(0.8f, sphere.Radius, "sphere radius");

        Floor floor = new Floor(new Vector2(6f, 8f), Color.Gray);
        Check.Near(new Vector2(6f, 8f), floor.Size, "floor size");

        box.Node.Free();
        sphere.Node.Free();
        floor.Node.Free();
    }

    [Test]
    public static void SeeThroughColoursAndShadows()
    {
        Box glass = new Box(Vector3.One, Color.White.WithAlpha(0.4f));
        Check.Near(0.4f, glass.Color.A, "alpha kept");
        Check.True(glass.CastsShadow, "shapes cast shadows by default");
        glass.CastsShadow = false;
        Check.True(!glass.CastsShadow, "shadow off");
        glass.Node.Free();
    }

    [Test]
    public static void CameraSettingsAndCurrent()
    {
        using Stage stage = Stage.Open();
        Camera camera = new Camera();
        camera.FieldOfView = 50f;
        camera.Near = 0.1f;
        camera.Far = 500f;
        Check.Near(50f, camera.FieldOfView, "field of view");
        Check.Near(0.1f, camera.Near, "near");
        Check.Near(500f, camera.Far, "far");
        Check.True(!camera.IsCurrent, "not current outside the world");
        stage.Root.Add(camera);
        camera.MakeCurrent();
        Check.True(camera.IsCurrent, "current once made so");
    }

    [Test]
    public static void SunShinesWhereItIsTold()
    {
        Sun sun = new Sun();
        Check.Near(Vector3.Normalize(new Vector3(-0.5f, -1f, -0.6f)), sun.Direction, "default direction");
        Check.True(sun.Shadows, "shadows on by default");
        Check.Near(1f, sun.Intensity, "intensity");
        sun.Direction = new Vector3(0f, -3f, 0f);
        Check.Near(new Vector3(0f, -1f, 0f), sun.Direction, "straight down works");
        Check.Throws<ArgumentException>(() => sun.Direction = Vector3.Zero, "direction with a length");
        sun.Node.Free();
    }
}
