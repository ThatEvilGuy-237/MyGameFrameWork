using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MyGameFrameWork.Framework.ShadersGLSL
{
    public class EvilShader
    {
        protected int _shaderProgram;
        protected int _vertexShader;
        protected int _fragmentShader;
        protected bool _isProgramUsed;

        private const string VertexShaderPath = @"Framework\ShadersGLSL\glsl\VertexShader.glsl";
        private const string FragmentShaderPath = @"Framework\ShadersGLSL\glsl\FragmentShader.glsl";

        public EvilShader()
        {
            string vertexSource = LoadShaderSource(VertexShaderPath);
            string fragmentSource = LoadShaderSource(FragmentShaderPath);
            _vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            _fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);
            _shaderProgram = LinkProgram(_vertexShader, _fragmentShader);
        }
        public EvilShader(string vertexShaderPath, string fragmentShaderPath)
        {
            string vertexSource = LoadShaderSource(vertexShaderPath);
            string fragmentSource = LoadShaderSource(fragmentShaderPath);
            _vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            _fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);
            _shaderProgram = LinkProgram(_vertexShader, _fragmentShader);
        }
        private string LoadShaderSource(string filePath)
        {
            // Move up from `bin/Debug/net8.0/` to the project directory
            // #!Change production
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
            string fullPath = Path.Combine(projectRoot, filePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Shader file not found: {fullPath}");

            return File.ReadAllText(fullPath);
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
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling shader: {infoLog}");
            }
        }

        private int LinkProgram(int vertexShader, int fragmentShader)
        {
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
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
        public void StopUse()
        {
            if (_isProgramUsed)
            {
                GL.UseProgram(0);
                _isProgramUsed = false;
            }
        }
        public void Dispose()
        {
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteShader(_vertexShader);
            GL.DeleteShader(_fragmentShader);
        }
    }
}
