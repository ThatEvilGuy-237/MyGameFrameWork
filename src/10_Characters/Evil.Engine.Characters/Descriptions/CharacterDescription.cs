namespace Evil.Engine.Characters;

internal sealed record CharacterDescription(
    string Name,
    float Height,
    float Scale,
    string Skin,
    OutlineDescription Outline,
    PieceDescription Body,
    IReadOnlyDictionary<string, ClothingDescription> Clothing,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Outfits,
    IReadOnlyDictionary<string, ClipDescription> Clips,
    IReadOnlyDictionary<string, ExpressionDescription> Expressions,
    IReadOnlyDictionary<string, PictureDescription> Pictures);
