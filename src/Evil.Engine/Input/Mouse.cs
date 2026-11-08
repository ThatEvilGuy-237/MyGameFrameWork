using System.Numerics;

namespace Evil.Engine.Input;

public sealed class Mouse
{
    private readonly HashSet<MouseButton> _down = [];
    private readonly HashSet<MouseButton> _pressed = [];
    private readonly HashSet<MouseButton> _released = [];

    internal Mouse()
    {
    }

    public Vector2 Position { get; private set; }

    public Vector2 Delta { get; private set; }

    public float Scroll { get; private set; }

    public bool IsDown(MouseButton button) => _down.Contains(button);

    public bool WasPressed(MouseButton button) => _pressed.Contains(button);

    public bool WasReleased(MouseButton button) => _released.Contains(button);

    internal void Place(Vector2 position) => Position = position;

    internal void Move(Vector2 position)
    {
        Delta += position - Position;
        Position = position;
    }

    internal void Press(MouseButton button)
    {
        _down.Add(button);
        _pressed.Add(button);
    }

    internal void Release(MouseButton button)
    {
        _down.Remove(button);
        _released.Add(button);
    }

    internal void Wheel(float amount) => Scroll += amount;

    internal void EndFrame()
    {
        _pressed.Clear();
        _released.Clear();
        Delta = Vector2.Zero;
        Scroll = 0f;
    }
}
