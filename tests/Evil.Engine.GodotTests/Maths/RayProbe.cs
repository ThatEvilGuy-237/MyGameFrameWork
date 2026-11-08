using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal sealed record RayProbe(Bounds3 Box, float Radius, Ray3 Ray);
