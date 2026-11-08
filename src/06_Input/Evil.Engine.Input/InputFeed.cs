using Evil.Engine.Platform;
using Godot;
using GodotButton = Godot.MouseButton;

namespace Evil.Engine.Input;

internal static class InputFeed
{
    public static void Feed(InputEvent input, Keyboard keyboard, Mouse mouse)
    {
        switch (input)
        {
            case InputEventKey key:
                FeedKey(key, keyboard);
                break;
            case InputEventMouseMotion motion:
                mouse.Move(motion.Position.ToEvil());
                break;
            case InputEventMouseButton button:
                FeedButton(button, mouse);
                break;
        }
    }

    private static void FeedKey(InputEventKey key, Keyboard keyboard)
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

    private static void FeedButton(InputEventMouseButton button, Mouse mouse)
    {
        mouse.Move(button.Position.ToEvil());
        float steps = button.Factor > 0f ? button.Factor : 1f;
        switch (button.ButtonIndex)
        {
            case GodotButton.WheelUp when button.Pressed:
                mouse.Wheel(steps);
                return;
            case GodotButton.WheelDown when button.Pressed:
                mouse.Wheel(-steps);
                return;
        }

        MouseButton? mapped = button.ButtonIndex switch
        {
            GodotButton.Left => MouseButton.Left,
            GodotButton.Right => MouseButton.Right,
            GodotButton.Middle => MouseButton.Middle,
            GodotButton.Xbutton1 => MouseButton.Back,
            GodotButton.Xbutton2 => MouseButton.Forward,
            _ => null,
        };
        if (mapped is not { } pressed)
        {
            return;
        }

        if (button.Pressed)
        {
            mouse.Press(pressed);
        }
        else
        {
            mouse.Release(pressed);
        }
    }
}
