namespace Evil.Engine.GodotTests;

internal static class SceneStackTests
{
    [Test]
    public static void EachSceneGetsAWorldAndACamera()
    {
        using Stage stage = Stage.Open();
        SceneStack scenes = new SceneStack(null, stage.Root);
        ProbeScene map = new ProbeScene("map", []);
        scenes.Push(map);
        scenes.ApplyChanges();
        Check.Equal(1, stage.Root.Children.Count, "one world");
        Check.True(ReferenceEquals(stage.Root, map.OwnWorld.Parent), "the world sits under the worlds root");
        Check.Equal(2, map.OwnWorld.Children.Count, "camera plus what Enter added");
        Check.True(map.OwnCamera.IsCurrent, "its camera is on screen");
    }

    [Test]
    public static void AFullSceneHidesTheOneBelow()
    {
        using Stage stage = Stage.Open();
        List<string> log = new List<string>();
        SceneStack scenes = new SceneStack(null, stage.Root);
        ProbeScene map = new ProbeScene("map", log);
        ProbeScene battle = new ProbeScene("battle", log);
        scenes.Push(map);
        scenes.ApplyChanges();
        scenes.Push(battle);
        scenes.ApplyChanges();
        Check.True(!map.OwnWorld.Visible, "map hidden");
        Check.True(battle.OwnWorld.Visible, "battle shown");
        Check.True(battle.OwnCamera.IsCurrent, "battle camera on screen");
        Check.Equal("map.enter map.pause battle.enter", string.Join(' ', log), "hooks");
    }

    [Test]
    public static void AnOverlayKeepsTheWorldBelowAndItsCamera()
    {
        using Stage stage = Stage.Open();
        SceneStack scenes = new SceneStack(null, stage.Root);
        ProbeScene battle = new ProbeScene("battle", []);
        ProbeScene menu = new ProbeScene("menu", [], overlay: true);
        scenes.Push(battle);
        scenes.Push(menu);
        scenes.ApplyChanges();
        Check.True(battle.OwnWorld.Visible, "battle still shown");
        Check.True(menu.OwnWorld.Visible, "menu shown");
        Check.True(battle.OwnCamera.IsCurrent, "the screen still looks through the battle camera");
    }

    [Test]
    public static void PopFreesTheWorldAndGivesTheViewBack()
    {
        using Stage stage = Stage.Open();
        List<string> log = new List<string>();
        SceneStack scenes = new SceneStack(null, stage.Root);
        ProbeScene map = new ProbeScene("map", log);
        ProbeScene battle = new ProbeScene("battle", log);
        scenes.Push(map);
        scenes.Push(battle);
        scenes.ApplyChanges();
        WorldObject battleWorld = battle.OwnWorld;
        scenes.Pop();
        scenes.ApplyChanges();
        Check.Equal(1, stage.Root.Children.Count, "battle world removed");
        Check.True(battleWorld.Node.IsQueuedForDeletion(), "battle world freed");
        Check.True(map.OwnWorld.Visible, "map shown again");
        Check.True(map.OwnCamera.IsCurrent, "map camera on screen again");
        Check.Throws<InvalidOperationException>(() => _ = battle.OwnWorld, "not on the scene stack");
        Check.Equal("map.enter map.pause battle.enter battle.exit map.resume", string.Join(' ', log), "hooks");
    }

    [Test]
    public static void SwitchReplacesTheTopScene()
    {
        using Stage stage = Stage.Open();
        List<string> log = new List<string>();
        SceneStack scenes = new SceneStack(null, stage.Root);
        scenes.Push(new ProbeScene("title", log));
        scenes.ApplyChanges();
        scenes.Switch(new ProbeScene("map", log));
        scenes.ApplyChanges();
        Check.Equal(1, scenes.Count, "one scene");
        Check.Equal("title.enter title.exit map.enter", string.Join(' ', log), "hooks");
    }

    [Test]
    public static void BadChangesThrow()
    {
        using Stage stage = Stage.Open();
        SceneStack scenes = new SceneStack(null, stage.Root);
        scenes.Pop();
        Check.Throws<InvalidOperationException>(scenes.ApplyChanges, "no scene on the stack");
        ProbeScene map = new ProbeScene("map", []);
        scenes.Push(map);
        scenes.Push(map);
        Check.Throws<InvalidOperationException>(scenes.ApplyChanges, "already on the scene stack");
    }

    [Test]
    public static void ExitAllRunsExitTopDown()
    {
        using Stage stage = Stage.Open();
        List<string> log = new List<string>();
        SceneStack scenes = new SceneStack(null, stage.Root);
        scenes.Push(new ProbeScene("map", log));
        scenes.Push(new ProbeScene("battle", log));
        scenes.ApplyChanges();
        log.Clear();
        scenes.ExitAll();
        Check.Equal("battle.exit map.exit", string.Join(' ', log), "hooks");
        Check.Equal(0, scenes.Count, "empty");
    }

    private sealed class ProbeScene(string name, List<string> log, bool overlay = false) : Scene
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
}
