using MyGameFrameWork.Framework.ShadersGLSL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace MyGameFrameWork.Framework.Utils
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
    }
}
