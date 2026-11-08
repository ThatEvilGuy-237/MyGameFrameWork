using Evil.Engine.Audio;

namespace Evil.Engine.GodotTests.Audio;

internal static class AudioTests
{
    [Test]
    public static void SoundsLoadOnceAndSayWhatTheyAre()
    {
        ContentLibrary content = new ContentLibrary(SoundsFolder());
        Sound click = content.Sound("click.wav");
        Check.Near(0.05f, click.Seconds, "a 50 ms click", 0.01f);
        Check.True(ReferenceEquals(click, content.Sound("click.wav")), "loaded once");
        void LoadMissingSound() => content.Sound("missing.wav");
        Check.Throws<FileNotFoundException>(LoadMissingSound, "'missing.wav' was not found");

        string folder = Path.Combine(Path.GetTempPath(), $"evil-engine-audio-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "notes.txt"), "not a sound");
        void LoadTextAsSound() => new ContentLibrary(folder).Sound("notes.txt");
        Check.Throws<InvalidDataException>(LoadTextAsSound, "not a .wav or .ogg sound");
    }

    [Test]
    public static void VolumeGroupsAreBuses()
    {
        using Stage stage = Stage.Open();
        AudioMixer mixer = new AudioMixer(stage.Root.Node);
        foreach (AudioGroup group in Enum.GetValues<AudioGroup>())
        {
            int index = Godot.AudioServer.GetBusIndex(group.ToString());
            Check.True(index > 0, $"{group} has a bus");
            Check.Equal("Master", Godot.AudioServer.GetBusSend(index).ToString(), $"{group} goes to the master");
        }

        mixer.SetVolume(AudioGroup.Music, 0.5f);
        Check.Near(0.5f, mixer.GetVolume(AudioGroup.Music), "half volume");
        Check.Near(-6.02f, Godot.AudioServer.GetBusVolumeDb(Godot.AudioServer.GetBusIndex("Music")), "in decibels", 0.05f);
        mixer.Mute(AudioGroup.Effects, true);
        Check.True(mixer.IsMuted(AudioGroup.Effects), "muted");
        mixer.Mute(AudioGroup.Effects, false);
        mixer.MasterVolume = 0.8f;
        Check.Near(0.8f, mixer.MasterVolume, "master volume");
        mixer.SetVolume(AudioGroup.Ambience, -2f);
        Check.True(mixer.GetVolume(AudioGroup.Ambience) < 0.001f, "no negative volume");
        mixer.MasterVolume = 1f;
        mixer.SetVolume(AudioGroup.Music, 1f);
        mixer.SetVolume(AudioGroup.Ambience, 1f);
    }

    [Test]
    public static void OneShotsPlayOnTheirGroupAndStop()
    {
        using Stage stage = Stage.Open();
        AudioMixer mixer = new AudioMixer(stage.Root.Node);
        Sound hop = new ContentLibrary(SoundsFolder()).Sound("hop.wav");
        SoundPlayback playing = mixer.Play(hop, AudioGroup.Effects, volume: 0.5f);
        Check.True(playing.IsPlaying, "playing");
        Godot.AudioStreamPlayer player = (Godot.AudioStreamPlayer)playing.Player;
        Check.Equal("Effects", player.Bus.ToString(), "on the effects bus");
        Check.Near(-6.02f, player.VolumeDb, "its own volume", 0.05f);
        playing.Stop();
        Check.True(!playing.IsPlaying, "stopped and freed");
        playing.Stop();
    }

    [Test]
    public static void SoundsCanFollowAnObject()
    {
        using Stage stage = Stage.Open();
        AudioMixer mixer = new AudioMixer(stage.Root.Node);
        Sound hit = new ContentLibrary(SoundsFolder()).Sound("hit.wav");
        WorldObject pawn = stage.Root.Add(new WorldObject("pawn"));
        SoundPlayback playing = mixer.PlayAt(hit, pawn);
        Check.True(ReferenceEquals(pawn.Node, playing.Player.GetParent()), "the sound hangs on the pawn");
        pawn.Destroy();
        Check.True(!playing.IsPlaying, "and goes with it");
        WorldObject loose = new WorldObject("loose");
        void PlayFromALooseObject() => mixer.PlayAt(hit, loose);
        Check.Throws<InvalidOperationException>(PlayFromALooseObject, "in a scene's world: World.Add it first, or use Play");
        loose.Destroy();
    }

    [Test]
    public static void MusicFadesInOnTheMusicBus()
    {
        using Stage stage = Stage.Open();
        AudioMixer mixer = new AudioMixer(stage.Root.Node);
        mixer.PlayMusic(Sounds().Sound("music.wav"), 1f);
        Check.Near(0f, mixer.Tracks[0].Level, "starts silent");
        mixer.Update(0.5f);
        Check.Near(0.5f, mixer.Tracks[0].Level, "fading in");
        mixer.Update(0.5f);
        Check.Near(1f, mixer.Tracks[0].Level, "full");
        Check.Equal("Music", mixer.Tracks[0].Player.Bus.ToString(), "on the music bus");
    }

    [Test]
    public static void ANewTrackCrossfadesTheOldOne()
    {
        using Stage stage = Stage.Open();
        AudioMixer mixer = new AudioMixer(stage.Root.Node);
        ContentLibrary content = Sounds();
        Sound other = content.Sound("hop.wav");
        mixer.PlayMusic(content.Sound("music.wav"), 0f);
        mixer.PlayMusic(other, 1f);
        Check.Equal(2, mixer.Tracks.Count, "both during the crossfade");
        mixer.Update(0.5f);
        Check.Near(0.5f, mixer.Tracks[0].Level, "old half out");
        Check.Near(0.5f, mixer.Tracks[1].Level, "new half in");
        mixer.Update(0.5f);
        Check.Equal(1, mixer.Tracks.Count, "the old track is gone");
        Check.True(ReferenceEquals(other, mixer.Music), "now playing");
        mixer.PlayMusic(other, 1f);
        Check.Equal(1, mixer.Tracks.Count, "asking for the same music changes nothing");
    }

    [Test]
    public static void StoppingMusicWithoutAFadeEndsItAtOnce()
    {
        using Stage stage = Stage.Open();
        AudioMixer mixer = new AudioMixer(stage.Root.Node);
        mixer.PlayMusic(Sounds().Sound("music.wav"), 0f);
        mixer.StopMusic(0f);
        mixer.Update(0f);
        Check.Equal(0, mixer.Tracks.Count, "stopped at once");
        Check.True(mixer.Music is null, "no music");
    }

    [Test]
    public static void RecordingNeedsAStart()
    {
        using Stage stage = Stage.Open();
        AudioMixer mixer = new AudioMixer(stage.Root.Node);
        void SaveWithoutRecording() => mixer.SaveRecording("x.wav");
        Check.Throws<InvalidOperationException>(SaveWithoutRecording, "StartRecording first");
        Check.True(!mixer.IsRecording, "not recording");
    }

    private static ContentLibrary Sounds() => new ContentLibrary(SoundsFolder());

    private static string SoundsFolder() =>
        Path.GetFullPath(Path.Combine(Godot.ProjectSettings.GlobalizePath("res://"), "..", "..", "samples", "OrbitDemo", "content", "sounds"));
}
