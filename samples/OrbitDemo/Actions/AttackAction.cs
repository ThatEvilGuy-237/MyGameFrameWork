namespace OrbitDemo;

internal sealed class AttackAction(GameContext game, Pawns pawns, DemoSounds sounds, SoundCues cues) : PawnAction
{
    private const float HitLandsAfter = 0.7f;

    public override string Label => "Attack";

    public override ActionStarted Start(Capsule pawn)
    {
        Capsule enemy = pawns.EnemyOf(pawn);
        Vector3 start = pawn.Position;
        Tween hop = HopAtAndBack(pawn, enemy.Position);
        KnockBack(enemy, start);
        game.Audio.PlayAt(sounds.Hop, pawn);
        cues.Add(new SoundCue(sounds.Hit, enemy, game.Time.TotalSeconds + HitLandsAfter));
        return new ActionStarted(hop, "done: attack");
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
}
