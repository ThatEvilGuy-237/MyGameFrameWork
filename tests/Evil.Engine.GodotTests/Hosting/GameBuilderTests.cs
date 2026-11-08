namespace Evil.Engine.GodotTests.Hosting;

internal static class GameBuilderTests
{
    [Test]
    public static void BuildNeedsAFirstScene()
    {
        void BuildWithoutAScene() => GameBuilder.Create("test", 320, 240).Build();
        Check.Throws<InvalidOperationException>(BuildWithoutAScene, "GameBuilder.Start was not called");
    }

    [Test]
    public static void BuildRejectsABadWindow()
    {
        Check.Throws<InvalidOperationException>(
            () => GameBuilder.Create("test", 0, 240).Start(new EmptyScene()).Build(),
            "window size must be positive");
        Check.Throws<InvalidOperationException>(
            () => GameBuilder.Create(" ", 320, 240).Start(new EmptyScene()).Build(),
            "window title is empty");
    }

    [Test]
    public static void CaptureAndExitNeedsAPathAndAFrame()
    {
        Check.Throws<InvalidOperationException>(
            () => GameBuilder.Create("test", 320, 240).Start(new EmptyScene()).CaptureAndExit("", 10).Build(),
            "GameBuilder.CaptureAndExit");
        Check.Throws<InvalidOperationException>(
            () => GameBuilder.Create("test", 320, 240).Start(new EmptyScene()).CaptureAndExit("shot.png", -1).Build(),
            "GameBuilder.CaptureAndExit");
    }

    [Test]
    public static void BuildKeepsTheValuesItWasGiven()
    {
        WindowSettings window = new WindowSettings("test", 320, 240, VSync: false);
        GameHost game = GameBuilder.Create(window)
            .ClearColor(Color.Black)
            .Start(new EmptyScene())
            .CaptureAndExit("shots/one.png", 5)
            .Build();
        Check.Equal(window, game.Window, "window");
        Check.Equal(Color.Black, game.Options.ClearColor, "clear colour");
        Check.Equal(Path.GetFullPath("shots/one.png"), game.Options.Snapshot!.Path, "capture path made absolute");
    }

    [Test]
    public static void ContextQueuesCapturesAndExit()
    {
        using Stage stage = Stage.Open();
        GameContext context = new GameContext(stage.Root);
        context.CaptureFrame("a.png");
        Check.True(context.HasPendingCaptures, "capture waiting");
        Check.True(context.TryTakeCapture(out string? path) && path == Path.GetFullPath("a.png"), "absolute path");
        Check.True(!context.IsExiting, "running");
        context.Exit();
        Check.True(context.IsExiting, "exiting");
    }
}
