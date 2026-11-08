using System.Numerics;

namespace Evil.Engine.Input;

public sealed class Mouse
{
    private readonly ButtonStates<MouseButton> _buttons = new ButtonStates<MouseButton>();

    internal Mouse()
    {
    }

    public Vector2 Position { get; private set; }

    public Vector2 Delta { get; private set; }

    public float Scroll { get; private set; }

    public bool IsDown(MouseButton button) => _buttons.IsDown(button);

    public bool WasPressed(MouseButton button) => _buttons.WasPressed(button);

    public bool WasReleased(MouseButton button) => _buttons.WasReleased(button);

    internal void Place(Vector2 position) => Position = position;

    internal void Move(Vector2 position)
    {
        Delta += position - Position;
        Position = position;
    }

    internal void Press(MouseButton button) => _buttons.Press(button);

    internal void Release(MouseButton button) => _buttons.Release(button);

    internal void ReleaseAll() => _buttons.ReleaseAll();

    internal void Wheel(float steps) => Scroll += steps;

    internal void EndFrame()
    {
        _buttons.EndFrame();
        Delta = Vector2.Zero;
        Scroll = 0f;
    }
}
