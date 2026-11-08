using Godot;

namespace Evil.Engine.Hosting;

internal sealed class OggFormat : SoundFormat
{
    protected override string Extension => ".ogg";

    public override AudioStream? Read(ContentLocation location) => AudioStreamOggVorbis.LoadFromFile(location.Full);
}
