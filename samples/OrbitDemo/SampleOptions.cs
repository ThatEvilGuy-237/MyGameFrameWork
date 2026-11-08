namespace OrbitDemo;

internal sealed record SampleOptions(string? CapturePath, int CaptureFrame, bool Driven)
{
    public static SampleOptions Parse(IReadOnlyList<string> args)
    {
        string? capturePath = null;
        var captureFrame = 90;
        var driven = false;
        for (var i = 0; i < args.Count; i++)
        {
            switch (args[i])
            {
                case "--capture" when i + 1 < args.Count:
                    capturePath = args[++i];
                    break;
                case "--frame" when i + 1 < args.Count:
                    captureFrame = int.Parse(args[++i]);
                    break;
                case "--drive":
                    driven = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{args[i]}'. Use --capture <file.png> [--frame <n>] [--drive].");
            }
        }

        return new SampleOptions(capturePath, captureFrame, driven);
    }
}
