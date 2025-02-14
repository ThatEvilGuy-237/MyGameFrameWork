using System;
using System.Drawing;
using SixLabors.ImageSharp;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using MyGameFrameWork.Framework.ShadersGLSL;

namespace MyGameFrameWork.Framework.Utils
{
    internal class TextureHandeler
    {
        public static int LoadTexture(string filePath)
        {
            using var image = Image.Load<Rgba32>(filePath);

            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            // Flip image manually (because OpenGL uses bottom-left origin)
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          image.Width, image.Height, 0, PixelFormat.Rgba,
                          PixelType.UnsignedByte, pixels);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return textureId;
        }
        public static void ClearTexture(int textureID)
        {
            GL.DeleteTexture(textureID);
        }

    }
}
