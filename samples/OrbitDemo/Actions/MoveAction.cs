namespace OrbitDemo;

internal sealed class MoveAction(MotionPlayer motion) : PawnAction
{
    public override string Label => "Move";

    public override ActionStarted Start(Capsule pawn)
    {
        float side = pawn.Position.X < 0f ? 1f : -1f;
        Vector3 ahead = pawn.Position + new Vector3(0f, 0f, -2f);
        Vector3 across = ahead + new Vector3(2f * side, 0f, 0f);
        Tween walk = motion.MoveAlong(pawn, [ahead, across], 0.6f);
        return new ActionStarted(walk, $"arrived: {pawn.Tag} at {across.X:0},{across.Z:0}");
    }
}
