using Evil.Engine.Platform;
using Evil.Engine.World;
using Godot;
using Color = Evil.Engine.Core.Color;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Lighting;

public class Sun : WorldObject
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
        get => (-Node.Basis.Z).ToEvil();
        set
        {
            if (value.LengthSquared() < 1e-12f)
            {
                throw new ArgumentException("The sun needs a direction with a length.", nameof(value));
            }

            Node.Basis = Facing.Along(value.ToGodot());
        }
    }

    public Color Color
    {
        get => Light.LightColor.ToEvil();
        set => Light.LightColor = value.ToGodot();
    }

    public float Intensity
    {
        get => Light.LightEnergy;
        set => Light.LightEnergy = value;
    }

    public bool Shadows
    {
        get => Light.ShadowEnabled;
        set => Light.ShadowEnabled = value;
    }

    private DirectionalLight3D Light => (DirectionalLight3D)Node;
}
