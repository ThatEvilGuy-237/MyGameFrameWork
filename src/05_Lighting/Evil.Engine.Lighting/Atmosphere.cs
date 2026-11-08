using Evil.Engine.Cameras;
using Evil.Engine.Platform;
using Godot;
using Color = Evil.Engine.Core.Color;

namespace Evil.Engine.Lighting;

public sealed class Atmosphere
{
    private Background _background = Background.ClearColor;
    private Fog? _fog;
    private bool _ambientFromSky;

    public Atmosphere()
    {
        Background = Background.ClearColor;
        AmbientColor = new Color(0.75f, 0.78f, 0.85f);
        AmbientStrength = 0.5f;
        ToneMapping = ToneMapping.Filmic;
    }

    public Background Background
    {
        get => _background;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _background = value;
            switch (value)
            {
                case SolidBackground solid:
                    Native.BackgroundMode = Godot.Environment.BGMode.Color;
                    Native.BackgroundColor = solid.Color.ToGodot();
                    break;
                case GradientBackground gradient:
                    Native.BackgroundMode = Godot.Environment.BGMode.Sky;
                    Native.Sky = SkyFor(gradient);
                    break;
                default:
                    Native.BackgroundMode = Godot.Environment.BGMode.ClearColor;
                    break;
            }

            RefreshAmbient();
        }
    }

    public Color AmbientColor
    {
        get => Native.AmbientLightColor.ToEvil();
        set => Native.AmbientLightColor = value.ToGodot();
    }

    public float AmbientStrength
    {
        get => Native.AmbientLightEnergy;
        set => Native.AmbientLightEnergy = Math.Max(0f, value);
    }

    public bool AmbientFromSky
    {
        get => _ambientFromSky;
        set
        {
            _ambientFromSky = value;
            RefreshAmbient();
        }
    }

    public Fog? Fog
    {
        get => _fog;
        set
        {
            _fog = value;
            Native.FogEnabled = value is not null;
            if (value is not null)
            {
                Native.FogLightColor = value.Color.ToGodot();
                Native.FogDensity = Math.Max(0f, value.Density);
                Native.FogSkyAffect = 0.25f;
            }
        }
    }

    public float Glow
    {
        get => Native.GlowEnabled ? Native.GlowIntensity : 0f;
        set
        {
            Native.GlowEnabled = value > 0f;
            Native.GlowIntensity = Math.Max(0f, value);
        }
    }

    public ToneMapping ToneMapping
    {
        get => Native.TonemapMode switch
        {
            Godot.Environment.ToneMapper.Filmic => ToneMapping.Filmic,
            Godot.Environment.ToneMapper.Aces => ToneMapping.Aces,
            _ => ToneMapping.Linear,
        };
        set => Native.TonemapMode = value switch
        {
            ToneMapping.Filmic => Godot.Environment.ToneMapper.Filmic,
            ToneMapping.Aces => Godot.Environment.ToneMapper.Aces,
            _ => Godot.Environment.ToneMapper.Linear,
        };
    }

    public float Exposure
    {
        get => Native.TonemapExposure;
        set => Native.TonemapExposure = Math.Max(0.01f, value);
    }

    internal Godot.Environment Native { get; } = new Godot.Environment();

    public static Atmosphere Studio() => new Atmosphere();

    public static Atmosphere Daylight()
    {
        Atmosphere air = new Atmosphere();
        air.Background = Background.Gradient(Color.FromHex("#3E7CC9"), Color.FromHex("#A9C9E8"), Color.FromHex("#5A5A55"));
        air.AmbientFromSky = true;
        air.AmbientStrength = 0.8f;
        return air;
    }

    public static Atmosphere Dusk()
    {
        Atmosphere air = new Atmosphere();
        air.Background = Background.Gradient(Color.FromHex("#2B2F5A"), Color.FromHex("#E89A6A"), Color.FromHex("#3A3030"));
        air.AmbientFromSky = true;
        air.AmbientStrength = 0.4f;
        air.Glow = 0.4f;
        air.Fog = new Fog(Color.FromHex("#C9A088"), 0.004f);
        return air;
    }

    public static Atmosphere Night()
    {
        Atmosphere air = new Atmosphere();
        air.Background = Background.Solid(Color.FromHex("#0B1020"));
        air.AmbientColor = Color.FromHex("#30405A");
        air.AmbientStrength = 0.35f;
        air.Glow = 0.8f;
        return air;
    }

    internal void ShowThrough(Camera camera) => camera.Lens.Environment = Native;

    private static Sky SkyFor(GradientBackground gradient)
    {
        ProceduralSkyMaterial material = new ProceduralSkyMaterial();
        material.SkyTopColor = gradient.Top.ToGodot();
        material.SkyHorizonColor = gradient.Horizon.ToGodot();
        material.GroundHorizonColor = gradient.Horizon.ToGodot();
        material.GroundBottomColor = gradient.Ground.ToGodot();
        Sky sky = new Sky();
        sky.SkyMaterial = material;
        return sky;
    }

    private void RefreshAmbient() =>
        Native.AmbientLightSource = _ambientFromSky && _background is GradientBackground
            ? Godot.Environment.AmbientSource.Sky
            : Godot.Environment.AmbientSource.Color;
}
