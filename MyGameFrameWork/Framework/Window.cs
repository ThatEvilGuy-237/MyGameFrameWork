using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK;
using OpenTK.Graphics.OpenGL;
// more about OpenTK https://opentk.net/learn/chapter1
// video coolors https://www.youtube.com/watch?v=eAQzayHICWA
using MyGameFrameWork.Framework.Utils;
using OpenTK.Mathematics;
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

            TestDawing();
            SwapBuffers();
        }
        float r = 0;
        private void TestDawing()
        {
            EvilUtils.SetColor(1.0f, 0.0f, 10.0f, 100.0f);


            EvilUtils.NewPush();
            EvilUtils.PushTranslate(400, 400f);
            EvilUtils.NewPush();
            EvilUtils.PushRotate(0, 0, -r);
            EvilUtils.DrawRectangle(-50f, -50, 100.0f, 100.0f);
            EvilUtils.NewPush();

            EvilUtils.PushTranslate(150, 0);
            EvilUtils.SetColor(100.0f, 1.0f, 0.0f, 100.0f);
            EvilUtils.DrawRectangle(-50f, -50f, 100.0f, 100.0f);

            EvilUtils.PopOrgin();
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
