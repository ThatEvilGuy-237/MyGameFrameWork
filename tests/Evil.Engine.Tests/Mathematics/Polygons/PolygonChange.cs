using System.Numerics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed record PolygonChange(Vector2[] Corners, Vector2 Move, float Degrees, float Scale);
