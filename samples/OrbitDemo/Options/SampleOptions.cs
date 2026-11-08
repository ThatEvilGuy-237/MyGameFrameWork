namespace OrbitDemo;

internal sealed class SampleOptions
{
    private const string Usage = "Use --capture <file.png> [--frame <n>] [--drive] [--record-audio <file.wav>].";

    private static readonly CommandLineFlag[] Flags = [new CaptureFlag(), new FrameFlag(), new DriveFlag(), new RecordAudioFlag()];

    public string? CapturePath { get; set; }

    public int CaptureFrame { get; set; } = 90;

    public bool Driven { get; set; }

    public string? RecordAudioPath { get; set; }

    public static SampleOptions Parse(IReadOnlyList<string> args)
    {
        SampleOptions options = new SampleOptions();
        ArgumentQueue arguments = new ArgumentQueue(args);
        while (arguments.TryTake(out string? argument))
        {
            FlagFor(argument).Read(options, arguments);
        }

        return options;
    }

    private static CommandLineFlag FlagFor(string argument)
    {
        foreach (CommandLineFlag flag in Flags)
        {
            if (flag.Name == argument)
            {
                return flag;
            }
        }

        throw new ArgumentException($"Unknown argument '{argument}'. {Usage}");
    }
}
