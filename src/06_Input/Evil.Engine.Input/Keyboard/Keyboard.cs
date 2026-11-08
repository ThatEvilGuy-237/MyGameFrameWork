namespace Evil.Engine.Input;

public sealed class Keyboard
{
    private readonly HashSet<Key> _down = [];
    private readonly HashSet<Key> _pressed = [];
    private readonly HashSet<Key> _released = [];

    internal Keyboard()
    {
    }

    public bool IsAnyDown => _down.Count > 0;

    public bool IsDown(Key key) => _down.Contains(key);

    public bool WasPressed(Key key) => _pressed.Contains(key);

    public bool WasReleased(Key key) => _released.Contains(key);

    internal void Press(Key key, bool isRepeat)
    {
        if (isRepeat)
        {
            return;
        }

        _down.Add(key);
        _pressed.Add(key);
    }

    internal void Release(Key key)
    {
        _down.Remove(key);
        _released.Add(key);
    }

    internal void ReleaseAll()
    {
        foreach (Key key in _down)
        {
            _released.Add(key);
        }

        _down.Clear();
    }

    internal void EndFrame()
    {
        _pressed.Clear();
        _released.Clear();
    }
}
