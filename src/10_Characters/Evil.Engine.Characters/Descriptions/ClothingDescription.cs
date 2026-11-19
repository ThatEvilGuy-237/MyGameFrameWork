namespace Evil.Engine.Characters;

internal sealed record ClothingDescription(string Slot, PieceDescription Piece, IReadOnlyList<int> Hides, IReadOnlyDictionary<string, float> Shapes);
