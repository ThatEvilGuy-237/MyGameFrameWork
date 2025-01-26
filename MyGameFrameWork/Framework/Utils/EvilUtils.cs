using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;

namespace MyGameFrameWork.Framework.Utils
{
    internal class EvilUtils
    {
        private static Window? _currentWindow;
        private static EvilShaders? _shaders;
        public static void AssignWindow(Window window)
        {
            _currentWindow = window;
            _shaders = new EvilShaders();
            _shaders.SetWindowSize(_currentWindow.windowWidth, _currentWindow._windowHeight);
        }
        public static void SetDpi()
        {

        }
        public static void UpdateWindowSize()
        {
            if (_currentWindow == null || _shaders == null)
            {
                throw new InvalidOperationException("Drawing target window is null");
            }
            _shaders.SetWindowSize(_currentWindow.windowWidth, _currentWindow._windowHeight);
        }
        public static void SetColor(float r, float g, float b, float a)
        {
            if (_shaders != null)
            {
                _shaders.SetColor(r, g, b, a);
            }
        }
        public static void DrawRectangle(float x, float y, float width, float height)
        {
            if (_currentWindow == null)
            {
                throw new InvalidOperationException("Drawing target window is null");
            }

            // Ensure the shader program is active
            _shaders?.Use();  // Activate shader program

            // Define the vertices of the rectangle (two triangles)
            float[] vertices = {
                // Triangle 1
                x, y, 0.0f,                     // Bottom-left
                x + width, y, 0.0f,             // Bottom-right
                x + width, y + height, 0.0f,    // Top-right

                // Triangle 2
                x, y, 0.0f,                     // Bottom-left
                x + width, y + height, 0.0f,    // Top-right
                x, y + height, 0.0f             // Top-left
            };

            // Generate a new buffer for the vertices and bind it
            int vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Create and bind VAO
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            // Enable the vertex array
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);  // Set pointer to the vertex positions

            // Draw the rectangle (two triangles)
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // Cleanup
            GL.DisableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffer(vertexBuffer);
            GL.DeleteVertexArray(vao);
        }

        public static void DrawLine(float x1, float y1, float x2, float y2)
        {
            if (_currentWindow == null)
            {
                throw new InvalidOperationException("Drawing target window is null");
            }

            _shaders?.Use();  // Activate shader program

            // Define the vertices for the line (in HD coordinates)
            float[] vertices = {
        x1, y1, 0.0f,   // Start point
        x2, y2, 0.0f    // End point
    };

            // Create and bind VAO/VBO
            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Draw the line (2 vertices)
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);

            // Cleanup
            GL.DisableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
        }




    }
}
