namespace MyGame;

public partial class Main : GameMain
{
    protected override GameHost CreateGame() =>
        GameBuilder.Create(new WindowSettings("My Game", 1280, 720))
            .Content("res://content")
            .Start(new MapScene())
            .Build();
}
