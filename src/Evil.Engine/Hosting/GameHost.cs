namespace Evil.Engine.Hosting;

public sealed class GameHost
{
    private readonly GameOptions _options;
    private bool _started;

    internal GameHost(GameOptions options) => _options = options;

    public WindowSettings Window => _options.Window;

    public string ContentRoot => _options.ContentRoot;

    public void Run()
    {
        if (_started)
        {
            throw new InvalidOperationException("GameHost.Run was already called; build a new game to run again.");
        }

        _started = true;
        using var window = new EngineWindow(_options);
        window.Run();
    }
}
