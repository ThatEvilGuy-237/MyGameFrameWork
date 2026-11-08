using Evil.Engine.Core;

namespace Evil.Engine.Lighting;

public sealed record GradientBackground(Color Top, Color Horizon, Color Ground) : Background;
