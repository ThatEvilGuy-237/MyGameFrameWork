using Evil.Engine.Capture;
using Evil.Engine.Textures;

namespace Evil.Engine.Tests;

internal static class CaptureTests
{
    [Test]
    public static void Crc32MatchesTheStandardCheckValue()
    {
        Check.Equal(0xCBF43926u, Crc32.Compute("123456789"u8), "crc32");
    }

    [Test]
    public static void PngRoundTripsThroughTheDecoder()
    {
        byte[] pixels =
        [
            255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255,
            10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120,
        ];
        using var stream = new MemoryStream(PngEncoder.Encode(3, 2, pixels));
        var image = ImageDecoder.Decode(stream);
        Check.Equal(3, image.Width, "width");
        Check.Equal(2, image.Height, "height");
        Check.True(image.Pixels.AsSpan().SequenceEqual(pixels), "pixels survive");
    }

    [Test]
    public static void PngRejectsAWrongBufferSize()
    {
        Check.Throws<ArgumentException>(() => PngEncoder.Encode(2, 2, new byte[8]), "needs 16 bytes");
    }

    [Test]
    public static void ScreenGrabsAreFlippedAndOpaque()
    {
        byte[] bottomUp = [1, 1, 1, 0, 2, 2, 2, 9];
        var topDown = FrameCapture.TopDownOpaque(bottomUp, 1, 2);
        Check.True(topDown.AsSpan().SequenceEqual(new byte[] { 2, 2, 2, 255, 1, 1, 1, 255 }), "rows swapped, alpha full");
    }
}
