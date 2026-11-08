namespace Evil.Engine.GodotTests.Scenes;

internal sealed class MapAndBattle
{
    public MapAndBattle(Stage stage)
    {
        Scenes = new SceneStack(null, stage.Root);
        Map = new ProbeScene("map", Log);
        Battle = new ProbeScene("battle", Log);
    }

    public List<string> Log { get; } = new List<string>();

    public SceneStack Scenes { get; }

    public ProbeScene Map { get; }

    public ProbeScene Battle { get; }

    public string Hooks => string.Join(' ', Log);
}
