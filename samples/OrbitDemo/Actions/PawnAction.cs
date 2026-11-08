namespace OrbitDemo;

internal abstract class PawnAction
{
    public abstract string Label { get; }

    public abstract ActionStarted Start(Capsule pawn);
}
