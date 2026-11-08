using System.Globalization;

namespace OrbitDemo;

internal sealed class FrameFlag : CommandLineFlag
{
    public override string Name => "--frame";

    public override void Read(SampleOptions options, ArgumentQueue arguments) =>
        options.CaptureFrame = int.Parse(arguments.ValueFor(Name), CultureInfo.InvariantCulture);
}
