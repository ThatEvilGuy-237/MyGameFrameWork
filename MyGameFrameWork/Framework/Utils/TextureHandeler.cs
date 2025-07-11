using SixLabors.ImageSharp;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OpenTK.Mathematics;
using MyGameFrameWork.Framework.Utils.Structs;

namespace MyGameFrameWork.Framework.Utils
{
    internal class TextureHandeler
    {
        private static Dictionary<string, int> _textureCache = new Dictionary<string, int>();
        private static Dictionary<string, Vector2> _textureSizes = new Dictionary<string, Vector2>();
        private static Dictionary<string, Dictionary<Rect, int>> _cutoutCache = new Dictionary<string, Dictionary<Rect, int>>();
        // Function to load and cache textures
        public static int LoadTexture(string filePath)
        {
            // Check if the texture is already cached
            if (_textureCache.ContainsKey(filePath))
            {
                return _textureCache[filePath];
            }

            // Load the texture if it's not in the cache
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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Cache the texture
            _textureCache[filePath] = textureId;
            _textureSizes[filePath] = new Vector2(image.Width, image.Height);
            return textureId;
        }
        // Get the cutout texture ID or create a new one if it doesn't exist
        public static int GetCutout(string filePath, Rect sourceRect)
        {
            // Ensure the full texture is loaded first
            int textureId = LoadTexture(filePath);

            // Check if the cutout is already cached
            if (_cutoutCache.ContainsKey(filePath) && _cutoutCache[filePath].ContainsKey(sourceRect))
            {
                return _cutoutCache[filePath][sourceRect]; // Return the cached cuto    ut ID
            }
            else
            {
                // If the cutout doesn't exist, create it or overwrite if different
                return CreateCutoutTexture(filePath, sourceRect);
            }
        }

        // Create a new texture from the sourceRect region
        private static int CreateCutoutTexture(string filePath, Rect sourceRect)
        {
            using var image = Image.Load<Rgba32>(filePath);

            // Ensure the sourceRect is within the bounds of the image
            if (sourceRect.X < 0 || sourceRect.Y < 0 || sourceRect.X + sourceRect.Width > image.Width || sourceRect.Y + sourceRect.Height > image.Height)
            {
                throw new ArgumentException("SourceRect is out of bounds of the image.");
            }
    
            // Create a new image that contains just the region defined by the sourceRect
            var cutoutImage = image.Clone(x => x.Crop(new SixLabors.ImageSharp.Rectangle((int)sourceRect.X, (int)sourceRect.Y, (int)sourceRect.Width, (int)sourceRect.Height)));

            int cutoutTextureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, cutoutTextureId);

            // Convert the cutout image to a byte array for OpenGL
            var pixels = new byte[cutoutImage.Width * cutoutImage.Height * 4];
            cutoutImage.CopyPixelDataTo(pixels);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          cutoutImage.Width, cutoutImage.Height, 0, PixelFormat.Rgba,
                          PixelType.UnsignedByte, pixels);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Cache the cutout texture
            if (!_cutoutCache.ContainsKey(filePath))
            {
                _cutoutCache[filePath] = new Dictionary<Rect, int>();
            }
            _cutoutCache[filePath][sourceRect] = cutoutTextureId;

            return cutoutTextureId;
        }
        public static Vector2 GetTextureSize(string filePath)
        {
            // Check if the texture size is already cached
            if (_textureSizes.ContainsKey(filePath))
            {
                return _textureSizes[filePath];
            }

            // If the texture size is not cached, load the image to get the size
            using var image = Image.Load<Rgba32>(filePath);
            _textureSizes[filePath] = new Vector2(image.Width, image.Height);
            return _textureSizes[filePath];
        }
        // Optionally clear all cached textures (e.g., on shutdown)
        public static void ClearCache()
        {
            foreach (var textureId in _textureCache.Values)
            {
                GL.DeleteTexture(textureId);
            }
            _textureCache.Clear();
        }
    }
}
