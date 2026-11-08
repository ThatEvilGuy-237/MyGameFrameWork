using Godot;
using EvilSound = Evil.Engine.Audio.Sound;

namespace Evil.Engine.Hosting;

internal sealed class SoundLoader : ContentLoader<EvilSound, AudioStream>
{
    private readonly SoundFormat[] _formats = [new WavFormat(), new OggFormat()];

    protected override string Kind => "a sound";

    protected override AudioStream ReadFile(ContentLocation location)
    {
        foreach (SoundFormat format in _formats)
        {
            if (format.Reads(location))
            {
                return format.Read(location) ?? throw location.Unreadable(Kind);
            }
        }

        throw new InvalidDataException($"Content: '{location.Path}' is not a .wav or .ogg sound.");
    }

    protected override EvilSound Wrap(AudioStream native, ContentLocation location) => new EvilSound(native, location.Path);

    protected override AudioStream NativeOf(EvilSound content) => content.Native;
}
