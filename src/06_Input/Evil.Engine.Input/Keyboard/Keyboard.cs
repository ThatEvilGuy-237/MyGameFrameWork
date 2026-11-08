namespace Evil.Engine.Input;

public sealed class Keyboard
{
    private readonly ButtonStates<Key> _keys = new ButtonStates<Key>();

    internal Keyboard()
    {
    }

    public bool IsAnyDown => _keys.IsAnyDown;

    public bool IsDown(Key key) => _keys.IsDown(key);

    public bool WasPressed(Key key) => _keys.WasPressed(key);

    public bool WasReleased(Key key) => _keys.WasReleased(key);

    internal void Press(Key key, bool isRepeat)
    {
        if (!isRepeat)
        {
            _keys.Press(key);
        }
    }

    internal void Release(Key key) => _keys.Release(key);

    internal void ReleaseAll() => _keys.ReleaseAll();

    internal void EndFrame() => _keys.EndFrame();
}
