// more about OpenTK https://opentk.net/learn/chapter1
// video coolors https://www.youtube.com/watch?v=eAQzayHICWA
using MyGameFrameWork.Framework.Utils;
using MyGameFrameWork.Framework.Utils.DrawingShapes;
using MyGameFrameWork.Framework.Utils.Structs;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
namespace MyGameFrameWork.Framework
{
    internal class Window : GameWindow
    {
        int VertexBufferObject;
        public int _windowWidth;
        public int _windowHeight;
        public int WindowId => VertexBufferObject;
        public int windowWidth => _windowWidth;
        public int windowHeight => _windowHeight;
        public Window(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            { ClientSize = (width, height), Title = title })
        {
            _windowWidth = width;
            _windowHeight = height;
            EvilUtils.AssignWindow(this);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            // Make sure to clear matrices before setting projection and modelview
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();  // Ensure that any previous projection matrix is cleared
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, windowWidth, windowHeight, 0, -1, 1);
            GL.LoadMatrix(ref projection);

            // Set Modelview to identity to ensure no transformations are applied
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }




        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, windowWidth, windowHeight);

            TestDrawing();
            SwapBuffers();
        }
        float r = 0;
        public void TestDrawing()
        {
            // RED RECT
            //EvilUtils.SetTexture("C:...\\Downloads\\cute.png");
            //EvilUtils.SetTexture("!#need image path", new Rect(200, 200,370 -200,370-200 ));
            EvilUtils.NewPush();
            EvilUtils.SetColor(1.0f, 0.0f, 0.0f, 100.0f);
            EvilUtils.PushTranslate(_windowWidth/2, _windowHeight/2);
            EvilUtils.NewPush();
            EvilUtils.PushTranslate(-200, -200);
            EvilUtils.PushRotate(0, 0, r);
            EvilUtils.DrawRectanlge(new RectF(-50,-50,100,100));
            EvilUtils.PopOrgin();

            // GREEN RECT
            EvilUtils.SetColor(0.0f, 1.0f, 0.0f, 100.0f);
            EvilUtils.NewPush();
            EvilUtils.PushTranslate(200, -200);
            EvilUtils.PushRotate(0, 0, -(r * 2));
            EvilUtils.DrawRectanlge(-50f, -50f, 100.0f, 200.0f);
            EvilUtils.PopOrgin();

            // BLUE AND YELLO IN THE SAME TRANSLATION
            // BLUE RECT
            EvilUtils.NewPush();
            EvilUtils.SetColor(0.0f, 0.0f, 1.0f, 100.0f);
            EvilUtils.PushTranslate(0, 100);
            EvilUtils.PushRotate(0, 0, -(r));
            EvilUtils.PushScale(1, 1, 20f);
            EvilUtils.DrawRectanlge(-25f, -25f, 50.0f, 50.0f);

            //YELLOW RECT
            EvilUtils.NewPush();
            EvilUtils.SetColor(1.0f, 1.0f, 0.0f, 100.0f);  // Yellow
            EvilUtils.PushTranslate(0, 150); // Move it down a bit from the origin
            EvilUtils.PushRotate(0, 0, (r));
            
            Shape Vertices = new Shape(new List<Vector3> {
                new Vector3(100, 120, 0),
                new Vector3(150, 200, 0),
                new Vector3(200, 250, 0),
                new Vector3(50, 170, 0),
                new Vector3(180, 90, 0),
                new Vector3(120, 60, 0),
                new Vector3(110, 180, 0),
                new Vector3(160, 140, 0),
                new Vector3(220, 190, 0),
                new Vector3(130, 210, 0)
            });
            Vertices.CenterAroundOrigin();
            RectF bounds = Vertices.GetBoundingRectangle();
            Console.WriteLine($"Bounding Rect: X={bounds.X}, Y={bounds.Y}, Width={bounds.Width}, Height={bounds.Height}");
           //EvilUtils.SetTexture("!#need image path", new Rect(0, 0, 370, 370));
            //Shape shape = new Shape(Vertices,new Vector4(1,1,1,1));
            Shape shape = new Shape(Vertices.Vertices);
            EvilUtils.Draw(shape);
           // EvilUtils.DrawRectangle(-50f, -25f, 100.0f, 50.0f); // 100x100 rectangle
            EvilUtils.PopOrgin(); EvilUtils.PopOrgin();

            // Reset and finish
            EvilUtils.PopOrgin();
            EvilUtils.PopOrgin();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
           r += 100 * (float)e.Time;
            Console.WriteLine(r);
            if (r >= 360) r = 0;
            // Add your game logic here
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}
