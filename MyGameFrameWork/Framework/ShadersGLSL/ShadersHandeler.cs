using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace MyGameFrameWork.Framework.ShadersGLSL
{
    public class ShadersHandeler : EvilShader
    {
        public void SetColor(float r, float g, float b, float a)
        {
            Use();
            int colorLoc = GL.GetUniformLocation(_shaderProgram, "u_Color");
            if (colorLoc == -1)
            {
                throw new Exception("Uniform 'u_Color' not found in shader program.");
            }
            GL.Uniform4(colorLoc, r, g, b, a);
        }

        public void SetWindowSize(float width, float height)
        {
            Use();
            int windowSize = GL.GetUniformLocation(_shaderProgram, "ViewportSize");
            if (windowSize == -1)
            {
                throw new Exception("Uniform 'ViewportSize' not found in shader program.");
            }
            GL.Uniform2(windowSize, width, height);
        }

        // Calc Transformation SRT
        public void ApplySRTMatrix(Matrix4 transformationMatrix)
        {
            Use();
            int transformLoc = GL.GetUniformLocation(_shaderProgram, "u_Transformation");
            if (transformLoc == -1)
            {
                throw new Exception("Uniform 'u_Transformation' not found in shader program.");
            }
            GL.UniformMatrix4(transformLoc, false, ref transformationMatrix);
        }
        // Add texture
        public void SetTexture(int textureID)
        {
            Use();
            GL.ActiveTexture(TextureUnit.Texture0);

            if (textureID > 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, textureID);
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind texture
            }

            int textureLoc = GL.GetUniformLocation(_shaderProgram, "texture1");
            if (textureLoc == -1)
            {
                throw new Exception("Uniform 'texture1' not found in shader program.");
            }
            GL.Uniform1(textureLoc, 0);
        }

    }
}
