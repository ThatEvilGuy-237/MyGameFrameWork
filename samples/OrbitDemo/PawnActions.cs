namespace OrbitDemo;

internal sealed class PawnActions(GameContext game, Pawns pawns, DemoSounds sounds)
{
    private const float HitLandsAfter = 0.7f;

    private Tween? _busy;
    private string? _whenDone;
    private Capsule? _hitTarget;
    private double _hitAt;

    public bool IsBusy => _busy is not null;

    public string? Last { get; private set; }

    public void Run(string action, Capsule pawn)
    {
        Last = action;
        Console.WriteLine($"action: {action}");
        game.Audio.Play(sounds.Click);
        switch (action)
        {
            case "Move":
                Move(pawn);
                break;
            case "Attack":
                Attack(pawn);
                break;
            default:
                SpinInPlace(pawn);
                break;
        }
    }

    public void Update()
    {
        PlayHitWhenItLands();
        if (_busy is { IsDone: true })
        {
            Console.WriteLine(_whenDone);
            _busy = null;
        }
    }

    private void Move(Capsule pawn)
    {
        float side = pawn.Position.X < 0f ? 1f : -1f;
        Vector3 ahead = pawn.Position + new Vector3(0f, 0f, -2f);
        Vector3 across = ahead + new Vector3(2f * side, 0f, 0f);
        _busy = game.Motion.MoveAlong(pawn, [ahead, across], 0.6f);
        _whenDone = $"arrived: {pawn.Tag} at {across.X:0},{across.Z:0}";
    }

    private void Attack(Capsule pawn)
    {
        Capsule enemy = pawns.EnemyOf(pawn);
        Vector3 start = pawn.Position;
        _busy = HopAtAndBack(pawn, enemy.Position);
        KnockBack(enemy, start);
        game.Audio.PlayAt(sounds.Hop, pawn);
        _hitTarget = enemy;
        _hitAt = game.Time.TotalSeconds + HitLandsAfter;
        _whenDone = "done: attack";
    }

    private Tween HopAtAndBack(Capsule pawn, Vector3 enemy)
    {
        Vector3 start = pawn.Position;
        Vector3 reach = Vector3.Lerp(start, enemy, 0.45f) with { Y = start.Y };
        game.Motion.Face(pawn, enemy, 0.25f);
        game.Motion.Hop(pawn, reach, 1.2f, 0.45f);
        return game.Motion.Hop(pawn, start, 0.5f, 0.4f);
    }

    private void KnockBack(Capsule enemy, Vector3 attacker)
    {
        Vector3 home = enemy.Position;
        Vector3 pushed = home + Vector3.Normalize(home - attacker) * 0.6f;
        game.Motion.Wait(enemy, HitLandsAfter);
        game.Motion.Hop(enemy, pushed with { Y = home.Y }, 0.35f, 0.2f);
        game.Motion.MoveTo(enemy, home, 0.35f);
    }

    private void SpinInPlace(Capsule pawn)
    {
        _busy = game.Motion.Spin(pawn, 360f, 0.8f);
        _whenDone = "done: wait";
    }

    private void PlayHitWhenItLands()
    {
        if (_hitTarget is null || game.Time.TotalSeconds < _hitAt)
        {
            return;
        }

        game.Audio.PlayAt(sounds.Hit, _hitTarget);
        Console.WriteLine("sound: hit");
        _hitTarget = null;
    }
}
