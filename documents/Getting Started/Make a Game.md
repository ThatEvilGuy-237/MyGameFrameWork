---
tags: [getting-started, how-to]
aliases: [Quick start, First game]
---

# Make a Game

A game is a `Program.cs` that builds the game with values, and one or more [[Scenes]].

## Steps

1. Make a console project on .NET 10 and reference the engine, one of two ways:
   - **Next to the engine** (while both change): a project reference to
     `src/Evil.Engine/Evil.Engine.csproj`.
   - **From the feed** (a fixed version): add the folder feed and pin the exact version.

     ```xml
     <packageSources>
       <add key="evil-engine" value="/mnt/hdd/projects/MyGameFrameWork-data/feed" />
     </packageSources>
     <packageSourceMapping>
       <packageSource key="evil-engine"><package pattern="Evil.*" /></packageSource>
     </packageSourceMapping>
     ```

     ```xml
     <PackageReference Include="Evil.Engine" Version="0.1.0" />
     ```

2. Write the first scene:

   ```csharp
   internal sealed class HelloScene : Scene
   {
       private float _angle;

       protected override void Update(GameTime time)
       {
           if (Game.Keyboard.WasPressed(Key.Escape))
           {
               Game.Exit();
           }

           _angle += 90f * time.DeltaSeconds;
       }

       protected override void Draw(Canvas canvas)
       {
           using var group = canvas.Group();
           canvas.Translate(canvas.Size / 2f);
           canvas.Rotate(_angle);
           canvas.FillRect(new RectF(-50f, -50f, 100f, 100f), Color.Red);
       }
   }
   ```

3. Build and run it in `Program.cs`:

   ```csharp
   GameBuilder.Create(new WindowSettings("Hello", 800, 600))
       .Content(Path.Combine(AppContext.BaseDirectory, "Content"))
       .Start(new HelloScene())
       .Build()
       .Run();
   ```

4. Put images under `Content/` and copy them to the output:

   ```xml
   <None Include="Content\**\*" CopyToOutputDirectory="PreserveNewest" />
   ```

> [!important] Values, not config
> The engine never reads config files or environment variables. Your `Program.cs` reads what it needs and
> passes values to [[Game Builder]], so all the wiring is in one place.

**Done when** a window opens with a turning red square and Escape closes it.

The usings a game needs: `System.Numerics`, `Evil.Engine.Graphics`, `Evil.Engine.Hosting`,
`Evil.Engine.Input`, `Evil.Engine.Mathematics`, `Evil.Engine.Scenes`, `Evil.Engine.Textures`. The sample
keeps them in one `GlobalUsings.cs`.
