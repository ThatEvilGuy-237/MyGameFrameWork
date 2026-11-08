using Godot;

namespace Evil.Engine.Audio;

internal sealed class MusicTrack
{
    private float _from;
    private float _to;
    private float _elapsed;
    private float _seconds;
    private bool _stopAtEnd;
    private bool _fading;

    public MusicTrack(Sound sound, string bus, float level)
    {
        Player = new AudioStreamPlayer();
        Player.Stream = sound.Native;
        Player.Bus = bus;
        Player.Finished += PlayAgain;
        Level = level;
        ApplyLevel();
    }

    public AudioStreamPlayer Player { get; }

    public float Level { get; private set; }

    public bool IsGone { get; private set; }

    public void FadeTo(float target, float seconds, bool stopAtEnd)
    {
        _from = Level;
        _to = target;
        _elapsed = 0f;
        _seconds = Math.Max(0f, seconds);
        _stopAtEnd = stopAtEnd;
        _fading = true;
        if (_seconds <= 0f)
        {
            Advance(0f);
        }
    }

    public void Advance(float seconds)
    {
        if (IsGone || !_fading)
        {
            return;
        }

        _elapsed += seconds;
        float progress = _seconds <= 0f ? 1f : Math.Clamp(_elapsed / _seconds, 0f, 1f);
        Level = _from + (_to - _from) * progress;
        ApplyLevel();
        if (progress < 1f)
        {
            return;
        }

        _fading = false;
        if (_stopAtEnd)
        {
            Release();
        }
    }

    public void Release()
    {
        IsGone = true;
        if (GodotObject.IsInstanceValid(Player))
        {
            Player.Stop();
            Player.QueueFree();
        }
    }

    private void PlayAgain() => Player.Play();

    private void ApplyLevel()
    {
        if (GodotObject.IsInstanceValid(Player))
        {
            Player.VolumeDb = AudioMixer.Decibels(Level);
        }
    }
}
