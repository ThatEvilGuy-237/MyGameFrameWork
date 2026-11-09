---
tags: [home, map]
aliases: [Start here, Index]
---

# Evil Engine

Our own game engine SDK for C#, version **0.2.0**: eleven class libraries over **Godot 4.7.2 (.NET)** for a full 3D
game. Godot renders, plays sound and runs the window; a game talks only to our API. It sees Evil Engine types and
`System.Numerics` vectors, never Godot's. The idea it started from is still at the heart: things in a **group**
move, turn and scale together ([[Grouped Transforms]]).

> [!tip] New here?
> [[Make a Game]] gets a window with a scene up in four steps; [[Run the Sample]] shows everything the engine does.

![[r10-audio.png]]

## Map

| Area | Notes |
|---|---|
| Getting started | [[Make a Game]] · [[Run the Sample]] |
| Architecture | [[Architecture Overview]] · [[Grouped Transforms]] · [[Testing]] |
| The 3D world | [[World]] · [[Content]] · [[Cameras]] · [[Lighting]] |
| A game | [[Scenes]] (the scene's life and `Game`) |
| Play | [[Input]] · [[Screen]] · [[Motion]] · [[Audio]] |
| Maths without Godot | [[Mathematics]] |
| Why it is built this way | [[Decisions]] |
| History | [[From MyGameFrameWork]] (2025) and tag `v0.1.0` (the OpenTK 2D engine) |
| How it was built | [[LOOP]] (one round at a time, each with a screenshot) |

## The engine in one scene

```csharp
internal sealed class SkirmishScene : Scene
{
    private Capsule? _pawn;
    private Tween? _busy;

    protected override void Enter()
    {
        Atmosphere = Atmosphere.Dusk();
        World.Add(Sun.Golden());
        Floor floor = World.Add(new Floor(new Vector2(24f, 24f), Color.White));
        floor.Material.Texture = Game.Content.Texture("textures/tiles.png");
        floor.Material.TextureScale = new Vector2(12f, 12f);
        World.Add(Game.Content.Model("models/tower.glb"));

        _pawn = World.Add(new Capsule(0.35f, 1.4f, Color.FromHex("#E5484D")));
        _pawn.Position = new Vector3(0f, 0.7f, 2f);
        Camera.Position = new Vector3(0f, 7f, 11f);
        Camera.LookAt(Vector3.Zero);
        Game.Audio.PlayMusic(Game.Content.Sound("sounds/music.wav"), 2f);
    }

    protected override void Update(GameTime time)
    {
        if (_busy is null && Game.Mouse.WasPressed(MouseButton.Left) && Pick(Game.Mouse.Position)?.Target == _pawn)
        {
            _busy = Game.Motion.MoveTo(_pawn!, _pawn!.Position + new Vector3(0f, 0f, -2f), 0.6f);
        }

        if (_busy is { IsDone: true })
        {
            _busy = null;
        }
    }

    protected override void Draw(Canvas canvas) =>
        canvas.DrawText(_busy is null ? "Click the pawn" : "Moving...", new Vector2(20f, 20f), 22f, Color.White);
}
```

A game wires it up in one class, its only Godot-facing line: see [[Make a Game]].
