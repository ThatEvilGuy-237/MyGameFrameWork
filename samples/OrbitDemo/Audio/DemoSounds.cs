namespace OrbitDemo;

internal sealed record DemoSounds(Sound Click, Sound Hop, Sound Hit)
{
    public static DemoSounds Load(ContentLibrary content) =>
        new DemoSounds(content.Sound("sounds/click.wav"), content.Sound("sounds/hop.wav"), content.Sound("sounds/hit.wav"));
}
