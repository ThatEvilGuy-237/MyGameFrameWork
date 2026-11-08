namespace OrbitDemo;

internal sealed class ActionRunner(AudioMixer audio, Sound click)
{
    private ActionStarted? _running;

    public bool IsBusy => _running is not null;

    public PawnAction? Last { get; private set; }

    public void Run(PawnAction action, Capsule pawn)
    {
        Last = action;
        Console.WriteLine($"action: {action.Label}");
        audio.Play(click);
        _running = action.Start(pawn);
    }

    public void Update()
    {
        if (_running is not null && _running.Busy.IsDone)
        {
            Console.WriteLine(_running.WhenDone);
            _running = null;
        }
    }
}
