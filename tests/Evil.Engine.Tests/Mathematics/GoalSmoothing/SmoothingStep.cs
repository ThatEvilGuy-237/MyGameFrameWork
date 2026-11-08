namespace Evil.Engine.Tests.Mathematics.GoalSmoothing;

internal sealed record SmoothingStep(float From, float Goal, float Sharpness, float Seconds);
