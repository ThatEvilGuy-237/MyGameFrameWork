using Evil.Engine.Core;

namespace Evil.Engine.Lighting;

public abstract record Background
{
    private protected Background()
    {
    }

    public static Background ClearColor { get; } = new ClearColorBackground();

    public static Background Solid(Color color) => new SolidBackground(color);

    public static Background Gradient(Color top, Color horizon, Color ground) => new GradientBackground(top, horizon, ground);
}

public sealed record ClearColorBackground : Background;

public sealed record SolidBackground(Color Color) : Background;

public sealed record GradientBackground(Color Top, Color Horizon, Color Ground) : Background;
