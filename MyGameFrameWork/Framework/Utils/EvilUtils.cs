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
        private static SRTVector _TotaalSRTVector;
        private static int _SRTDepth;
        public static void AssignWindow(Window window)
        {
            _SRTDepth = 0;
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
        //translations
        public static void NewPush()
        {
            _SRTDepth++;
            if (_SRTDepth <= 1) return;
            _SRTVectorsStack.Add(_CurrentSRTVector);
            _CurrentSRTVector = new SRTVector();
        }
        public static void PopOrgin()
        {
            _SRTDepth--;
            if (_SRTDepth < 0) _SRTDepth = 0;
            _CurrentSRTVector = _SRTVectorsStack.GetLast();
            _SRTVectorsStack.Pop();
        }
        private static void ApplySRTMatrix()
        {
            SRTVector totaal = new SRTVector();

            GetFullTranslation();
            _ShadersHandeler!.ApplySRTMatrix(_TotaalSRTVector.GetSRTMatrix());
        }
        public static void GetFullTranslation()
        {
            SRTVector totaalSRT = new SRTVector();
            Stack<SRTVector> vectorsStack = _SRTVectorsStack.GetStack();

            // Start by adding the transformations from the stack
            for (int i = 0; i < vectorsStack.Count; i++)
            {
                SRTVector vector = vectorsStack.ElementAt(i);

                totaalSRT.Translate += vector.Translate;
                totaalSRT.Rotate += vector.Rotate;
            }
            SRTVector currentVector = new SRTVector(_CurrentSRTVector);
            currentVector.Translate = AddRotationTranslate(currentVector.Translate, totaalSRT.Rotate.Z);
            totaalSRT.Translate += currentVector.Translate;

            totaalSRT.Rotate += currentVector.Rotate;
            _TotaalSRTVector = totaalSRT;
        }

        public static Vector3 AddRotationTranslate(Vector3 vector, float deg)
        {
            float rad = MathF.PI / 180f * deg;

            float newX = vector.X * MathF.Cos(rad) - vector.Y * MathF.Sin(rad);
            float newY = vector.Y * MathF.Cos(rad) + vector.X * MathF.Sin(rad);
            vector.X = newX;
            vector.Y = newY;
            vector.Z = 0;
            return vector;
        }
        // Translate
        #region Translate-SRT
        public static void PushTranslate(Vector3 translate)
        {
            _CurrentSRTVector.Translate += translate;
        }
        public static void PushTranslate(float x = 0f, float y = 0f, float z = 0f)
        {
            PushTranslate(new Vector3(x, y, z));
        }
        #endregion
        //Rotate
        #region Rotate-SRT
        public static void PushRotate(Vector3 rotate)
        {
            _CurrentSRTVector.Rotate += rotate;
        }
        public static void PushRotate(float rotate)
        {
            PushRotate(new Vector3(rotate, rotate, rotate));
        }
        public static void PushRotate(float x = 0f, float y = 0f, float z = 0f)
        {
            PushRotate(new Vector3(x, y, z));
        }
        #endregion

        //Scale
        #region Scale-SRT
        public static void PushScale(Vector3 scale)
        {
            _CurrentSRTVector.Scale += scale;
        }
        public static void PushScale(float scale)
        {
            PushScale(new Vector3(scale));
        }
        public static void PushScale(float x = 1f, float y = 1f, float z = 1f)
        {
            PushScale(new Vector3(x, y, z));
        }
        #endregion
        public static void SetColor(float r, float g, float b, float a)
        {
            if (_ShadersHandeler != null)
            {
                _ShadersHandeler.SetColor(r, g, b, a);
            }
        }

        // DRAWING 
        #region Scale-SRT
        public static void DrawRectangle(float x, float y, float width, float height)
        {
            if (_CurrentWindow == null)
            {
                throw new InvalidOperationException("Drawing target window is null");
            }

            // Ensure the shader program is active
            _ShadersHandeler?.Use();  // Activate shader program
            ApplySRTMatrix();
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



        #endregion
    }
}
