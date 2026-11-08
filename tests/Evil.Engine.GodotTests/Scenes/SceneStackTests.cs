namespace Evil.Engine.GodotTests.Scenes;

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
        MapAndBattle game = new MapAndBattle(stage);
        game.Scenes.Push(game.Map);
        game.Scenes.ApplyChanges();
        game.Scenes.Push(game.Battle);
        game.Scenes.ApplyChanges();
        Check.True(!game.Map.OwnWorld.Visible, "map hidden");
        Check.True(game.Battle.OwnWorld.Visible, "battle shown");
        Check.True(game.Battle.OwnCamera.IsCurrent, "battle camera on screen");
        Check.Equal("map.enter map.pause battle.enter", game.Hooks, "hooks");
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
        MapAndBattle game = new MapAndBattle(stage);
        game.Scenes.Push(game.Map);
        game.Scenes.Push(game.Battle);
        game.Scenes.ApplyChanges();
        WorldObject battleWorld = game.Battle.OwnWorld;
        game.Scenes.Pop();
        game.Scenes.ApplyChanges();
        Check.Equal(1, stage.Root.Children.Count, "battle world removed");
        Check.True(battleWorld.Node.IsQueuedForDeletion(), "battle world freed");
        Check.True(game.Map.OwnWorld.Visible, "map shown again");
        Check.True(game.Map.OwnCamera.IsCurrent, "map camera on screen again");
        void ReadTheWorldOffTheStack() => _ = game.Battle.OwnWorld;
        Check.Throws<InvalidOperationException>(ReadTheWorldOffTheStack, "not on the scene stack");
        Check.Equal("map.enter map.pause battle.enter battle.exit map.resume", game.Hooks, "hooks");
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
        Check.Throws<InvalidOperationException>(scenes.ApplyChanges, "already on the scene stack. A scene object can be there once: make a new one to show it again.");
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

    [Test]
    public static void PoppingAnEmptyStackSaysWhatToDo()
    {
        using Stage stage = Stage.Open();
        SceneStack scenes = new SceneStack(null, stage.Root);
        scenes.Pop();
        Check.Throws<InvalidOperationException>(scenes.ApplyChanges, "Push a scene first, or call Game.Exit() to end the game.");
    }
}
