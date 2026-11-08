using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal sealed record NestedProbe(Pose Outer, Pose Middle, Pose Inner, Vector3 Point);
