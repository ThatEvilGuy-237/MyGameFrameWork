namespace Evil.Engine.Characters;

internal sealed record ClipDescription(float Seconds, bool Loops, IReadOnlyList<FaceMoment> Faces);
