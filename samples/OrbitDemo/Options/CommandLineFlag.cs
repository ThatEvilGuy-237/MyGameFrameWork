namespace OrbitDemo;

internal abstract class CommandLineFlag
{
    public abstract string Name { get; }

    public abstract void Read(SampleOptions options, ArgumentQueue arguments);
}
