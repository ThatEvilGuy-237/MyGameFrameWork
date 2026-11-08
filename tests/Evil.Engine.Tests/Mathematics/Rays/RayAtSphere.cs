using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rays;

internal sealed record RayAtSphere(Vector3 Center, float Radius, Ray3 Ray);
