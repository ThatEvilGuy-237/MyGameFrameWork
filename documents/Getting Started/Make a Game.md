---
tags: [getting-started, how-to]
aliases: [Quick start, First game, New game]
---

# Make a Game

A game is a small Godot 4.7.2 (.NET) project whose code talks only to Evil Engine. You need the .NET 10 SDK and
Godot 4.7.2 .NET.

## Steps

1. **Make the Godot project.** In a new folder, add `project.godot`:

   ```ini
   config_version=5

   [application]
   config/name="My Game"
   run/main_scene="res://main.tscn"
   config/features=PackedStringArray("4.7", "C#", "Forward Plus")

   [dotnet]
   project/assembly_name="MyGame"
   ```

   and `main.tscn`, a single node that runs `Main.cs`:

   ```ini
   [gd_scene load_steps=2 format=3]

   [ext_resource type="Script" path="res://Main.cs" id="1"]

   [node name="Main" type="Node"]
   script = ExtResource("1")
   ```

2. **Add the project file** `MyGame.csproj`. Godot's SDK pins Godot's own packages, so central package versions stay
   off here ([[Decisions]] 24); nullable is on because the engine's API says what can be missing (`Hit?`).

   ```xml
   <Project Sdk="Godot.NET.Sdk/4.7.2">
     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
       <Nullable>enable</Nullable>
       <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
     </PropertyGroup>
     <ItemGroup>
       <PackageReference Include="Evil.Engine" Version="0.2.0" />
     </ItemGroup>
   </Project>
   ```

   with a `nuget.config` that finds our packages in the folder feed (and everything else on nuget.org):

   ```xml
   <configuration>
     <packageSources>
       <clear />
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
       <add key="evil-engine" value="/mnt/hdd/projects/MyGameFrameWork-data/feed" />
     </packageSources>
     <packageSourceMapping>
       <packageSource key="evil-engine"><package pattern="Evil.*" /></packageSource>
       <packageSource key="nuget.org"><package pattern="*" /></packageSource>
     </packageSourceMapping>
   </configuration>
   ```

   While the engine itself changes, a project reference to `src/99_Engine/Evil.Engine/Evil.Engine.csproj` works too.

3. **Write `Main.cs`**, the game's only Godot-facing line, and the first scene:

   ```csharp
   public partial class Main : GameMain
   {
       protected override GameHost CreateGame() =>
           GameBuilder.Create(new WindowSettings("My Game", 1280, 720))
               .Content("res://content")
               .Start(new MapScene())
               .Build();
   }

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

   Put the usings in one `GlobalUsings.cs`: `System.Numerics`, `Evil.Engine.Audio`, `Evil.Engine.Cameras`,
   `Evil.Engine.Core`, `Evil.Engine.Hosting`, `Evil.Engine.Input`, `Evil.Engine.Lighting`, `Evil.Engine.Mathematics`,
   `Evil.Engine.Motion`, `Evil.Engine.Scenes`, `Evil.Engine.Screen`, `Evil.Engine.World`.

4. **Run it.** On a desktop, open the folder in Godot 4.7.2 (.NET) and press play. Godot imports the content and builds
   the C#. On the server: `dotnet build`, then `bash scripts/godot.sh --headless <folder> --import` once, then
   `bash scripts/godot.sh <folder>`.

**Done when** a window shows a turning blue box on a green floor with "My Game" in the corner, and Escape closes it.

![[make-a-game.png]]

`scripts/make-a-game-check.sh` builds and runs exactly these steps after every pack (see [[Testing]]).

> [!important] The project's code style
> Real types instead of `var`, and objects built with their constructor and then plain property statements (see the
> repo's `CLAUDE.md`). The examples in these notes follow it, and `scripts/style-check.py` checks them.
