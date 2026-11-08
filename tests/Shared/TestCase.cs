namespace Evil.Engine.Testing;

internal abstract class TestCase
{
    public abstract string Name { get; }

    public abstract void Execute();
}
