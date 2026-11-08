namespace Evil.Engine.Sample;

internal sealed record SampleOptions(string? CapturePath, int CaptureFrame)
{
    public static SampleOptions Parse(string[] args)
    {
        string? capturePath = null;
        var captureFrame = 90;
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--capture" when i + 1 < args.Length:
                    capturePath = args[++i];
                    break;
                case "--frame" when i + 1 < args.Length:
                    captureFrame = int.Parse(args[++i]);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{args[i]}'. Use --capture <file.png> [--frame <n>].");
            }
        }

        return new SampleOptions(capturePath, captureFrame);
    }
}
