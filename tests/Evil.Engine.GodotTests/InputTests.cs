using Evil.Engine.Input;
using Evil.Engine.Mathematics;
using GodotButton = Godot.MouseButton;
using GodotKey = Godot.Key;
using Key = Evil.Engine.Input.Key;
using MouseButton = Evil.Engine.Input.MouseButton;

namespace Evil.Engine.GodotTests;

internal static class InputTests
{
    [Test]
    public static void EveryKeyHasExactlyOneGodotKey()
    {
        var mapped = GodotKeys.All.Values.ToList();
        Check.Equal(Enum.GetValues<Key>().Length, mapped.Count, "every key is mapped");
        Check.Equal(mapped.Count, mapped.Distinct().Count(), "no key is mapped twice");
    }

    [Test]
    public static void ATapInsideOneFrameIsKept()
    {
        var keyboard = new Keyboard();
        var mouse = new Mouse();
        InputFeed.Feed(KeyEvent(GodotKey.A, pressed: true), keyboard, mouse);
        InputFeed.Feed(KeyEvent(GodotKey.A, pressed: false), keyboard, mouse);
        Check.True(keyboard.WasPressed(Key.A) && keyboard.WasReleased(Key.A), "pressed and released this frame");
        Check.True(!keyboard.IsDown(Key.A), "not held");
        keyboard.EndFrame();
        Check.True(!keyboard.WasPressed(Key.A), "the next frame starts clean");
    }

    [Test]
    public static void HeldKeysRepeatWithoutNewPresses()
    {
        var keyboard = new Keyboard();
        var mouse = new Mouse();
        InputFeed.Feed(KeyEvent(GodotKey.Space, pressed: true), keyboard, mouse);
        keyboard.EndFrame();
        InputFeed.Feed(KeyEvent(GodotKey.Space, pressed: true, echo: true), keyboard, mouse);
        Check.True(keyboard.IsDown(Key.Space) && !keyboard.WasPressed(Key.Space), "a repeat is not a new press");
    }

    [Test]
    public static void LayoutKeysCountWhenThereIsNoPhysicalKey()
    {
        var keyboard = new Keyboard();
        InputFeed.Feed(new Godot.InputEventKey { Keycode = GodotKey.Escape, Pressed = true }, keyboard, new Mouse());
        Check.True(keyboard.IsDown(Key.Escape), "escape");
        InputFeed.Feed(new Godot.InputEventKey { PhysicalKeycode = GodotKey.Menu, Pressed = true }, keyboard, new Mouse());
        Check.True(!keyboard.IsAnyDown || keyboard.IsDown(Key.Escape), "an unknown key adds nothing");
    }

    [Test]
    public static void MouseMovesClicksAndScrolls()
    {
        var keyboard = new Keyboard();
        var mouse = new Mouse();
        mouse.Place(new Vector2(10f, 10f));
        InputFeed.Feed(new Godot.InputEventMouseMotion { Position = new Godot.Vector2(15f, 12f) }, keyboard, mouse);
        InputFeed.Feed(new Godot.InputEventMouseMotion { Position = new Godot.Vector2(20f, 20f) }, keyboard, mouse);
        InputFeed.Feed(Button(GodotButton.Left, pressed: true), keyboard, mouse);
        InputFeed.Feed(Button(GodotButton.Left, pressed: false), keyboard, mouse);
        InputFeed.Feed(Button(GodotButton.WheelUp, pressed: true), keyboard, mouse);
        InputFeed.Feed(Button(GodotButton.WheelDown, pressed: true, factor: 2f), keyboard, mouse);
        InputFeed.Feed(Button(GodotButton.Xbutton1, pressed: true), keyboard, mouse);
        Check.Near(new Vector2(20f, 20f), mouse.Position, "position");
        Check.Near(new Vector2(10f, 10f), mouse.Delta, "moved over the frame");
        Check.True(mouse.WasPressed(MouseButton.Left) && mouse.WasReleased(MouseButton.Left), "a quick click counts");
        Check.Near(-1f, mouse.Scroll, "one up, two down");
        Check.True(mouse.IsDown(MouseButton.Back), "side button");
        mouse.EndFrame();
        Check.Near(Vector2.Zero, mouse.Delta, "delta resets");
        Check.Near(0f, mouse.Scroll, "scroll resets");
    }

