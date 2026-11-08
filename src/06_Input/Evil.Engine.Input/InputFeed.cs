using Evil.Engine.Platform;
using Godot;
using GodotButton = Godot.MouseButton;

namespace Evil.Engine.Input;

internal sealed class InputFeed(Keyboard keyboard, Mouse mouse)
{
    public void Feed(InputEvent input)
    {
        switch (input)
        {
            case InputEventKey key:
                FeedKey(key);
                break;
            case InputEventMouseMotion motion:
                mouse.Move(motion.Position.ToEvil());
                break;
            case InputEventMouseButton button:
                FeedButton(button);
                break;
        }
    }

    public void EndFrame()
    {
        keyboard.EndFrame();
        mouse.EndFrame();
    }

    public void ReleaseAll()
    {
        keyboard.ReleaseAll();
        mouse.ReleaseAll();
    }

    private void FeedKey(InputEventKey key)
    {
        Godot.Key code = key.PhysicalKeycode != Godot.Key.None ? key.PhysicalKeycode : key.Keycode;
        if (!GodotKeys.TryMap(code, out Key mapped))
        {
            return;
        }

        if (key.Pressed)
        {
            keyboard.Press(mapped, key.Echo);
        }
        else
        {
            keyboard.Release(mapped);
        }
    }

    private void FeedButton(InputEventMouseButton button)
    {
        mouse.Move(button.Position.ToEvil());
        if (FeedWheel(button) || ButtonOf(button.ButtonIndex) is not { } mapped)
        {
            return;
        }

        if (button.Pressed)
        {
            mouse.Press(mapped);
        }
        else
        {
            mouse.Release(mapped);
        }
    }

    private bool FeedWheel(InputEventMouseButton button)
    {
        bool up = button.ButtonIndex == GodotButton.WheelUp;
        bool down = button.ButtonIndex == GodotButton.WheelDown;
        if (!button.Pressed || (!up && !down))
        {
            return false;
        }

        float steps = button.Factor > 0f ? button.Factor : 1f;
        mouse.Wheel(up ? steps : -steps);
        return true;
    }

    private static MouseButton? ButtonOf(GodotButton button) => button switch
    {
        GodotButton.Left => MouseButton.Left,
        GodotButton.Right => MouseButton.Right,
        GodotButton.Middle => MouseButton.Middle,
        GodotButton.Xbutton1 => MouseButton.Back,
        GodotButton.Xbutton2 => MouseButton.Forward,
        _ => null,
    };

}
