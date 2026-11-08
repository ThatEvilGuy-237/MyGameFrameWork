using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.Lighting;

public sealed class Lamp : Light
{
    public Lamp()
        : this(Color.White, 5f)
    {
    }

    public Lamp(Color color, float range)
        : base(new OmniLight3D())
    {
        Color = color;
        Range = range;
    }

    public float Range
    {
        get => ((OmniLight3D)Node).OmniRange;
        set => ((OmniLight3D)Node).OmniRange = Math.Max(0.01f, value);
    }

    public float Falloff
    {
        get => ((OmniLight3D)Node).OmniAttenuation;
        set => ((OmniLight3D)Node).OmniAttenuation = Math.Max(0f, value);
    }

    public static Lamp Warm(float range) => new(new Color(1f, 0.72f, 0.42f), range) { Intensity = 1.5f };

    public static Lamp Cool(float range) => new(new Color(0.65f, 0.8f, 1f), range) { Intensity = 1.2f };
}
