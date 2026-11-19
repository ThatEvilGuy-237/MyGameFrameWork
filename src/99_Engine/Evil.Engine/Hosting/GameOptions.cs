using Evil.Engine.Core;
using Evil.Engine.Scenes;
using Evil.Engine.Screen;

namespace Evil.Engine.Hosting;

internal sealed record GameOptions(
    WindowSettings Window,
    GraphicsSettings Graphics,
    Color ClearColor,
    string ContentRoot,
    Scene StartScene,
    FrameSnapshot? Snapshot);
