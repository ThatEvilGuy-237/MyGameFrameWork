using Godot;

namespace Evil.Engine.Hosting;

internal abstract class SoundFormat
{
    protected abstract string Extension { get; }

    public bool Reads(ContentLocation location) => location.Extension == Extension;

    public abstract AudioStream? Read(ContentLocation location);
}
