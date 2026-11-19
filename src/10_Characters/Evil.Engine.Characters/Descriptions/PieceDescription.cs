namespace Evil.Engine.Characters;

internal sealed record PieceDescription(string File, IReadOnlyList<PartDescription> Parts);
