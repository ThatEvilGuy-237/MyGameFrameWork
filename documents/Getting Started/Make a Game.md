---
tags: [getting-started, how-to]
aliases: [Quick start, First game, New game, Template]
---

# Make a Game

A game is a small Godot 4.7.2 (.NET) project whose code talks only to Evil Engine. You need the .NET 10 SDK and
Godot 4.7.2 .NET. One command makes it.

## Steps

1. **Install the template**, once per machine:

   ```bash
   dotnet new install Evil.Engine.Templates --add-source /mnt/hdd/projects/MyGameFrameWork-data/feed
   ```

2. **Make the game.** `-n` names the project, its folder and its namespace; `--title` sets the window title:

   ```bash
   dotnet new evil-game -n Skirmish --title "Tile Skirmish"
   ```

   `--feed` points at another package folder or NuGet source if the engine's packages live elsewhere. With a folder
   feed, add `--no-update-check`: there is no online version to compare with. To call the first scene something else,
   rename `MapScene.cs` and its class, and the `new MapScene()` in `Main.cs`.

3. **Run it.** On a desktop, open the folder in Godot 4.7.2 (.NET) and press play. On the server, with the engine
   repo's script (a game folder has no `scripts/`):

   ```bash
   cd Skirmish
   dotnet build
   bash /mnt/hdd/projects/MyGameFrameWork/scripts/godot.sh --headless . --import
   bash /mnt/hdd/projects/MyGameFrameWork/scripts/godot.sh .
   ```

   The first line the game prints says it started, before the first scene enters, for example
   `Evil Engine 0.3.0: "Tile Skirmish" 1280x720, first scene MapScene, content res://content`. No line means the
   engine never started. The import is needed once, and again after new content. To take screenshots
   or click with `xdotool`, see "Driving a game on the server" in [[Testing]].

**Done when** a window shows a turning blue box on a green floor with the title in the corner, and Escape closes
it.

![[make-a-game.png]]

## What you get

| File | What |
|---|---|
| `Main.cs` | the game's one Godot line: window, content folder, first scene |
| `MapScene.cs` | the first scene: a floor, a turning box, the title, Escape quits |
| `GlobalUsings.cs` | `System`, collections, `System.Numerics` and every Evil Engine namespace, so no file needs its own usings |
| `<name>.csproj` | Godot's SDK, the `Evil.Engine` package, warnings as errors, the code style in the build |
| `CodeStyle.targets` | stops the build on `new()` (EVIL001) and on `{ Name = ... }` blocks (EVIL002), with file, line and what to write instead, all in one build |
| `.editorconfig` | stops the build on `var` |
| `project.godot`, `main.tscn`, `nuget.config`, `.gitignore` | the Godot project, its one scene, where the packages come from, what git skips |

```csharp
namespace MyGame;

public partial class Main : GameMain
{
    protected override GameHost CreateGame() =>
        GameBuilder.Create(new WindowSettings("My Game", 1280, 720))
            .Content("res://content")
            .Start(new MapScene())
            .Build();
}
```

```csharp
namespace MyGame;

internal sealed class MapScene : Scene
{
    private Box? _marker;

    protected override void Enter()
    {
        Atmosphere = Atmosphere.Daylight();
        World.Add(Sun.Noon());
        World.Add(new Floor(new Vector2(20f, 20f), Color.FromHex("#4A6B5A")));
        _marker = World.Add(new Box(Vector3.One, Color.FromHex("#3E63DD")));
        _marker.Position = new Vector3(0f, 0.5f, 0f);
        Camera.Position = new Vector3(0f, 8f, 10f);
        Camera.LookAt(Vector3.Zero);
    }

    protected override void Update(GameTime time)
    {
        if (Game.Keyboard.WasPressed(Key.Escape))
        {
            Game.Exit();
        }

        _marker!.RotateY(45f * time.DeltaSeconds);
    }

    protected override void Draw(Canvas canvas)
    {
        canvas.DrawText("My Game", new Vector2(20f, 20f), 24f, Color.White);
    }
}
```

```xml
<Project Sdk="Godot.NET.Sdk/4.7.2">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
  <Import Project="CodeStyle.targets" />
  <ItemGroup>
    <PackageReference Include="Evil.Engine" Version="0.2.0" />
  </ItemGroup>
</Project>
```

`.Content("res://content")` names the folder for models, textures, fonts and sounds. It does not have to exist
until the game loads something from it. Godot's SDK pins Godot's own packages, so central package versions stay
off ([[Decisions]] 24).

> [!important] The code style is part of the game
> A game made this way builds only in the project's code style: real types instead of `var` (the compiler's
> IDE0008), and objects built with their constructor and then plain statements: no `new()` and no initializer
> blocks (`CodeStyle.targets`). One build lists every problem with its file and line and what to write instead.
> The rest of the house style is in [[Clean Code]].

`scripts/make-a-game-check.sh` runs exactly these steps after every pack (see [[Testing]]), and `check.sh` fails if
this note stops showing the template's files as they are.
