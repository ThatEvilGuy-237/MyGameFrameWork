using Godot;

namespace Evil.Engine.Screen;

public sealed class Graphics
{
    public const float SmallestScale = 0.25f;
    public const float LargestScale = 2f;

    private readonly Viewport? _view;
    private EdgeSmoothing _edges;
    private PictureSmoothing _picture;
    private float _renderScale = 1f;

    internal Graphics(GraphicsSettings settings, Viewport? view = null)
    {
        _view = view;
        Edges = settings.Edges;
        Picture = settings.Picture;
        RenderScale = settings.RenderScale;
    }

    public EdgeSmoothing Edges
    {
        get => _edges;
        set
        {
            _edges = value;
            if (_view is not null)
            {
                _view.Msaa3D = value switch
                {
                    EdgeSmoothing.X2 => Viewport.Msaa.Msaa2X,
                    EdgeSmoothing.X4 => Viewport.Msaa.Msaa4X,
                    EdgeSmoothing.X8 => Viewport.Msaa.Msaa8X,
                    _ => Viewport.Msaa.Disabled,
                };
            }
        }
    }

    public PictureSmoothing Picture
    {
        get => _picture;
        set
        {
            _picture = value;
            if (_view is not null)
            {
                _view.ScreenSpaceAA = value switch
                {
                    PictureSmoothing.Fxaa => Viewport.ScreenSpaceAAEnum.Fxaa,
                    PictureSmoothing.Smaa => Viewport.ScreenSpaceAAEnum.Smaa,
                    _ => Viewport.ScreenSpaceAAEnum.Disabled,
                };
            }
        }
    }

    public float RenderScale
    {
        get => _renderScale;
        set
        {
            if (!(value >= SmallestScale && value <= LargestScale))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Graphics.RenderScale goes from {SmallestScale} to {LargestScale}: 1 draws the world at the window's size, 2 at twice that and scaled down.");
            }

            _renderScale = value;
            if (_view is not null)
            {
                _view.Scaling3DMode = Viewport.Scaling3DModeEnum.Bilinear;
                _view.Scaling3DScale = value;
            }
        }
    }
}
