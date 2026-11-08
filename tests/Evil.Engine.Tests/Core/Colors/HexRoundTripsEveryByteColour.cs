using Evil.Engine.Core;

namespace Evil.Engine.Tests.Core.Colors;

internal sealed class HexRoundTripsEveryByteColour : Property<Color>
{
    protected override Color Generate(Random random) =>
        Color.FromBytes((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

    protected override void Verify(Color sample)
    {
        Check.Equal(sample, Color.FromHex(sample.ToHex()), "through hex");
        Check.Equal(sample, Color.FromHex(sample.ToHex().ToLowerInvariant()), "through lowercase hex");
    }
}
