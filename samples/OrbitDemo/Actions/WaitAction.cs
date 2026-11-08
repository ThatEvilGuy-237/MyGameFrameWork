namespace OrbitDemo;

internal sealed class WaitAction(MotionPlayer motion) : PawnAction
{
    public override string Label => "Wait";

    public override ActionStarted Start(Capsule pawn) => new ActionStarted(motion.Spin(pawn, 360f, 0.8f), "done: wait");
}
