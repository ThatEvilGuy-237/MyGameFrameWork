using Godot;

namespace Evil.Engine.Audio;

public sealed class SoundPlayback
{
    private readonly Node _player;

    internal SoundPlayback(Node player, Sound sound, AudioGroup group)
    {
        _player = player;
        Sound = sound;
        Group = group;
    }

    public Sound Sound { get; }

    public AudioGroup Group { get; }

    public bool IsPlaying => !IsGone(_player) && Playing(_player);

    internal Node Player => _player;

    public void Stop()
    {
        if (IsGone(_player))
        {
            return;
        }

        switch (_player)
        {
            case AudioStreamPlayer flat:
                flat.Stop();
                break;
            case AudioStreamPlayer3D placed:
                placed.Stop();
                break;
        }

        _player.QueueFree();
    }

    private static bool IsGone(Node player)
    {
        if (!GodotObject.IsInstanceValid(player))
        {
            return true;
        }

        for (Node? current = player; current is not null; current = current.GetParent())
        {
            if (current.IsQueuedForDeletion())
            {
                return true;
            }
        }

        return false;
    }

    private static bool Playing(Node player) => player switch
    {
        AudioStreamPlayer flat => flat.Playing,
        AudioStreamPlayer3D placed => placed.Playing,
        _ => false,
    };
}
