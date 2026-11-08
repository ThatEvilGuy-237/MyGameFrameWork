using Evil.Engine.Platform;
using Evil.Engine.World;
using Godot;
using Color = Evil.Engine.Core.Color;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Lighting;

public abstract class Light : WorldObject
{
    private protected Light(Light3D node)
        : base(node)
    {
    }

    public Color Color
    {
        get => Native.LightColor.ToEvil();
        set => Native.LightColor = value.ToGodot();
    }

    public float Intensity
    {
        get => Native.LightEnergy;
        set => Native.LightEnergy = Math.Max(0f, value);
    }

    public bool Shadows
    {
        get => Native.ShadowEnabled;
        set => Native.ShadowEnabled = value;
    }

    private protected Vector3 Pointing
    {
        get => (-Node.Basis.Z).Normalized().ToEvil();
        set
        {
            if (value.LengthSquared() < 1e-12f)
            {
                throw new ArgumentException($"A {GetType().Name.ToLowerInvariant()} needs a direction with a length.", nameof(value));
            }

            Node.Quaternion = Facing.Along(value.ToGodot()).GetRotationQuaternion();
        }
    }

    private Light3D Native => (Light3D)Node;
}
