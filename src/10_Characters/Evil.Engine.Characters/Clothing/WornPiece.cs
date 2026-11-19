using Godot;

namespace Evil.Engine.Characters;

internal sealed record WornPiece(string Name, ClothingDescription Told, IReadOnlyList<MeshInstance3D> Meshes);
