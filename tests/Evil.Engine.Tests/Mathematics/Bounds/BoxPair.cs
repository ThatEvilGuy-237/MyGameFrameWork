using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Bounds;

internal sealed record BoxPair(Bounds3 A, Bounds3 B, Vector3 Point);
