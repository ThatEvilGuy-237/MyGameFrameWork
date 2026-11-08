using Evil.Engine.Screen;

namespace Evil.Engine.Hosting;

public sealed class GameHost
{
    internal GameHost(GameOptions options) => Options = options;

    public WindowSettings Window => Options.Window;

    internal GameOptions Options { get; }
}
