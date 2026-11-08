using Godot;
using Color = Evil.Engine.Core.Color;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Lighting;

public sealed class Spotlight : Light
{
    public Spotlight()
        : this(Color.White, 10f, 30f)
    {
    }

    public Spotlight(Color color, float range, float angle)
        : base(NewSpot(angle))
    {
        Color = color;
        Range = range;
        Direction = -Vector3.UnitY;
    }

    public Vector3 Direction
    {
        get => Pointing;
        set => Pointing = value;
    }

    public float Range
    {
        get => ((SpotLight3D)Node).SpotRange;
        set => ((SpotLight3D)Node).SpotRange = Math.Max(0.01f, value);
    }

    public float Angle
    {
        get => ((SpotLight3D)Node).SpotAngle;
        set => ((SpotLight3D)Node).SpotAngle = CheckedAngle(value);
    }

    public float Softness
    {
        get => ((SpotLight3D)Node).SpotAngleAttenuation;
        set => ((SpotLight3D)Node).SpotAngleAttenuation = Math.Max(0f, value);
    }

    private static SpotLight3D NewSpot(float angle)
    {
        float checkedAngle = CheckedAngle(angle);
        SpotLight3D light = new SpotLight3D();
        light.SpotAngle = checkedAngle;
        return light;
    }

    private static float CheckedAngle(float angle)
    {
        if (angle <= 0f || angle >= 180f)
        {
            throw new ArgumentOutOfRangeException(nameof(angle), $"A spotlight's angle is measured from its middle to its edge and must be between 0 and 180 degrees, got {angle}.");
        }

        return angle;
    }
}