    [Test]
    public static void LosingFocusLetsGoOfEverything()
    {
        var keyboard = new Keyboard();
        var mouse = new Mouse();
        InputFeed.Feed(KeyEvent(GodotKey.W, pressed: true), keyboard, mouse);
        InputFeed.Feed(Button(GodotButton.Right, pressed: true), keyboard, mouse);
        keyboard.EndFrame();
        mouse.EndFrame();
        keyboard.ReleaseAll();
        mouse.ReleaseAll();
        Check.True(!keyboard.IsDown(Key.W) && keyboard.WasReleased(Key.W), "W let go");
        Check.True(!mouse.IsDown(MouseButton.Right) && mouse.WasReleased(MouseButton.Right), "right button let go");
    }

    [Test]
    public static void PickFindsTheNearestShapeUnderTheMouse()
    {
        using var stage = Stage.Open();
        var camera = Camera(stage);
        var front = stage.Root.Add(new Box { Name = "front" });
        var back = stage.Root.Add(new Box { Name = "back", Position = new Vector3(0f, 0f, -5f) });
        var hit = Picker.Pick(camera, stage.Root, Middle());
        Check.True(hit is not null && ReferenceEquals(front, hit.Target), "the front box");
        Check.Near(new Vector3(0f, 0f, 0.5f), hit!.Point, "on its near face");
        Check.Near(9.5f, hit.Distance, "distance from the camera");

        front.Pickable = false;
        Check.True(ReferenceEquals(back, Picker.Pick(camera, stage.Root, Middle())?.Target), "unpickable is skipped");
        back.Visible = false;
        Check.True(Picker.Pick(camera, stage.Root, Middle()) is null, "hidden is skipped");
    }

    [Test]
    public static void ATurnedBoxIsHitExactly()
    {
        var box = new Box();
        box.RotateY(45f);
        Check.True(box.HitDistance(new Ray3(new Vector3(0.6f, 0f, 5f), -Vector3.UnitZ)) is not null, "inside the turned corner");
        Check.True(box.HitDistance(new Ray3(new Vector3(0.75f, 0f, 5f), -Vector3.UnitZ)) is null, "past the corner");
        box.Destroy();
    }

    [Test]
    public static void ASphereIsHitByItsRealShape()
    {
        var ball = new Sphere(0.5f, Color.White);
        Check.True(ball.HitDistance(new Ray3(new Vector3(0.3f, 0.3f, 5f), -Vector3.UnitZ)) is not null, "on the ball");
        Check.True(ball.HitDistance(new Ray3(new Vector3(0.45f, 0.45f, 5f), -Vector3.UnitZ)) is null, "in the box corner but off the ball");
        ball.Destroy();
    }

    [Test]
    public static void AHitOnAPartFindsTheWhole()
    {
        var pawn = new Capsule(0.35f, 1.4f, Color.Red) { Tag = "pawn-red" };
        var hat = pawn.Add(new Cone(0.3f, 0.45f, Color.White));
        Check.True(ReferenceEquals(pawn, hat.Ancestor<Capsule>()), "the hat belongs to the pawn");
        Check.Equal("pawn-red", hat.Ancestor<Capsule>()?.Tag, "tag carries game data");
        Check.True(pawn.Ancestor<Capsule>() is null, "nothing above the pawn");
        pawn.Destroy();
    }

    private static Godot.InputEventKey KeyEvent(GodotKey key, bool pressed, bool echo = false) =>
        new() { PhysicalKeycode = key, Pressed = pressed, Echo = echo };

    private static Godot.InputEventMouseButton Button(GodotButton button, bool pressed, float factor = 0f) =>
        new() { ButtonIndex = button, Pressed = pressed, Factor = factor, Position = new Godot.Vector2(20f, 20f) };

    private static Camera Camera(Stage stage)
    {
        var camera = stage.Root.Add(new Camera { Position = new Vector3(0f, 0f, 10f) });
        camera.LookAt(Vector3.Zero);
        camera.MakeCurrent();
        return camera;
    }

    private static Vector2 Middle() => Stage.Host!.GetViewport().GetVisibleRect().Size.ToEvil() / 2f;
}
