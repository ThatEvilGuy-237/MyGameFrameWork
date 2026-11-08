namespace OrbitDemo;

internal sealed class CaptureFlag : CommandLineFlag
{
    public override string Name => "--capture";

    public override void Read(SampleOptions options, ArgumentQueue arguments) => options.CapturePath = arguments.ValueFor(Name);
}
