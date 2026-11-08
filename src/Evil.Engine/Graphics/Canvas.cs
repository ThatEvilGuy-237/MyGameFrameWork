using System.Numerics;
using Evil.Engine.Mathematics;
using Evil.Engine.Textures;

namespace Evil.Engine.Graphics;

public sealed class Canvas
{
    private static readonly Vector2 WhiteTexel = new(0.5f, 0.5f);

    private readonly ITriangleSink _sink;
    private readonly Texture2D _white;
    private readonly TransformStack _transforms = new();
    private Vertex[] _scratch = new Vertex[192];

    internal Canvas(ITriangleSink sink, Texture2D white)
    {
        _sink = sink;
        _white = white;
    }

    public Vector2 Size { get; private set; }

    public RectF Bounds => new(0f, 0f, Size.X, Size.Y);

    public int Depth => _transforms.Depth;

    public Matrix3x2 Transform => _transforms.Current;

    public void Push() => _transforms.Push();

    public void Pop() => _transforms.Pop();

    public TransformScope Group() => _transforms.Group();

    public void Translate(float x, float y) => _transforms.Translate(x, y);

    public void Translate(Vector2 offset) => _transforms.Translate(offset);

    public void Rotate(float degrees) => _transforms.Rotate(degrees);

    public void Scale(float factor) => _transforms.Scale(factor);

    public void Scale(float x, float y) => _transforms.Scale(x, y);

    public void Apply(Transform2D transform) => _transforms.Apply(transform);

    public Vector2 ToWorld(Vector2 local) => _transforms.ToWorld(local);

    public Vector2 ToLocal(Vector2 world) => _transforms.ToLocal(world);

    public void FillRect(RectF rect, Color color) => Quad(_white.Full, rect, color);

    public void DrawRect(RectF rect, Color color, float thickness = 1f)
    {
        var edge = MathF.Min(thickness, MathF.Min(rect.Width, rect.Height) / 2f);
        if (edge <= 0f)
        {
            return;
        }

        FillRect(new RectF(rect.Left, rect.Top, rect.Width, edge), color);
        FillRect(new RectF(rect.Left, rect.Bottom - edge, rect.Width, edge), color);
        FillRect(new RectF(rect.Left, rect.Top + edge, edge, rect.Height - edge * 2f), color);
        FillRect(new RectF(rect.Right - edge, rect.Top + edge, edge, rect.Height - edge * 2f), color);
    }

    public void DrawLine(Vector2 from, Vector2 to, Color color, float thickness = 1f)
    {
        var direction = to - from;
        var length = direction.Length();
        if (length < 1e-6f || thickness <= 0f)
        {
            return;
        }

        var side = new Vector2(-direction.Y, direction.X) / length * (thickness / 2f);
        var vertices = Rent(6);
        vertices[0] = new Vertex(from + side, WhiteTexel, color);
        vertices[1] = new Vertex(to + side, WhiteTexel, color);
        vertices[2] = new Vertex(from - side, WhiteTexel, color);
        vertices[3] = new Vertex(to + side, WhiteTexel, color);
        vertices[4] = new Vertex(to - side, WhiteTexel, color);
        vertices[5] = new Vertex(from - side, WhiteTexel, color);
        Submit(_white, 6);
    }

    public void FillTriangle(Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        var vertices = Rent(3);
        vertices[0] = new Vertex(a, WhiteTexel, color);
        vertices[1] = new Vertex(b, WhiteTexel, color);
        vertices[2] = new Vertex(c, WhiteTexel, color);
        Submit(_white, 3);
    }

    public void FillCircle(Vector2 center, float radius, Color color, int segments = 32)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(segments, 3);
        if (radius <= 0f)
        {
            return;
        }

        var vertices = Rent(segments * 3);
        var previous = center + new Vector2(radius, 0f);
        for (var i = 1; i <= segments; i++)
        {
            var angle = MathF.Tau * i / segments;
            var point = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            var at = (i - 1) * 3;
            vertices[at] = new Vertex(center, WhiteTexel, color);
            vertices[at + 1] = new Vertex(previous, WhiteTexel, color);
            vertices[at + 2] = new Vertex(point, WhiteTexel, color);
            previous = point;
        }

