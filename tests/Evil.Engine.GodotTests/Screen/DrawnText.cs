using Evil.Engine.Screen;

namespace Evil.Engine.GodotTests.Screen;

internal sealed record DrawnText(Vector2 Baseline, int Size, float Width, TextAlign Align, Font? Font);
