using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MyGameFrameWork.Framework.Utils
{
    internal class EvilUtils
    {
        private static Window? _CurrentWindow;
        private static ShadersHandeler? _ShadersHandeler;
        private static SRTVectorsStack _SRTVectorsStack = new SRTVectorsStack();
        private static SRTVector _CurrentSRTVector;
        public static void AssignWindow(Window window)
        {
            _CurrentWindow = window;
            _ShadersHandeler = new ShadersHandeler();
            _ShadersHandeler.SetWindowSize(_CurrentWindow.windowWidth, _CurrentWindow._windowHeight);
        }
        public static void UpdateWindowSize()
        {
            if (_CurrentWindow == null || _ShadersHandeler == null)
            {
                throw new InvalidOperationException("Drawing target window is null");
            }
            _ShadersHandeler.SetWindowSize(_CurrentWindow.windowWidth, _CurrentWindow._windowHeight);
        }
        private static void NewPush()
        {
            _SRTVectorsStack.Add(_CurrentSRTVector.Scale, _CurrentSRTVector.Rotate, _CurrentSRTVector.Translate);
            _CurrentSRTVector = new SRTVector();
        }
        //translations
        private static void ApplySRTMatrix()
        {
            //_ShadersHandeler!.ApplySRTMatrix();
        }
        public static void PushTranslate(Vector3 translate)
        {
            Matrix4 x = _ShadersHandeler!.CalcTranslationMatrix(translate);
        }


        public static void SetColor(float r, float g, float b, float a)
        {
            if (_ShadersHandeler != null)
            {
                _ShadersHandeler.SetColor(r, g, b, a);
            }
        }

        public static void DrawCube()
        {
            if (_CurrentWindow == null)
            {
                throw new InvalidOperationException("Drawing target window is null");
            }

            // Ensure the shader program is active
            _ShadersHandeler?.Use();  // Activate shader program

            // Define the vertices of the cube (6 faces, each made up of 2 triangles, 36 vertices in total)
            float size = 100f; // Cube size adjusted to fit between 0 and 800 (half of 800 to center it)
            float shift = 100f; // Shift to the center of the 0-800 range (half of 800)

            // Vertices for the cube
            float[] vertices = {
    // Front face (z = 1)
    -size, -size, size,  // Bottom-left
    size, -size, size,   // Bottom-right
    size, size, size,    // Top-right

    -size, -size, size,  // Bottom-left
    size, size, size,    // Top-right
    -size, size, size,   // Top-left

    // Back face (z = -1)
    -size, -size, -size, // Bottom-left
    -size, size, -size,  // Top-left
    size, size, -size,   // Top-right

    -size, -size, -size, // Bottom-left
    size, size, -size,   // Top-right
    size, -size, -size,  // Bottom-right

    // Left face (x = -1)
    -size, -size, size,  // Front-bottom-left
    -size, -size, -size, // Back-bottom-left
    -size, size, -size,  // Back-top-left

    -size, -size, size,  // Front-bottom-left
    -size, size, -size,  // Back-top-left
    -size, size, size,   // Front-top-left

    // Right face (x = 1)
    size, -size, size,   // Front-bottom-right
    size, size, size,    // Front-top-right
    size, size, -size,   // Back-top-right

    size, -size, size,   // Front-bottom-right
    size, size, -size,   // Back-top-right
    size, -size, -size,  // Back-bottom-right

    // Top face (y = 1)
    -size, size, size,   // Front-top-left
    size, size, size,    // Front-top-right
    size, size, -size,   // Back-top-right

    -size, size, size,   // Front-top-left
    size, size, -size,   // Back-top-right
    -size, size, -size,  // Back-top-left

    // Bottom face (y = -1)
    -size, -size, size,  // Front-bottom-left
    -size, -size, -size, // Back-bottom-left
    size, -size, -size,  // Back-bottom-right

    -size, -size, size,  // Front-bottom-left
    size, -size, -size,  // Back-bottom-right
    size, -size, size    // Front-bottom-right
};

            // Apply transformation to center the cube in the 0-800 range
            // Shift all vertices so the center of the cube is at (400, 400, 400)
            for (int i = 0; i < vertices.Length; i += 3)
            {
                vertices[i] += shift;    // x
                vertices[i + 1] += shift; // y
                vertices[i + 2] += shift; // z
            }


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

            // Draw the cube (36 vertices = 6 faces * 2 triangles * 3 vertices)
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // Cleanup
            GL.DisableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffer(vertexBuffer);
            GL.DeleteVertexArray(vao);
        }



        public static void DrawRectangle(float x, float y, float width, float height)
        {
            if (_CurrentWindow == null)
            {
                throw new InvalidOperationException("Drawing target window is null");
            }

            // Ensure the shader program is active
            _ShadersHandeler?.Use();  // Activate shader program

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
            GL.Scale(5f,5f,1.0f);
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
            if (_CurrentWindow == null)
            {
                throw new InvalidOperationException("Drawing target window is null");
            }

            _ShadersHandeler?.Use();  // Activate shader program

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
