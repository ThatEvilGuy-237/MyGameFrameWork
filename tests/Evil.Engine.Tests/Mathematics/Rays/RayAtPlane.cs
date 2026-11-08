using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rays;

internal sealed record RayAtPlane(Vector3 Point, Vector3 Normal, Ray3 Ray);
