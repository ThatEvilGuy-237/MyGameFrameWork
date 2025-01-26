using OpenTK.Graphics.OpenGL;
using System;

namespace MyGameFrameWork.Framework.Utils
{
    internal class EvilShaders
    {
        private int _shaderProgram;
        private int _vertexShader;
        private int _fragmentShader;
        private bool _isProgramUsed;

        // Default Shader sources (vertex and fragment)
        private static readonly string DefaultVertexShaderSource = @"
        #version 330 core

        layout(location = 0) in vec3 aPosition;
        uniform vec2 ViewportSize;

        void main()
        {
            float nx = aPosition.x / ViewportSize.x * 2.0 - 1.0;
            float ny = aPosition.y / ViewportSize.y * 2.0 - 1.0;
            gl_Position = vec4(nx, -ny, 0.0, 1.0);
        }";

        private static readonly string DefaultFragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        uniform vec4 u_Color;
        void main()
        {
            FragColor = u_Color;
        }";

        public EvilShaders()
        {
            _vertexShader = CompileShader(ShaderType.VertexShader, DefaultVertexShaderSource);
            _fragmentShader = CompileShader(ShaderType.FragmentShader, DefaultFragmentShaderSource);
            _shaderProgram = LinkProgram(_vertexShader, _fragmentShader);
        }

        public EvilShaders(string vertexShaderSource, string fragmentShaderSource)
        {
            _vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            _fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);
            _shaderProgram = LinkProgram(_vertexShader, _fragmentShader);
        }

        private int CompileShader(ShaderType shaderType, string shaderSource)
        {
            int shader = GL.CreateShader(shaderType);
            GL.ShaderSource(shader, shaderSource);
            GL.CompileShader(shader);
            CheckShaderCompile(shader);

            return shader;
        }

        private void CheckShaderCompile(int shader)
        {
            GL.GetShaderInfoLog(shader, out string infoLog);
            if (!string.IsNullOrEmpty(infoLog))
            {
                throw new Exception($"Error compiling shader: {infoLog}");
            }
        }

        private int LinkProgram(int vertexShader, int fragmentShader)
        {
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            GL.GetProgramInfoLog(program, out string infoLog);
            if (!string.IsNullOrEmpty(infoLog))
            {
                throw new Exception($"Error linking program: {infoLog}");
            }
            return program;
        }

        public void Use()
        {
            if (!_isProgramUsed)
            {
                GL.UseProgram(_shaderProgram);
                _isProgramUsed = true;
            }
        }

        public void SetColor(float r, float g, float b, float a)
        {
            Use();  // Ensure the shader is active
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

        public void Dispose()
        {
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteShader(_vertexShader);
            GL.DeleteShader(_fragmentShader);
        }
    }
}

