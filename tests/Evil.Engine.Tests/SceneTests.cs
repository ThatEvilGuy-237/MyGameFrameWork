using System.Numerics;
using Evil.Engine.Graphics;
using Evil.Engine.Hosting;
using Evil.Engine.Mathematics;
using Evil.Engine.Scenes;

namespace Evil.Engine.Tests;

internal static class SceneTests
{
    [Test]
    public static void PushPopAndSwitchCallTheHooksInOrder()
    {
        var log = new List<string>();
        var scenes = new SceneStack(null);
        scenes.Push(new LoggingScene("map", log));
        scenes.ApplyChanges();
        scenes.Push(new LoggingScene("battle", log));
        scenes.ApplyChanges();
        scenes.Pop();
        scenes.ApplyChanges();
        scenes.Switch(new LoggingScene("menu", log));
        scenes.ApplyChanges();
        Check.Equal(
            "map.enter map.pause battle.enter battle.exit map.resume map.exit menu.enter",
            string.Join(' ', log),
            "hooks");
        Check.Equal(1, scenes.Count, "scenes left");
    }

    [Test]
    public static void ChangesWaitForTheEndOfTheFrame()
    {
        var scenes = new SceneStack(null);
        scenes.Push(new LoggingScene("map", []));
        Check.Equal(0, scenes.Count, "nothing changes mid-frame");
        Check.True(scenes.HasPendingChanges, "pending");
        scenes.ApplyChanges();
        Check.Equal(1, scenes.Count, "applied");
    }

    [Test]
    public static void BadChangesThrow()
    {
        var scenes = new SceneStack(null);
        scenes.Pop();
        Check.Throws<InvalidOperationException>(scenes.ApplyChanges, "no scene on the stack");
        var map = new LoggingScene("map", []);
        scenes.Push(map);
        scenes.Push(map);
        Check.Throws<InvalidOperationException>(scenes.ApplyChanges, "already on the scene stack");
    }

    [Test]
    public static void OverlaysDrawOnTopOfTheSceneBelow()
    {
        var log = new List<string>();
        var scenes = new SceneStack(null);
        scenes.Push(new LoggingScene("map", log));
        scenes.Push(new LoggingScene("pause", log, overlay: true));
        scenes.ApplyChanges();
        log.Clear();
        var (canvas, _, _) = Fakes.Canvas();
        scenes.Draw(canvas);
        Check.Equal("map.draw pause.draw", string.Join(' ', log), "both draw, bottom first");

        scenes.Push(new LoggingScene("battle", log));
        scenes.ApplyChanges();
        log.Clear();
        scenes.Draw(canvas);
        Check.Equal("battle.draw", string.Join(' ', log), "a full scene hides what is below");
    }

    [Test]
    public static void AnOverlayStartsFromAClearTransform()
    {
        var scenes = new SceneStack(null);
        scenes.Push(new CameraScene());
        scenes.Push(new LoggingScene("pause", [], overlay: true));
        scenes.ApplyChanges();
        var (canvas, sink, _) = Fakes.Canvas();
        scenes.Draw(canvas);
        Check.Near(Vector2.Zero, sink.Batches[^1].Vertices[0].Position, "the overlay is not moved by the camera below");
    }

    [Test]
    public static void ASceneThatForgetsPopIsNamed()
    {
        var scenes = new SceneStack(null);
        scenes.Push(new LeakyScene());
        scenes.ApplyChanges();
        var (canvas, _, _) = Fakes.Canvas();
        Check.Throws<InvalidOperationException>(() => scenes.Draw(canvas), "LeakyScene.Draw left 1 Push()");
    }

    [Test]
    public static void GameIsOnlyThereWhileOnTheStack()
    {
        var scene = new LoggingScene("map", []);
        Check.Throws<InvalidOperationException>(() => scene.ReadGame(), "not on the scene stack");
    }

    private sealed class LoggingScene(string name, List<string> log, bool overlay = false) : Scene
    {
        public override bool IsOverlay => overlay;

        public GameContext ReadGame() => Game;

        protected override void Enter() => log.Add($"{name}.enter");

        protected override void Exit() => log.Add($"{name}.exit");

        protected override void Pause() => log.Add($"{name}.pause");

        protected override void Resume() => log.Add($"{name}.resume");

        protected override void Draw(Canvas canvas)
        {
            log.Add($"{name}.draw");
            canvas.FillRect(new RectF(0f, 0f, 1f, 1f), Color.White);
        }
    }

    private sealed class CameraScene : Scene
    {
        protected override void Draw(Canvas canvas)
        {
            canvas.Translate(-300f, -200f);
            canvas.FillRect(new RectF(0f, 0f, 1f, 1f), Color.White);
        }
    }

    private sealed class LeakyScene : Scene
    {
        protected override void Draw(Canvas canvas) => canvas.Push();
    }
}
