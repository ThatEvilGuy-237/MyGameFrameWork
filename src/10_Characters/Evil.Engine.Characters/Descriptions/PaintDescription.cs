using System.Numerics;

namespace Evil.Engine.Characters;

internal sealed record PaintDescription(string Picture, PaintWeight Weight, Vector3 ShadeTint, bool LitOnly);
