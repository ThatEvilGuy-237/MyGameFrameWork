using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Poses;

internal sealed record NestedPoses(Pose Parent, Pose Child, Vector3 Point);
