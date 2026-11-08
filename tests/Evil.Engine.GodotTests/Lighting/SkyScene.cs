namespace Evil.Engine.GodotTests.Lighting;

internal sealed class SkyScene(bool useNight) : Scene
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
