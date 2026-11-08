using Godot;

namespace Evil.Engine.Audio;

public sealed class Sound
{
    internal Sound(AudioStream native, string name)
    {
        Native = native;
        Name = name;
    }

    public string Name { get; }

    public float Seconds => (float)Native.GetLength();

    internal AudioStream Native { get; }
}
