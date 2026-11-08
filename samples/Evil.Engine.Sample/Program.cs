using Evil.Engine.Sample;

var options = SampleOptions.Parse(args);
var contentRoot = Path.Combine(AppContext.BaseDirectory, "Content");

var builder = GameBuilder.Create(new WindowSettings("Evil Engine sample", 800, 800))
    .ClearColor(Color.FromHex("#334D4D"))
    .Content(contentRoot)
    .Textures(TextureFilter.Nearest)
    .Start(new OrbitScene());

if (options.CapturePath is not null)
{
    builder.CaptureAndExit(options.CapturePath, options.CaptureFrame);
}

builder.Build().Run();
