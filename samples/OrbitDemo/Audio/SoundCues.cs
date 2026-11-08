namespace OrbitDemo;

internal sealed class SoundCues(AudioMixer audio)
{
    private readonly List<SoundCue> _waiting = new List<SoundCue>();

    public void Add(SoundCue cue) => _waiting.Add(cue);

    public void Update(double now)
    {
        for (int i = _waiting.Count - 1; i >= 0; i--)
        {
            SoundCue cue = _waiting[i];
            if (now < cue.At)
            {
                continue;
            }

            audio.PlayAt(cue.Sound, cue.From);
            Console.WriteLine($"sound: {Path.GetFileNameWithoutExtension(cue.Sound.Name)}");
            _waiting.RemoveAt(i);
        }
    }
}
