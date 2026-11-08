namespace Evil.Engine.Mathematics;

public readonly struct TransformScope : IDisposable
{
    private readonly TransformStack? _stack;

    internal TransformScope(TransformStack stack) => _stack = stack;

    public void Dispose() => _stack?.Pop();
}
