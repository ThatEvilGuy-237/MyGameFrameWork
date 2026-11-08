using Godot;
using Color = Evil.Engine.Core.Color;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Lighting;

public sealed class Sun : Light
{
    public Sun()
        : this(new Vector3(-0.5f, -1f, -0.6f))
    {
    }

    public Sun(Vector3 direction)
        : base(new DirectionalLight3D())
    {
        Direction = direction;
        Shadows = true;
    }

    public Vector3 Direction
    {
        get => Pointing;
        set => Pointing = value;
    }

    public float ShadowDistance
    {
        get => ((DirectionalLight3D)Node).DirectionalShadowMaxDistance;
        set => ((DirectionalLight3D)Node).DirectionalShadowMaxDistance = Math.Max(1f, value);
    }

    public static Sun Noon() => Preset(new Vector3(-0.3f, -1f, -0.2f), new Color(1f, 0.98f, 0.92f), 1.1f);

    public static Sun Golden() => Preset(new Vector3(-1f, -0.45f, -0.3f), new Color(1f, 0.78f, 0.5f), 1f);

    public static Sun Moonlight() => Preset(new Vector3(0.4f, -1f, 0.5f), new Color(0.6f, 0.7f, 1f), 0.35f);

    private static Sun Preset(Vector3 direction, Color color, float intensity)
    {
        Sun sun = new Sun(direction);
        sun.Color = color;
        sun.Intensity = intensity;
        return sun;
    }
}
