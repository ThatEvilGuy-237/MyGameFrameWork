using System.Numerics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed record RegularShape(Vector2 Center, float Radius, int Sides, float Turn);