        Submit(_white, segments * 3);
    }

    public void FillPolygon(Polygon polygon, Color color)
    {
        ArgumentNullException.ThrowIfNull(polygon);
        var triangles = polygon.Triangles;
        var vertices = Rent(triangles.Count);
        for (var i = 0; i < triangles.Count; i++)
        {
            vertices[i] = new Vertex(polygon.Points[triangles[i]], WhiteTexel, color);
        }

        Submit(_white, triangles.Count);
    }

    public void FillPolygon(Polygon polygon, TextureRegion region, Color tint)
    {
        ArgumentNullException.ThrowIfNull(polygon);
        ArgumentNullException.ThrowIfNull(region);
        var triangles = polygon.Triangles;
        var bounds = polygon.Bounds;
        var span = new Vector2(MathF.Max(bounds.Width, 1e-6f), MathF.Max(bounds.Height, 1e-6f));
        var uvSize = region.UvMax - region.UvMin;
        var vertices = Rent(triangles.Count);
        for (var i = 0; i < triangles.Count; i++)
        {
            var point = polygon.Points[triangles[i]];
            var uv = region.UvMin + (point - bounds.Position) / span * uvSize;
            vertices[i] = new Vertex(point, uv, tint);
        }

        Submit(region.Texture, triangles.Count);
    }

    public void Draw(Texture2D texture, Vector2 position) =>
        Quad(texture.Full, new RectF(position.X, position.Y, texture.Width, texture.Height), Color.White);

    public void Draw(Texture2D texture, RectF destination) => Quad(texture.Full, destination, Color.White);

    public void Draw(TextureRegion region, Vector2 position) =>
        Quad(region, new RectF(position.X, position.Y, region.Width, region.Height), Color.White);

    public void Draw(TextureRegion region, RectF destination) => Quad(region, destination, Color.White);

    public void Draw(TextureRegion region, RectF destination, Color tint) => Quad(region, destination, tint);

    internal void Begin(Vector2 size)
    {
        Size = size;
        _transforms.Reset();
    }

    internal void End()
    {
        var depth = _transforms.Depth;
        _transforms.Reset();
        if (depth != 0)
        {
            throw new InvalidOperationException($"{depth} Push() without a matching Pop() in this frame.");
        }
    }

    private void Quad(TextureRegion region, RectF destination, Color color)
    {
        ArgumentNullException.ThrowIfNull(region);
        if (destination.Width == 0f || destination.Height == 0f)
        {
            return;
        }

        var min = region.UvMin;
        var max = region.UvMax;
        var topLeft = new Vertex(destination.TopLeft, min, color);
        var topRight = new Vertex(destination.TopRight, new Vector2(max.X, min.Y), color);
        var bottomLeft = new Vertex(destination.BottomLeft, new Vector2(min.X, max.Y), color);
        var bottomRight = new Vertex(destination.BottomRight, max, color);
        var vertices = Rent(6);
        vertices[0] = topLeft;
        vertices[1] = topRight;
        vertices[2] = bottomLeft;
        vertices[3] = topRight;
        vertices[4] = bottomRight;
        vertices[5] = bottomLeft;
        Submit(region.Texture, 6);
    }

    private Span<Vertex> Rent(int count)
    {
        if (_scratch.Length < count)
        {
            _scratch = new Vertex[Math.Max(count, _scratch.Length * 2)];
        }

        return _scratch.AsSpan(0, count);
    }

    private void Submit(Texture2D texture, int count)
    {
        ObjectDisposedException.ThrowIf(texture.IsDisposed, texture);
        if (count == 0)
        {
            return;
        }

        var vertices = _scratch.AsSpan(0, count);
        var matrix = _transforms.Current;
        if (!matrix.IsIdentity)
        {
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertices[i] with { Position = Vector2.Transform(vertices[i].Position, matrix) };
            }
        }

        _sink.Submit(texture, vertices);
    }
}
