using Evil.Engine.World;
using Godot;

namespace Evil.Engine.Audio;

public sealed class AudioMixer
{
    private const float Silent = 0.0001f;

    private readonly Node _root;
    private readonly List<MusicTrack> _tracks = [];
    private AudioEffectRecord? _recorder;

    internal AudioMixer(Node root)
    {
        _root = root;
        foreach (AudioGroup group in Enum.GetValues<AudioGroup>())
        {
            EnsureBus(group);
        }
    }

    public Sound? Music { get; private set; }

    public bool IsRecording => _recorder is not null && _recorder.IsRecordingActive();

    public float MasterVolume
    {
        get => Linear(0);
        set => SetBusVolume(0, value);
    }

    internal IReadOnlyList<MusicTrack> Tracks => _tracks;

    public float GetVolume(AudioGroup group) => Linear(BusIndex(group));

    public void SetVolume(AudioGroup group, float volume) => SetBusVolume(BusIndex(group), volume);

    public bool IsMuted(AudioGroup group) => AudioServer.IsBusMute(BusIndex(group));

    public void Mute(AudioGroup group, bool muted) => AudioServer.SetBusMute(BusIndex(group), muted);

    public SoundPlayback Play(Sound sound, AudioGroup group = AudioGroup.Effects, float volume = 1f, float pitch = 1f)
    {
        ArgumentNullException.ThrowIfNull(sound);
        AudioStreamPlayer player = new AudioStreamPlayer();
        player.Stream = sound.Native;
        player.Bus = BusName(group);
        player.VolumeDb = Decibels(volume);
        player.PitchScale = Math.Max(0.01f, pitch);
        player.Finished += player.QueueFree;
        _root.AddChild(player);
        player.Play();
        return new SoundPlayback(player, sound, group);
    }

    public SoundPlayback PlayAt(Sound sound, WorldObject source, AudioGroup group = AudioGroup.Effects, float volume = 1f)
    {
        ArgumentNullException.ThrowIfNull(sound);
        ArgumentNullException.ThrowIfNull(source);
        ObjectDisposedException.ThrowIf(source.IsDestroyed, source);
        if (!source.Node.IsInsideTree())
        {
            throw new InvalidOperationException($"AudioMixer.PlayAt needs '{source.Name}' in a scene's world.");
        }

        AudioStreamPlayer3D player = new AudioStreamPlayer3D();
        player.Stream = sound.Native;
        player.Bus = BusName(group);
        player.VolumeDb = Decibels(volume);
        player.Finished += player.QueueFree;
        source.Node.AddChild(player);
        player.Play();
        return new SoundPlayback(player, sound, group);
    }

    public void PlayMusic(Sound sound, float fadeSeconds = 1f)
    {
        ArgumentNullException.ThrowIfNull(sound);
        if (ReferenceEquals(Music, sound))
        {
            return;
        }

        foreach (MusicTrack old in _tracks)
        {
            old.FadeTo(0f, fadeSeconds, stopAtEnd: true);
        }

        AudioStreamPlayer player = new AudioStreamPlayer();
        player.Stream = sound.Native;
        player.Bus = BusName(AudioGroup.Music);
        player.Finished += () => player.Play();
        _root.AddChild(player);
        MusicTrack track = new MusicTrack(player, fadeSeconds > 0f ? 0f : 1f);
        track.FadeTo(1f, fadeSeconds, stopAtEnd: false);
        player.Play();
        _tracks.Add(track);
        Music = sound;
    }

    public void StopMusic(float fadeSeconds = 1f)
    {
        foreach (MusicTrack track in _tracks)
        {
            track.FadeTo(0f, fadeSeconds, stopAtEnd: true);
        }

        Music = null;
    }

    public void StartRecording()
    {
        if (_recorder is null)
        {
            _recorder = new AudioEffectRecord();
            AudioServer.AddBusEffect(0, _recorder);
        }

        _recorder.SetRecordingActive(true);
    }

    public void SaveRecording(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (_recorder is null || !_recorder.IsRecordingActive())
        {
            throw new InvalidOperationException("AudioMixer.SaveRecording needs StartRecording first.");
        }

        AudioStreamWav? recording = _recorder.GetRecording();
        _recorder.SetRecordingActive(false);
        if (recording is null)
        {
            throw new InvalidOperationException("AudioMixer.SaveRecording: nothing was recorded.");
        }

        string? folder = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        Error error = recording.SaveToWav(path);
        recording.Dispose();
        if (error != Error.Ok)
        {
            throw new IOException($"Could not save the recording to '{path}': {error}.");
        }
    }

    public void StopAll()
    {
        foreach (MusicTrack track in _tracks)
        {
            track.Release();
        }

        _tracks.Clear();
        Music = null;
        foreach (Node child in _root.GetChildren())
        {
            if (child is AudioStreamPlayer player)
            {
                player.Stop();
                player.QueueFree();
            }
        }
    }

    internal void Close()
    {
        StopAll();
        if (_recorder is null)
        {
            return;
        }

        _recorder.SetRecordingActive(false);
        for (int i = AudioServer.GetBusEffectCount(0) - 1; i >= 0; i--)
        {
            if (ReferenceEquals(AudioServer.GetBusEffect(0, i), _recorder))
            {
                AudioServer.RemoveBusEffect(0, i);
            }
        }

        _recorder.Dispose();
        _recorder = null;
    }

    internal void Update(float seconds)
    {
        for (int i = _tracks.Count - 1; i >= 0; i--)
        {
            _tracks[i].Advance(seconds);
            if (_tracks[i].IsGone)
            {
                _tracks.RemoveAt(i);
            }
        }
    }

    internal static float Decibels(float volume) => Mathf.LinearToDb(Math.Max(volume, Silent));

    internal static string BusName(AudioGroup group) => group.ToString();

    private static int BusIndex(AudioGroup group) => AudioServer.GetBusIndex(BusName(group));

    private static void EnsureBus(AudioGroup group)
    {
        string name = BusName(group);
        if (AudioServer.GetBusIndex(name) >= 0)
        {
            return;
        }

        AudioServer.AddBus();
        int index = AudioServer.BusCount - 1;
        AudioServer.SetBusName(index, name);
        AudioServer.SetBusSend(index, "Master");
    }

    private static float Linear(int bus) => Mathf.DbToLinear(AudioServer.GetBusVolumeDb(bus));

    private static void SetBusVolume(int bus, float volume) => AudioServer.SetBusVolumeDb(bus, Decibels(Math.Clamp(volume, 0f, 4f)));
}
