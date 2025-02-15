using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Text;

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
        public void SetTexture(int textureID, Vector4 objectRectSize)
        {
            Use();  // Ensure the shader program is active
            PrintAllUniforms();
            GL.ActiveTexture(TextureUnit.Texture0); // Use the first texture unit

            if (textureID > 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, textureID); // Bind the texture
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind the texture
            }

            // Set the texture uniform in the shader
            int textureLoc = GL.GetUniformLocation(_shaderProgram, "texture1");
            if (textureLoc == -1)
            {
                throw new Exception("Uniform 'texture1' not found in shader program.");
            }
            GL.Uniform1(textureLoc, 0); // Set the texture unit to 0 (TextureUnit.Texture0)
            Use();
            // Set the ObjectRectSize uniform in the shader
            int objRectSize = GL.GetUniformLocation(_shaderProgram, "ObjectRectSize");
            if (objRectSize == -1)
            {
               // throw new Exception("Uniform 'SourceRect' not found in shader program.");
            }
            GL.Uniform4(objRectSize, objectRectSize); // Set the SourceRect (x, y, width, height)
        }
        void PrintAllUniforms()
        {
            GL.GetProgram(_shaderProgram, GetProgramParameterName.ActiveUniforms, out int uniformCount);

            Console.WriteLine($"Active Uniforms ({uniformCount} found):");
            for (int i = 0; i < uniformCount; i++)
            {
                int length;
                int size;
                ActiveUniformType type;

                GL.GetActiveUniform(_shaderProgram, i, 256, out length, out size, out type, out string name);
                Console.WriteLine($"  {i}: {name} ({type})");
            }
        }


    }
}
