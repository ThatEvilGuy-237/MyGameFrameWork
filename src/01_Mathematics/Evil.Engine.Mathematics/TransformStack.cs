using System.Numerics;

namespace Evil.Engine.Mathematics;

public sealed class TransformStack
{
    private readonly Stack<Matrix3x2> _saved = new Stack<Matrix3x2>();

    public Matrix3x2 Current { get; private set; } = Matrix3x2.Identity;

    public int Depth => _saved.Count;

    public void Push() => _saved.Push(Current);

    public void Pop()
    {
        if (_saved.Count == 0)
        {
            throw new InvalidOperationException("Pop() without a matching Push().");
        }

        Current = _saved.Pop();
    }

    public TransformScope Group()
    {
        Push();
        return new TransformScope(this);
    }

    public void Translate(Vector2 offset) => Current = Matrix3x2.CreateTranslation(Finite.Point(offset, "Translate's offset")) * Current;

    public void Translate(float x, float y) => Translate(new Vector2(x, y));

    public void Rotate(float degrees) => Current = Matrix3x2.CreateRotation(Angle.ToRadians(Finite.Number(degrees, "Rotate's degrees"))) * Current;

    public void Scale(Vector2 factor) => Current = Matrix3x2.CreateScale(Finite.Point(factor, "Scale's factor")) * Current;

    public void Scale(float factor) => Scale(new Vector2(factor));

    public void Scale(float x, float y) => Scale(new Vector2(x, y));

    public void Apply(Transform2D transform) => Current = transform.ToMatrix() * Current;

    public void Apply(Matrix3x2 matrix) => Current = matrix * Current;

    public Vector2 ToWorld(Vector2 local) => Vector2.Transform(local, Current);

    public Vector2 ToLocal(Vector2 world) =>
        Matrix3x2.Invert(Current, out Matrix3x2 inverse) ? Vector2.Transform(world, inverse) : world;

    public void Reset()
    {
        _saved.Clear();
        Current = Matrix3x2.Identity;
    }
}
