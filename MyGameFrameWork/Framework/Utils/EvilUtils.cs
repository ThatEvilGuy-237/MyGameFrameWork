using MyGameFrameWork.Framework.ShadersGLSL;
using MyGameFrameWork.Framework.Utils.DrawingShapes;
using MyGameFrameWork.Framework.Utils.Structs;
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
            _TotaalSRTVector = new SRTVector();
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
                totaalSRT.Scale *= vector.Scale;
            }
            SRTVector currentVector = new SRTVector(_CurrentSRTVector);
            currentVector.Translate = AddRotationTranslate(currentVector.Translate, totaalSRT.Rotate.Z);
            totaalSRT.Translate += currentVector.Translate;

            totaalSRT.Rotate += currentVector.Rotate;
            totaalSRT.Scale *= currentVector.Scale;
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
        //#-FIX
        public static void PushScale(Vector3 scale)
        {
            _CurrentSRTVector.Scale *= scale;
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

        // DRAWING 
        #region DRAWING
        public static void SetColor(float r, float g, float b, float a)
        {
            if (_ShadersHandeler != null)
            {
                _ShadersHandeler.SetColor(r, g, b, a);
            }
        }
        static int _CurrentTextureID = 0;
        //static Vector4 _CurrentSourceRect = new Vector4();

        public static void SetTexture(string texturePath, Rect sourceRect)
        {
            // Load the texture
            TextureHandeler.LoadTexture(texturePath);
            _CurrentTextureID = TextureHandeler.LoadTexture(texturePath);

            // Set the normalized SourceRect for the shader
            //_CurrentSourceRect = new Vector4(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
        }

        private static void StartDraw()
        {
            if (_CurrentWindow == null)
            {
                throw new InvalidOperationException("Drawing target window is null");
            }
            _ShadersHandeler?.Use();  // Activate shader program
            ApplySRTMatrix();
        }
        public static void StopDrawing(int vertexBuffer, int vao)
        {
            // Cleanup
            GL.DisableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffer(vertexBuffer);
            GL.DeleteVertexArray(vao);
        }
        public static void Draw(Shape shape)
        {
            StartDraw();

            List<Vector3> verticesList = shape.Vertices;

            // Convert the List<Vector3> to a float array for OpenGL
            float[] vertices = shape.GetFloatArray();
            RectF bound = shape.GetBoundingRectangle();
            _ShadersHandeler?.SetTexture(_CurrentTextureID, new Vector4(bound.X, bound.Y, bound.Width, bound.Height));
            // Generate a new buffer for the vertices and bind it
            int vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Create and bind VAO (Vertex Array Object)
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            // Enable the vertex array
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);  // Set pointer to the vertex positions

            // Draw the shape (assuming the shape is a polygon with vertices defined)
            GL.DrawArrays(PrimitiveType.Triangles, 0, shape.Vertices.Count);
            // Cleanup
            StopDrawing(vertexBuffer, vao);
        }
        public static void DrawRectanlge(Rect rect)
        {
            Draw(ToShape(rect));
        }
        public static void DrawRectanlge(RectF rect)
        {
            Draw(ToShape(rect));
        }
        public static void DrawRectanlge(RectD rect)
        {
            Draw(ToShape(rect));
        }
        public static void DrawRectanlge(float x, float y, float width, float height)
        {
            DrawRectanlge(new RectF(x, y, width, height));
        }

        #endregion

        // SHAPES CONVERTER 
        #region CONVERTER
        public static Shape ToShape<T>(IRectangle<T> rect) where T : struct
        {
            Shape shape = new Shape();

            // Convert values to float (shape is float)
            float x = Convert.ToSingle(rect.X);
            float y = Convert.ToSingle(rect.Y);
            float width = Convert.ToSingle(rect.Width);
            float height = Convert.ToSingle(rect.Height);

            // First triangle:
            shape.Vertices.Add(new Vector3(x, y, 0)); // Top-left
            shape.Vertices.Add(new Vector3(x + width, y, 0)); // Top-right
            shape.Vertices.Add(new Vector3(x, y + height, 0)); // Bottom-left

            // Second triangle:
            shape.Vertices.Add(new Vector3(x + width, y, 0)); // Top-right
            shape.Vertices.Add(new Vector3(x + width, y + height, 0)); // Bottom-right
            shape.Vertices.Add(new Vector3(x, y + height, 0)); // Bottom-left

            return shape;
        }



        #endregion
    }
}
