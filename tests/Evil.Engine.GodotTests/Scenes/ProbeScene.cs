namespace Evil.Engine.GodotTests.Scenes;

internal sealed class ProbeScene(string name, List<string> log, bool overlay = false) : Scene
{
    public override bool IsOverlay => overlay;

    public WorldObject OwnWorld => World;

    public Camera OwnCamera => Camera;

    protected override void Enter()
    {
        log.Add($"{name}.enter");
        World.Add(new Box());
    }

    protected override void Exit() => log.Add($"{name}.exit");

    protected override void Pause() => log.Add($"{name}.pause");

    protected override void Resume() => log.Add($"{name}.resume");
}
