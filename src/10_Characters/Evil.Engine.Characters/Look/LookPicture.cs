using Godot;
using Vector4 = System.Numerics.Vector4;

namespace Evil.Engine.Characters;

internal sealed record LookPicture(Texture2D Texture, Vector4 Region);
