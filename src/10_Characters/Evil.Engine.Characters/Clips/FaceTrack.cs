namespace Evil.Engine.Characters;

internal sealed class FaceTrack
{
    public const string Neutral = "neutral";

    private readonly IReadOnlyList<FaceMoment> _moments;

    public FaceTrack(IReadOnlyList<FaceMoment> moments) => _moments = moments;

    public string At(float second)
    {
        string face = Neutral;
        foreach (FaceMoment moment in _moments)
        {
            if (moment.Second > second)
            {
                break;
            }

            face = moment.Expression;
        }

        return face;
    }
}
