namespace Evil.Engine.Cameras;

public sealed record OrbitLimits(
    float MinDistance = 2f,
    float MaxDistance = 60f,
    float MinPitch = 5f,
    float MaxPitch = 89f)
{
    public static OrbitLimits Default { get; } = new OrbitLimits();

    internal void Validate()
    {
        if (MinDistance <= 0f || MaxDistance < MinDistance)
        {
            throw new ArgumentException($"OrbitLimits: the distance range must be above 0 and run low to high, got {MinDistance} to {MaxDistance}.");
        }

        if (MinPitch < -89f || MaxPitch > 89f || MaxPitch < MinPitch)
        {
            throw new ArgumentException($"OrbitLimits: the pitch range must sit within -89 to 89 degrees and run low to high, got {MinPitch} to {MaxPitch}.");
        }
    }
}
