using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;
using Vector2 = System.Numerics.Vector2;

namespace Evil.Engine.World;

public sealed class Material
{
    private Texture? _texture;

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

    public Texture? Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            Native.AlbedoTexture = value?.Native;
        }
    }

    public Vector2 TextureScale
    {
        get => new Vector2(Native.Uv1Scale.X, Native.Uv1Scale.Y);
        set => Native.Uv1Scale = new Godot.Vector3(value.X, value.Y, 1f);
    }

    public bool Unshaded
    {
        get => Native.ShadingMode == BaseMaterial3D.ShadingModeEnum.Unshaded;
        set => Native.ShadingMode = value
            ? BaseMaterial3D.ShadingModeEnum.Unshaded
            : BaseMaterial3D.ShadingModeEnum.PerPixel;
    }

    internal StandardMaterial3D Native { get; } = new StandardMaterial3D();

    public static Material Matte(Color color)
    {
        Material material = new Material(color);
        material.Roughness = 1f;
        return material;
    }

    public static Material Shiny(Color color)
    {
        Material material = new Material(color);
        material.Roughness = 0.2f;
        return material;
    }

    public static Material Metal(Color color)
    {
        Material material = new Material(color);
        material.Metallic = 1f;
        material.Roughness = 0.3f;
        return material;
    }

    public static Material Glowing(Color color, float strength = 2f)
    {
        Material material = new Material(color);
        material.Glow = color;
        material.GlowStrength = strength;
        return material;
    }
}
