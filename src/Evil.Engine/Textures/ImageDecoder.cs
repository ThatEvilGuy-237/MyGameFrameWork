using StbImageSharp;

namespace Evil.Engine.Textures;

internal static class ImageDecoder
{
    public static DecodedImage Decode(Stream stream)
    {
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        return new DecodedImage(image.Width, image.Height, image.Data);
    }
}

internal readonly record struct DecodedImage(int Width, int Height, byte[] Pixels);
