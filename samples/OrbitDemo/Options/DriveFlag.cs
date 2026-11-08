namespace OrbitDemo;

internal sealed class DriveFlag : CommandLineFlag
{
    public override string Name => "--drive";

    public override void Read(SampleOptions options, ArgumentQueue arguments) => options.Driven = true;
}
