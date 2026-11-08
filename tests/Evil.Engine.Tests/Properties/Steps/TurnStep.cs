using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties.Steps;

internal sealed record TurnStep(float Degrees) : TransformStep
{
    public override void ApplyTo(TransformStack stack) => stack.Rotate(Degrees);
}
