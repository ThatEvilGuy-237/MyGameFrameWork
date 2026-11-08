using Evil.Engine.Graphics;
using Evil.Engine.Scenes;
using Evil.Engine.Textures;

namespace Evil.Engine.Hosting;

internal sealed record GameOptions(
    WindowSettings Window,
    Color ClearColor,
    string ContentRoot,
    TextureFilter TextureFilter,
    Scene StartScene,
    FrameSnapshot? Snapshot);

internal sealed record FrameSnapshot(string Path, long Frame);
