---
tags: [api, hosting]
aliases: [GameBuilder, GameHost, GameContext, Hosting]
---

# Game Builder

Namespace `Evil.Engine.Hosting`. The builder takes **values**: no lambdas, no config sections.

```csharp
GameBuilder.Create(new WindowSettings("Our game", 1280, 720, VSync: true))
    .ClearColor(Color.FromHex("#1B2A2A"))
    .Content(contentFolder)
    .Textures(TextureFilter.Nearest)
    .Start(new MapScene())
    .Build()
    .Run();
```

| Call | Takes | Default |
|---|---|---|
| `Create(WindowSettings)` / `Create(title, width, height)` | the window | |
| `ClearColor(Color)` | background | `(0.2, 0.3, 0.3)`, the old engine's teal |
| `Content(folder)` | where `Textures.Load` looks | the game's output folder |
| `Textures(TextureFilter)` | default sampling | `Linear` (use `Nearest` for pixel art) |
| `Start(Scene)` | the first scene | required |
| `CaptureAndExit(path, frame)` | save that frame as PNG, then close | off |
| `Build()` | checks every value, returns a `GameHost` | |

`WindowSettings(Title, Width = 1280, Height = 720, VSync = true, Resizable = true, UpdatesPerSecond = 60)`.

> [!important] Fails early and says where
> `Build()` throws with the call that got the bad value, for example
> `GameBuilder.Start was not called: the game needs a first scene` or
> `GameBuilder.Content: the folder '...' does not exist`.

## GameHost

`Run()` opens the window, runs the loop until the game exits, and frees everything. It runs once per host.

## GameContext (`Game` inside a scene)

| Member | What |
|---|---|
| `Keyboard`, `Mouse` | [[Input]] |
| `Textures` | [[Textures]] |
| `Scenes` | [[Scenes]] |
| `ScreenSize` | window size in pixels, follows resizing |
| `Time` | `GameTime(DeltaSeconds, TotalSeconds, Frame)` |
| `LastFrame` | `FrameStats(DrawCalls, Triangles)` |
| `Exit()` | closes after this update |
| `CaptureFrame(path)` | saves this frame as PNG after it is drawn |

The game also closes when the last scene is popped.
