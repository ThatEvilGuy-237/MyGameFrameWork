using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.World;

public sealed class Material
{
    public Material()
        : this(Color.White)
    {
    }

    public Material(Color color) => Color = color;

    public Color Color
    {
        get => Native.AlbedoColor.ToEvil();
        set
        {
            Native.AlbedoColor = value.ToGodot();
            Native.Transparency = value.A < 1f
                ? BaseMaterial3D.TransparencyEnum.Alpha
                : BaseMaterial3D.TransparencyEnum.Disabled;
        }
    }

    public float Roughness
    {
        get => Native.Roughness;
        set => Native.Roughness = Math.Clamp(value, 0f, 1f);
    }

    public float Metallic
    {
        get => Native.Metallic;
        set => Native.Metallic = Math.Clamp(value, 0f, 1f);
    }

    public Color Glow
    {
        get => Native.EmissionEnabled ? Native.Emission.ToEvil() : Color.Black;
        set
        {
            Native.EmissionEnabled = value.R > 0f || value.G > 0f || value.B > 0f;
            Native.Emission = value.ToGodot();
        }
    }

    public float GlowStrength
    {
        get => Native.EmissionEnergyMultiplier;
        set => Native.EmissionEnergyMultiplier = Math.Max(0f, value);
    }

    public bool Unshaded
    {
        get => Native.ShadingMode == BaseMaterial3D.ShadingModeEnum.Unshaded;
        set => Native.ShadingMode = value
            ? BaseMaterial3D.ShadingModeEnum.Unshaded
            : BaseMaterial3D.ShadingModeEnum.PerPixel;
    }

    internal StandardMaterial3D Native { get; } = new();

    public static Material Matte(Color color) => new(color) { Roughness = 1f };

    public static Material Shiny(Color color) => new(color) { Roughness = 0.2f };

    public static Material Metal(Color color) => new(color) { Metallic = 1f, Roughness = 0.3f };

    public static Material Glowing(Color color, float strength = 2f) => new(color) { Glow = color, GlowStrength = strength };
}
