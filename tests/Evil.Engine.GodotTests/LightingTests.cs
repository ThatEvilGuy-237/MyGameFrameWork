namespace Evil.Engine.GodotTests;

internal static class LightingTests
{
    [Test]
    public static void LampsShineAroundThem()
    {
        var lamp = new Lamp(Color.Red, 6f) { Intensity = 2f };
        Check.Equal(Color.Red, lamp.Color, "colour");
        Check.Near(6f, lamp.Range, "range");
        Check.Near(2f, lamp.Intensity, "intensity");
        Check.True(!lamp.Shadows, "lamp shadows start off");
        lamp.Shadows = true;
        Check.True(lamp.Shadows, "shadows on");
        var dark = new Lamp { Intensity = -3f };
        Check.Near(0f, dark.Intensity, "no negative light");
        dark.Destroy();
        var warm = Lamp.Warm(4f);
        Check.True(warm.Color.R > warm.Color.B, "warm is warm");
        lamp.Destroy();
        warm.Destroy();
    }

    [Test]
    public static void SpotlightsPointAndLimitTheirAngle()
    {
        var spot = new Spotlight(Color.White, 12f, 25f);
        Check.Near(25f, spot.Angle, "angle");
        Check.Near(12f, spot.Range, "range");
        Check.Near(-Vector3.UnitY, spot.Direction, "points down by default");
        spot.Direction = new Vector3(1f, -1f, 0f);
        Check.Near(Vector3.Normalize(new Vector3(1f, -1f, 0f)), spot.Direction, "points where told");
        Check.Throws<ArgumentOutOfRangeException>(() => spot.Angle = 0f, "between 0 and 180");
        Check.Throws<ArgumentOutOfRangeException>(() => spot.Angle = 180f, "between 0 and 180");
        Check.Throws<ArgumentOutOfRangeException>(() => _ = new Spotlight(Color.White, 5f, 200f), "between 0 and 180");
        spot.Destroy();
    }

    [Test]
    public static void SunPresetsShineDown()
    {
        foreach (var sun in new[] { Sun.Noon(), Sun.Golden(), Sun.Moonlight() })
        {
            Check.True(sun.Direction.Y < 0f, "shines from above");
            Check.True(sun.Shadows, "casts shadows");
            sun.Destroy();
        }

        var moon = Sun.Moonlight();
        var noon = Sun.Noon();
        Check.True(moon.Intensity < noon.Intensity, "the moon is dimmer than noon");
        moon.Destroy();
        noon.Destroy();
    }

    [Test]
    public static void ADefaultAtmosphereLightsTheDarkSides()
    {
        var air = new Atmosphere();
        Check.Near(0.5f, air.AmbientStrength, "ambient light on");
        Check.Equal(ToneMapping.Filmic, air.ToneMapping, "filmic tone");
        Check.Equal(Background.ClearColor, air.Background, "background is the game's clear colour");
        Check.True(air.Native.BackgroundMode == Godot.Environment.BGMode.ClearColor, "Godot agrees");
        Check.True(air.Fog is null && air.Glow == 0f, "no fog, no glow");
    }

    [Test]
    public static void BackgroundsSwitchAndFeedTheAmbientLight()
    {
        var air = new Atmosphere { Background = Background.Solid(Color.Blue), AmbientFromSky = true };
        Check.True(air.Native.BackgroundMode == Godot.Environment.BGMode.Color, "solid colour");
        Check.True(air.Native.AmbientLightSource == Godot.Environment.AmbientSource.Color, "no sky to take light from");

        air.Background = Background.Gradient(Color.Blue, Color.White, Color.Gray);
        Check.True(air.Native.BackgroundMode == Godot.Environment.BGMode.Sky, "sky");
        var sky = (Godot.ProceduralSkyMaterial)air.Native.Sky.SkyMaterial;
        Check.Equal(Color.Blue, sky.SkyTopColor.ToEvil(), "top colour");
        Check.Equal(Color.White, sky.SkyHorizonColor.ToEvil(), "horizon colour");
        Check.True(air.Native.AmbientLightSource == Godot.Environment.AmbientSource.Sky, "ambient from the sky");
    }

    [Test]
    public static void FogGlowToneAndExposure()
    {
        var air = new Atmosphere { Fog = new Fog(Color.Gray, 0.02f), Glow = 0.7f, ToneMapping = ToneMapping.Aces, Exposure = 1.3f };
        Check.True(air.Native.FogEnabled, "fog on");
        Check.Near(0.02f, air.Native.FogDensity, "fog density");
        Check.Near(0.7f, air.Glow, "glow");
        Check.Equal(ToneMapping.Aces, air.ToneMapping, "tone");
        Check.Near(1.3f, air.Exposure, "exposure");
        air.Fog = null;
        air.Glow = 0f;
        Check.True(!air.Native.FogEnabled && !air.Native.GlowEnabled, "both off again");
        Check.Near(0f, air.Glow, "glow reads 0 when off");
    }

    [Test]
    public static void AtmospherePresetsAreReady()
    {
        Check.True(Atmosphere.Daylight().Background is GradientBackground, "daylight has a sky");
        Check.True(Atmosphere.Dusk().Fog is not null, "dusk is hazy");
        Check.True(Atmosphere.Night().Background is SolidBackground, "night is dark");
        Check.True(Atmosphere.Studio().AmbientStrength > 0f, "studio is lit");
    }

    [Test]
    public static void EachSceneShowsItsOwnAtmosphere()
    {
        using var stage = Stage.Open();
        var scenes = new SceneStack(null, stage.Root);
        var night = new SkyScene(useNight: true);
        var plain = new SkyScene(useNight: false);
        scenes.Push(night);
        scenes.Push(plain);
        scenes.ApplyChanges();
        Check.True(ReferenceEquals(night.OwnAtmosphere.Native, night.OwnCamera.Lens.Environment), "night scene's camera shows its night");
        Check.True(ReferenceEquals(plain.OwnAtmosphere.Native, plain.OwnCamera.Lens.Environment), "the other scene has its own");
        Check.True(!ReferenceEquals(night.OwnAtmosphere, plain.OwnAtmosphere), "not shared");
        var dusk = Atmosphere.Dusk();
        plain.Use(dusk);
        Check.True(ReferenceEquals(dusk.Native, plain.OwnCamera.Lens.Environment), "a new atmosphere shows at once");
    }

    private sealed class SkyScene(bool useNight) : Scene
    {
        public Atmosphere OwnAtmosphere => Atmosphere;

        public Camera OwnCamera => Camera;

        public void Use(Atmosphere atmosphere) => Atmosphere = atmosphere;

        protected override void Enter()
        {
            if (useNight)
            {
                Atmosphere = Atmosphere.Night();
            }
        }
    }
}
