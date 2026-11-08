using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Transforms;

internal sealed record TurnStep(float Degrees) : TransformStep
{
    public override void ApplyTo(TransformStack stack) => stack.Rotate(Degrees);
}
