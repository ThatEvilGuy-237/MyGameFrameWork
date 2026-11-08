namespace OrbitDemo;

internal sealed class RecordAudioFlag : CommandLineFlag
{
    public override string Name => "--record-audio";

    public override void Read(SampleOptions options, ArgumentQueue arguments) => options.RecordAudioPath = arguments.ValueFor(Name);
}
