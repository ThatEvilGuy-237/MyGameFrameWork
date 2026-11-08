using Evil.Engine.Hosting;
using Evil.Engine.Scenes;

namespace Evil.Engine.Tests;

internal static class HostingTests
{
    [Test]
    public static void BuildNeedsAFirstScene()
    {
        var builder = GameBuilder.Create("test", 320, 240);
        Check.Throws<InvalidOperationException>(() => builder.Build(), "GameBuilder.Start was not called");
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
        Check.Throws<InvalidOperationException>(
            () => GameBuilder.Create(new WindowSettings("test", UpdatesPerSecond: -1)).Start(new EmptyScene()).Build(),
            "UpdatesPerSecond");
    }

    [Test]
    public static void BuildRejectsAMissingContentFolder()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"evil-engine-missing-{Guid.NewGuid():N}");
        Check.Throws<InvalidOperationException>(
            () => GameBuilder.Create("test", 320, 240).Content(missing).Start(new EmptyScene()).Build(),
            "GameBuilder.Content");
    }

    [Test]
    public static void BuildKeepsTheValuesItWasGiven()
    {
        var game = GameBuilder.Create(new WindowSettings("test", 320, 240, VSync: false))
            .Content(AppContext.BaseDirectory)
            .Start(new EmptyScene())
            .Build();
        Check.Equal(new WindowSettings("test", 320, 240, VSync: false), game.Window, "window");
        Check.Equal(Path.GetFullPath(AppContext.BaseDirectory), game.ContentRoot, "content root");
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

    private sealed class EmptyScene : Scene;
}
