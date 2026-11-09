# Evil Engine

Our own game engine SDK for C#, for a full 3D game: eleven clean class libraries on top of
[Godot 4.7.2](https://godotengine.org/) (.NET). Godot renders, plays sound and runs the window; a game talks only to
our API: world objects, models, cameras, lights, input, the screen overlay, motion, audio and scenes.

It started as **MyGameFrameWork**, a small OpenGL project to learn how rendering works before the internship at
AZTEQ, inspired by [MonoGame](https://monogame.net/). What I did not like in MonoGame was that every draw puts the
origin back at `(0, 0)`. The idea here is the opposite: things in a **group** stay together and follow their
parent's move, turn and scale. That idea is still the heart of the engine, now in 3D. The 2D OpenTK version is tag
`v0.1.0`; the original code is commit `8b1fb3f`.

![The orbit demo](documents/screenshots/r10-audio.png)

## A game

```csharp
public partial class Main : GameMain
{
    protected override GameHost CreateGame() =>
        GameBuilder.Create(new WindowSettings("Orbit", 1280, 720))
            .Content("res://content")
            .Start(new OrbitScene())
            .Build();
}

internal sealed class OrbitScene : Scene
{
    private Box? _blue;

    protected override void Enter()
    {
        Atmosphere = Atmosphere.Dusk();
        World.Add(Sun.Golden());
        _blue = World.Add(new Box(Vector3.One, Color.FromHex("#3E63DD")));
        Sphere yellow = _blue.Add(new Sphere(0.55f, Color.FromHex("#FFC53D")));
        yellow.Position = new Vector3(3f, 0f, 0f);
        Camera.Position = new Vector3(0f, 7f, 11f);
        Camera.LookAt(Vector3.Zero);
    }

    protected override void Update(GameTime time) => _blue!.RotateY(-50f * time.DeltaSeconds);

    protected override void Draw(Canvas canvas) => canvas.DrawText("Orbit", new Vector2(20f, 20f), 24f, Color.White);
}
```

`_blue.Add(...)` puts the yellow sphere in the blue box's group: turn the box and the sphere orbits it. A new game
from scratch: [documents/Getting Started/Make a Game.md](documents/Getting%20Started/Make%20a%20Game.md).

## Layout

```
src/00_Core          Evil.Engine.Core           GameTime, Color, easing (no dependencies)
src/01_Mathematics   Evil.Engine.Mathematics    2D and 3D maths: poses, rays, bounds, orbits, polygons (no dependencies)
src/02_Platform      Evil.Engine.Platform       the Godot glue, internal to the SDK
src/03_World         Evil.Engine.World          WorldObject groups, shapes, materials, textures, models
src/04_Cameras       Evil.Engine.Cameras        Camera, OrbitCamera, screen rays
src/05_Lighting      Evil.Engine.Lighting       Sun, Lamp, Spotlight, Atmosphere
src/06_Input         Evil.Engine.Input          keyboard, mouse, picking the object under the mouse
src/07_Screen        Evil.Engine.Screen         display, frame capture, 2D overlay canvas with text
src/08_Motion        Evil.Engine.Motion         moves, hops, turns, fades, queued per object
src/09_Audio         Evil.Engine.Audio          sounds, music, volume groups, recording
src/99_Engine        Evil.Engine                GameBuilder, GameMain, content, scenes (always last)
samples/OrbitDemo                               a Godot project using only our API
tests/Evil.Engine.Tests                         pure tests (Core, Mathematics)
tests/Evil.Engine.GodotTests                    tests that run inside Godot against real nodes
templates/Evil.Engine.Templates               dotnet new evil-game: a new game in one command
documents/                                      docs (Obsidian vault): start at documents/Home.md
```

A library only uses lower numbers. A game references the `Evil.Engine` package and gets the rest with it.

## Run

```bash
./scripts/check.sh                          # style, build, tests, the demo rendered and driven with real input
bash scripts/godot.sh samples/OrbitDemo     # the demo on a machine without a screen
./scripts/pack.sh                           # pack all eleven packages into the folder feed
./scripts/make-a-game-check.sh              # dotnet new evil-game from the feed: built, style-checked and run
```

On a desktop, open `samples/OrbitDemo` in Godot 4.7.2 (.NET) and press play. Hover and click a pawn, then Move,
Attack or Wait. Space pauses, Q and E turn the camera, the wheel zooms, right-drag orbits, M mutes the music, F10
shrinks the window, F11 toggles full screen, F12 saves a screenshot.
