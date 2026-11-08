using Evil.Engine.Core;

namespace Evil.Engine.Tests;

internal static class CoreTests
{
    [Test]
    public static void ColorFromHexAndBack()
    {
        Color orange = Color.FromHex("#FF8000");
        Check.Near(1f, orange.R, "red");
        Check.Near(128f / 255f, orange.G, "green");
        Check.Near(1f, orange.A, "alpha defaults to opaque");
        Check.Equal("#FF8000FF", orange.ToHex(), "hex");
        Check.Near(0x44 / 255f, Color.FromHex("11223344").A, "alpha from hex");
        void ReadFiveDigits() => Color.FromHex("#12345");
        Check.Throws<FormatException>(ReadFiveDigits, "#RRGGBB");
    }

    [Test]
    public static void ColorMixes()
    {
        Check.Equal(new Color(0.5f, 0.5f, 0.5f, 1f), Color.Lerp(Color.Black, Color.White, 0.5f), "halfway");
        Check.Equal(new Color(1f, 0f, 0f, 0.5f), Color.Red.WithAlpha(0.5f), "with alpha");
        Check.Equal(Color.Black, Color.Red.Multiply(Color.Blue).WithAlpha(1f), "red times blue");
    }
}
