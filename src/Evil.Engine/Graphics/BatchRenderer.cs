using System.Numerics;
using Evil.Engine.Textures;
using OpenTK.Graphics.OpenGL4;

namespace Evil.Engine.Graphics;

internal sealed class BatchRenderer : ITriangleSink, IDisposable
{
    public const int Capacity = 3 * 4096;

    private readonly Vertex[] _vertices = new Vertex[Capacity];
    private readonly ShaderProgram _shader;
    private readonly int _vertexArray;
    private readonly int _vertexBuffer;
    private Texture2D? _texture;
    private int _count;
    private bool _disposed;

    public BatchRenderer()
    {
        _shader = ShaderProgram.FromEmbedded("sprite.vert.glsl", "sprite.frag.glsl");
        _vertexArray = GL.GenVertexArray();
        _vertexBuffer = GL.GenBuffer();
        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, Capacity * Vertex.SizeInBytes, IntPtr.Zero, BufferUsageHint.StreamDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, Vertex.PositionOffset);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, Vertex.TexCoordOffset);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, Vertex.ColorOffset);
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public int DrawCalls { get; private set; }

    public int Triangles { get; private set; }

    public void Begin(Vector2 viewport)
    {
        DrawCalls = 0;
        Triangles = 0;
        _count = 0;
        _texture = null;
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _shader.Use();
        _shader.Set("u_viewport", viewport);
        _shader.Set("u_texture", 0);
    }

    public void Submit(Texture2D texture, ReadOnlySpan<Vertex> vertices)
    {
        if (!ReferenceEquals(_texture, texture))
        {
            Flush();
            _texture = texture;
        }

        while (!vertices.IsEmpty)
        {
            if (_count == Capacity)
            {
                Flush();
            }

            var take = Math.Min(Capacity - _count, vertices.Length);
            vertices[..take].CopyTo(_vertices.AsSpan(_count));
            _count += take;
            vertices = vertices[take..];
        }
    }

    public void End()
    {
        Flush();
        _texture = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        GL.DeleteBuffer(_vertexBuffer);
        GL.DeleteVertexArray(_vertexArray);
        _shader.Dispose();
    }

    private void Flush()
    {
        if (_count == 0 || _texture is null)
        {
            return;
        }

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture.Handle);
        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, Capacity * Vertex.SizeInBytes, IntPtr.Zero, BufferUsageHint.StreamDraw);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _count * Vertex.SizeInBytes, _vertices);
        GL.DrawArrays(PrimitiveType.Triangles, 0, _count);
        DrawCalls++;
        Triangles += _count / 3;
        _count = 0;
    }
}
