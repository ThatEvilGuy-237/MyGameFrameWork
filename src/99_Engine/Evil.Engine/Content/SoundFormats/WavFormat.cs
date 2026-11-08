using Godot;

namespace Evil.Engine.Hosting;

internal sealed class WavFormat : SoundFormat
{
    protected override string Extension => ".wav";

    public override AudioStream? Read(ContentLocation location) => AudioStreamWav.LoadFromFile(location.Full);
}
