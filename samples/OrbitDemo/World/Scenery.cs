namespace OrbitDemo;

internal static class Scenery
{
    public static void Build(WorldObject world, ContentLibrary content)
    {
        world.Add(Sun.Golden());
        AddTiledFloor(world, content.Texture("textures/tiles.png"));
        AddTowers(world, content);
        AddTorchPillar(world, -7f);
        AddTorchPillar(world, 7f);
    }

    private static void AddTiledFloor(WorldObject world, Texture tiles)
    {
        Floor floor = world.Add(new Floor(new Vector2(24f, 24f), Color.White));
        floor.Material.Texture = tiles;
        floor.Material.TextureScale = new Vector2(12f, 12f);
    }

    private static void AddTowers(WorldObject world, ContentLibrary content)
    {
        Model watchtower = world.Add(content.Model("models/tower.glb"));
        watchtower.Position = new Vector3(-5.5f, 0f, 3.2f);

        Model outpost = world.Add(content.Model("models/tower.glb"));
        outpost.Position = new Vector3(5.5f, 0f, 3.2f);
        outpost.RotateY(180f);
        outpost.Part("Flag")!.Material = Material.Matte(Color.FromHex("#E5484D"));
    }

    private static void AddTorchPillar(WorldObject world, float x)
    {
        Cylinder pillar = new Cylinder(0.45f, 3f, Color.White);
        pillar.Material = Material.Matte(Color.FromHex("#8C8A80"));
        world.Add(pillar);
        pillar.Position = new Vector3(x, 1.5f, -5f);

        Sphere flame = new Sphere(0.18f, Color.White);
        flame.Material = Material.Glowing(Color.FromHex("#FFB45E"), 2.5f);
        flame.CastsShadow = false;
        pillar.Add(flame);
        flame.Position = new Vector3(0f, 1.75f, 0f);
        flame.Add(Lamp.Warm(7f));
    }
}
