namespace Evil.Engine.Screen;

public sealed record GraphicsSettings(
    EdgeSmoothing Edges = EdgeSmoothing.Off,
    PictureSmoothing Picture = PictureSmoothing.Off,
    float RenderScale = 1f);
