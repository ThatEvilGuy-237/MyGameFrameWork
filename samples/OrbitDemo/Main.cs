namespace OrbitDemo;

public partial class Main : GameMain
{
    protected override GameHost CreateGame()
    {
        SampleOptions options = SampleOptions.Parse(Arguments);
        GameBuilder builder = GameBuilder.Create(new WindowSettings("Evil Engine orbit demo", 1280, 720))
            .ClearColor(Color.FromHex("#1B2A2A"))
            .Content("res://content")
            .Start(new OrbitScene(options));

        if (options.CapturePath is not null)
        {
            builder.CaptureAndExit(options.CapturePath, options.Driven ? 20000 : options.CaptureFrame);
        }

        return builder.Build();
    }
}
