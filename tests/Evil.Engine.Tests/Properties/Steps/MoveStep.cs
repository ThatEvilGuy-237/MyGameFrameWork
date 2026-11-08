using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties.Steps;

internal sealed record MoveStep(Vector2 Offset) : TransformStep
{
    public override void ApplyTo(TransformStack stack) => stack.Translate(Offset);
}
