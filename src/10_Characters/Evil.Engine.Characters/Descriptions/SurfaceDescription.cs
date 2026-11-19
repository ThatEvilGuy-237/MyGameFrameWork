using System.Numerics;

namespace Evil.Engine.Characters;

internal sealed record SurfaceDescription(
    string Name,
    Vector3 Lit,
    Vector3 Shade,
    float Threshold,
    RimDescription? Rim,
    Vector3? Inside,
    IReadOnlyList<PaintDescription> Paints);
