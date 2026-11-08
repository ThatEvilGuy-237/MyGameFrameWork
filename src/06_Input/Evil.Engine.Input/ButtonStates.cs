namespace Evil.Engine.Input;

internal sealed class ButtonStates<TButton>
    where TButton : struct, Enum
{
    private readonly HashSet<TButton> _down = new HashSet<TButton>();
    private readonly HashSet<TButton> _pressed = new HashSet<TButton>();
    private readonly HashSet<TButton> _released = new HashSet<TButton>();

    public bool IsAnyDown => _down.Count > 0;

    public bool IsDown(TButton button) => _down.Contains(button);

    public bool WasPressed(TButton button) => _pressed.Contains(button);

    public bool WasReleased(TButton button) => _released.Contains(button);

    public void Press(TButton button)
    {
        _down.Add(button);
        _pressed.Add(button);
    }

    public void Release(TButton button)
    {
        _down.Remove(button);
        _released.Add(button);
    }

    public void ReleaseAll()
    {
        foreach (TButton button in _down)
        {
            _released.Add(button);
        }

        _down.Clear();
    }

    public void EndFrame()
    {
        _pressed.Clear();
        _released.Clear();
    }
}
