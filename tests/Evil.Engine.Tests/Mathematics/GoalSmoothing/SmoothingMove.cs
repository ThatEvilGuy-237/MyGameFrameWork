using System.Numerics;

namespace Evil.Engine.Tests.Mathematics.GoalSmoothing;

internal sealed record SmoothingMove(Vector3 From, Vector3 Goal, float Sharpness, float Seconds);
