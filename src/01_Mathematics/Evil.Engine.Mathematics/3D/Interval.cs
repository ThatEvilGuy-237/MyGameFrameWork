namespace Evil.Engine.Mathematics;

internal readonly record struct Interval(float Start, float End)
{
    public static Interval Ahead => new Interval(0f, float.PositiveInfinity);

    public static Interval Everywhere => new Interval(float.NegativeInfinity, float.PositiveInfinity);

    public static Interval Nowhere => new Interval(float.PositiveInfinity, float.NegativeInfinity);

    public bool IsEmpty => Start > End;

    public static Interval Between(float a, float b) => new Interval(MathF.Min(a, b), MathF.Max(a, b));

    public Interval Overlap(Interval other) => new Interval(MathF.Max(Start, other.Start), MathF.Min(End, other.End));
}
