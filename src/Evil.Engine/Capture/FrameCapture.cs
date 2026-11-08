using OpenTK.Graphics.OpenGL4;

namespace Evil.Engine.Capture;

internal static class FrameCapture
{
    public static void Save(string path, int width, int height) =>
        PngEncoder.Save(path, width, height, ReadPixels(width, height));

    private static byte[] ReadPixels(int width, int height)
    {
        var bottomUp = new byte[width * height * 4];
        GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
        GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, bottomUp);
        return TopDownOpaque(bottomUp, width, height);
    }

    internal static byte[] TopDownOpaque(byte[] bottomUp, int width, int height)
    {
        var stride = width * 4;
        var topDown = new byte[bottomUp.Length];
        for (var y = 0; y < height; y++)
        {
            Array.Copy(bottomUp, (height - 1 - y) * stride, topDown, y * stride, stride);
        }

        for (var alpha = 3; alpha < topDown.Length; alpha += 4)
        {
            topDown[alpha] = 255;
        }

        return topDown;
    }
}
