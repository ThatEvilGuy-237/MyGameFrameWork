namespace OrbitDemo;

internal sealed class SpinningShapes
{
    private readonly Box _red;
    private readonly Box _green;
    private readonly Box _sun;
    private readonly Sphere _planet;

    public SpinningShapes(WorldObject world)
    {
        _red = AddBox(world, new Vector3(1.4f, 1.4f, 1.4f), Color.FromHex("#E5484D"), new Vector3(-5f, 0.7f, -2f));
        _green = AddBox(world, new Vector3(1.2f, 2.4f, 1.2f), Color.FromHex("#30A46C"), new Vector3(5f, 1.2f, -2f));
        _sun = AddBox(world, Vector3.One, Color.FromHex("#3E63DD"), new Vector3(0f, 1f, 1f));
        _planet = AddPlanet(_sun);
    }

    public void Spin(float seconds)
    {
        _red.RotateY(60f * seconds);
        _green.RotateY(-90f * seconds);
        _sun.RotateY(-50f * seconds);
        _planet.RotateY(160f * seconds);
    }

    private static Box AddBox(WorldObject world, Vector3 size, Color color, Vector3 position)
    {
        Box box = world.Add(new Box(size, color));
        box.Position = position;
        return box;
    }

    private static Sphere AddPlanet(Box sun)
    {
        Sphere planet = sun.Add(new Sphere(0.55f, Color.FromHex("#FFC53D")));
        planet.Position = new Vector3(3f, 0f, 0f);

        Torus ring = planet.Add(new Torus(0.8f, 0.95f, Color.FromHex("#E8D9A8")));
        ring.Rotation = new Vector3(20f, 0f, 10f);

        Sphere moon = new Sphere(0.22f, Color.White);
        moon.Material = Material.Glowing(Color.FromHex("#DDEBFF"), 1.5f);
        planet.Add(moon);
        moon.Position = new Vector3(1.4f, 0f, 0f);
        return planet;
    }
}
