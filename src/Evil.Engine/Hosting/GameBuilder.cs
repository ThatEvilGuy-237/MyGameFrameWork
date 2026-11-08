using Evil.Engine.Graphics;
using Evil.Engine.Scenes;
using Evil.Engine.Textures;

namespace Evil.Engine.Hosting;

public sealed class GameBuilder
{
    private readonly WindowSettings _window;
    private Color _clearColor = new(0.2f, 0.3f, 0.3f);
    private string _contentRoot = AppContext.BaseDirectory;
    private TextureFilter _textureFilter = TextureFilter.Linear;
    private Scene? _startScene;
    private FrameSnapshot? _snapshot;

    private GameBuilder(WindowSettings window) => _window = window;

    public static GameBuilder Create(WindowSettings window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return new GameBuilder(window);
    }

    public static GameBuilder Create(string title, int width, int height) =>
        Create(new WindowSettings(title, width, height));

    public GameBuilder ClearColor(Color color)
    {
        _clearColor = color;
        return this;
    }

    public GameBuilder Content(string rootFolder)
    {
        _contentRoot = rootFolder;
        return this;
    }

    public GameBuilder Textures(TextureFilter filter)
    {
        _textureFilter = filter;
        return this;
    }

    public GameBuilder Start(Scene scene)
    {
        _startScene = scene;
        return this;
    }

    public GameBuilder CaptureAndExit(string path, long frame)
    {
        _snapshot = new FrameSnapshot(path, frame);
        return this;
    }

    public GameHost Build()
    {
        if (string.IsNullOrWhiteSpace(_window.Title))
        {
            throw new InvalidOperationException("GameBuilder.Create: the window title is empty.");
        }

        if (_window.Width <= 0 || _window.Height <= 0)
        {
            throw new InvalidOperationException($"GameBuilder.Create: the window size must be positive, got {_window.Width}x{_window.Height}.");
        }

        if (_window.UpdatesPerSecond < 0 || double.IsNaN(_window.UpdatesPerSecond))
        {
            throw new InvalidOperationException($"GameBuilder.Create: UpdatesPerSecond must be 0 (as fast as possible) or more, got {_window.UpdatesPerSecond}.");
        }

        if (string.IsNullOrWhiteSpace(_contentRoot) || !Directory.Exists(_contentRoot))
        {
            throw new InvalidOperationException($"GameBuilder.Content: the folder '{_contentRoot}' does not exist.");
        }

        if (_startScene is null)
        {
            throw new InvalidOperationException("GameBuilder.Start was not called: the game needs a first scene.");
        }

        if (_snapshot is not null && (string.IsNullOrWhiteSpace(_snapshot.Path) || _snapshot.Frame < 0))
        {
            throw new InvalidOperationException($"GameBuilder.CaptureAndExit needs a file path and a frame of 0 or more, got '{_snapshot.Path}' at frame {_snapshot.Frame}.");
        }

        var snapshot = _snapshot is null ? null : _snapshot with { Path = Path.GetFullPath(_snapshot.Path) };
        return new GameHost(new GameOptions(_window, _clearColor, Path.GetFullPath(_contentRoot), _textureFilter, _startScene, snapshot));
    }
}
