---
tags: [home, map]
aliases: [Start here, Index]
---

# Evil Engine

Our own game engine SDK for C#, now in **3D on top of Godot 4.7.2**. Godot does the rendering, lights, shadows
and windows; our libraries are what a game talks to. A game sees Evil Engine types and `System.Numerics`
vectors, never Godot's API, and the one idea this started from stays at the heart: things in a **group** move,
turn and scale together (see [[Grouped Transforms]]).

> [!important] Being built in rounds
> The SDK grows one round every 30 minutes. What is done and what comes next is in [[LOOP]]. Notes that describe
> features still to come are written when their round lands.

![[orbit.png]]

## Map

| Area | Notes |
|---|---|
| Plan and progress | [[LOOP]] |
| Architecture | [[Architecture Overview]] · [[Grouped Transforms]] · [[Testing]] |
| API | [[World]] · [[Content]] · [[Cameras]] · [[Lighting]] · [[Input]] · [[Screen]] · [[Motion]] · [[Mathematics]] (more per round) |
| History | [[From MyGameFrameWork]] (the 0.1.0 OpenTK engine, tag `v0.1.0`) |
| Why it is built this way | [[Decisions]] |

## A game today

```csharp
public partial class Main : GameMain
{
    protected override GameHost CreateGame() =>
        GameBuilder.Create(new WindowSettings("Orbit", 1280, 720))
            .Start(new OrbitScene())
            .Build();
}

internal sealed class OrbitScene : Scene
{
    private Box? _blue;

    protected override void Enter()
    {
        Camera.Position = new Vector3(0f, 7f, 11f);
        Camera.LookAt(Vector3.Zero);
        Atmosphere = Atmosphere.Dusk();
        World.Add(Sun.Golden());
        World.Add(new Floor(new Vector2(24f, 24f), Color.FromHex("#3C5A5A")));

        _blue = World.Add(new Box(Vector3.One, Color.FromHex("#3E63DD")));
        var yellow = _blue.Add(new Sphere(0.55f, Color.FromHex("#FFC53D")));
        yellow.Position = new Vector3(3f, 0f, 0f);
    }

    protected override void Update(GameTime time) => _blue!.RotateY(-50f * time.DeltaSeconds);
}
```

`_blue.Add(...)` puts the yellow sphere inside the blue box's group: turn the box and the sphere orbits it.

## Run it

```bash
./scripts/check.sh                                  # build, tests, the demo in Godot, a screenshot
bash scripts/godot.sh samples/OrbitDemo             # the demo on the server (virtual screen)
```

On a desktop: open `samples/OrbitDemo` in Godot 4.7.2 (.NET) and press play.
