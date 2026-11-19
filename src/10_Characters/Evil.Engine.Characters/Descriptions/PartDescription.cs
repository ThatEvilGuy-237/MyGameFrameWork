namespace Evil.Engine.Characters;

internal sealed record PartDescription(string Name, bool Outline, ShadowRole Shadow, IReadOnlyList<SurfaceDescription> Surfaces);
