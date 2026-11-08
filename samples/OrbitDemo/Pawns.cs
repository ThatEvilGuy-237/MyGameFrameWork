namespace OrbitDemo;

internal sealed class Pawns
{
    private readonly List<Capsule> _all = [];
    private readonly Spotlight _spot;
    private readonly Torus _ring;

    public Pawns(WorldObject world)
    {
        _all.Add(AddPawn(world, "pawn-red", new Vector3(-2.5f, 0f, 3.5f), Color.FromHex("#E5484D")));
        _all.Add(AddPawn(world, "pawn-blue", new Vector3(2.5f, 0f, 3.5f), Color.FromHex("#3E63DD")));
        _spot = AddSelectionSpot(world);
        _ring = AddSelectionRing(world);
    }

    public Capsule? Hovered { get; private set; }

    public Capsule? Selected { get; private set; }

    public Capsule EnemyOf(Capsule pawn) => _all.First(other => !ReferenceEquals(other, pawn));

    public Capsule? Find(WorldObject? picked)
    {
        for (WorldObject? current = picked; current is not null; current = current.Parent)
        {
            if (current is Capsule pawn && _all.Contains(pawn))
            {
                return pawn;
            }
        }

        return null;
    }

    public void Hover(Capsule? pawn, Vector2 mouse)
    {
        if (ReferenceEquals(pawn, Hovered))
        {
            return;
        }

        ClearHover();
        Hovered = pawn;
        if (pawn is not null)
        {
            pawn.Material.Glow = pawn.Color;
            pawn.Material.GlowStrength = 0.35f;
        }

        Console.WriteLine($"hover: {pawn?.Tag ?? "nothing"} at {(int)mouse.X},{(int)mouse.Y}");
    }

    public void ClearHover()
    {
        if (Hovered is not null)
        {
            Hovered.Material.Glow = Color.Black;
            Hovered = null;
        }
    }

    public void Select(Capsule pawn)
    {
        Selected = pawn;
        _ring.Visible = true;
        FollowSelection();
        Console.WriteLine($"selected: {pawn.Tag}");
    }

    public void FollowSelection()
    {
        if (Selected is null)
        {
            return;
        }

        Vector3 feet = Selected.WorldPosition with { Y = 0.05f };
        _ring.Position = feet;
        _spot.Position = feet + new Vector3(0f, 9f, 0f);
    }

    private static Capsule AddPawn(WorldObject world, string name, Vector3 place, Color team)
    {
        Capsule pawn = new Capsule(0.35f, 1.4f, team);
        pawn.Name = name;
        pawn.Tag = name;
        world.Add(pawn);
        pawn.Position = place + new Vector3(0f, 0.7f, 0f);

        Cone hat = pawn.Add(new Cone(0.3f, 0.45f, Color.FromHex("#F0F0F0")));
        hat.Position = new Vector3(0f, 0.9f, 0f);
        return pawn;
    }

    private static Spotlight AddSelectionSpot(WorldObject world)
    {
        Spotlight spot = new Spotlight(Color.FromHex("#CFE3FF"), 14f, 24f);
        spot.Shadows = true;
        spot.Intensity = 2f;
        world.Add(spot);
        spot.Position = new Vector3(0f, 9f, 3.5f);
        return spot;
    }

    private static Torus AddSelectionRing(WorldObject world)
    {
        Torus ring = new Torus(0.5f, 0.65f, Color.White);
        ring.Material = Material.Glowing(Color.FromHex("#FFC53D"), 1.5f);
        ring.CastsShadow = false;
        ring.Pickable = false;
        ring.Visible = false;
        return world.Add(ring);
    }
}
