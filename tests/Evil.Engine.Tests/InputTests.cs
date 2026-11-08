using System.Numerics;
using Evil.Engine.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using GlfwMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using MouseButton = Evil.Engine.Input.MouseButton;

namespace Evil.Engine.Tests;

internal static class InputTests
{
    [Test]
    public static void EveryKeyMatchesOpenTK()
    {
        foreach (var key in Enum.GetValues<Key>())
        {
            Check.True(Enum.TryParse<Keys>(key.ToString(), out var openTk), $"OpenTK has no key named {key}");
            Check.Equal((int)openTk, (int)key, $"code of {key}");
        }
    }

    [Test]
    public static void MouseButtonsMatchOpenTK()
    {
        Check.Equal((int)GlfwMouseButton.Left, (int)MouseButton.Left, "left");
        Check.Equal((int)GlfwMouseButton.Right, (int)MouseButton.Right, "right");
        Check.Equal((int)GlfwMouseButton.Middle, (int)MouseButton.Middle, "middle");
        Check.Equal((int)GlfwMouseButton.Button4, (int)MouseButton.Back, "back");
        Check.Equal((int)GlfwMouseButton.Button5, (int)MouseButton.Forward, "forward");
    }

    [Test]
    public static void ATapInsideOneFrameIsNotLost()
    {
        var keyboard = new Keyboard();
        keyboard.Press(Key.P, isRepeat: false);
        keyboard.Release(Key.P);
        Check.True(keyboard.WasPressed(Key.P), "pressed this frame");
        Check.True(keyboard.WasReleased(Key.P), "released this frame");
        Check.True(!keyboard.IsDown(Key.P), "no longer held");
        keyboard.EndFrame();
        Check.True(!keyboard.WasPressed(Key.P), "the next frame starts clean");
    }

    [Test]
    public static void HeldKeysRepeatWithoutNewPresses()
    {
        var keyboard = new Keyboard();
        keyboard.Press(Key.Space, isRepeat: false);
        keyboard.EndFrame();
        keyboard.Press(Key.Space, isRepeat: true);
        Check.True(keyboard.IsDown(Key.Space), "still held");
        Check.True(!keyboard.WasPressed(Key.Space), "a repeat is not a new press");
        Check.True(keyboard.IsAnyDown, "any key down");
    }

    [Test]
    public static void MouseTracksMovesClicksAndScroll()
    {
        var mouse = new Mouse();
        mouse.Place(new Vector2(10f, 10f));
        mouse.Move(new Vector2(15f, 12f));
        mouse.Move(new Vector2(20f, 20f));
        mouse.Press(MouseButton.Left);
        mouse.Release(MouseButton.Left);
        mouse.Wheel(1f);
        mouse.Wheel(2f);
        Check.Near(new Vector2(20f, 20f), mouse.Position, "position");
        Check.Near(new Vector2(10f, 10f), mouse.Delta, "delta over the frame");
        Check.True(mouse.WasPressed(MouseButton.Left) && mouse.WasReleased(MouseButton.Left), "a quick click counts");
        Check.Near(3f, mouse.Scroll, "scroll adds up");
        mouse.EndFrame();
        Check.Near(Vector2.Zero, mouse.Delta, "delta resets");
        Check.Near(0f, mouse.Scroll, "scroll resets");
        Check.Near(new Vector2(20f, 20f), mouse.Position, "position stays");
    }
}
