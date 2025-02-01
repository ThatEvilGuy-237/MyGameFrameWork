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

            EvilUtils.SetDpi();
        }




        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, windowWidth, windowHeight);
            EvilUtils.SetColor(1.0f, 0.0f, 100.0f, 100.0f);

            float x = 0f;
            float y = 0f;
            float width = 0.5f;
            float height = 0.5f;

            // Now draw the rectangle centered at (centerX, centerY)
            //EvilUtils.DrawRectangle(x, y, width, height);
            //EvilUtils.DrawLine(0f, 0.0f, 800.0f, 100.0f);

            //EvilUtils.DrawLine(0f, 10.0f, 200.0f, 100.0f);

            //EvilUtils.DrawRectangle(200f, 200f, 200.0f, 200.0f);
            EvilUtils.DrawCube();
            SwapBuffers();  // Swap the buf
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Add your game logic here
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}
