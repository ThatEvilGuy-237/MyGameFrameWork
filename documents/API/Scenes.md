---
tags: [api, scenes, game]
aliases: [Scene, GameContext, Game, Scene stack]
---

# Scenes

Library `Evil.Engine` (`src/99_Engine`, folders `Scenes/` and `Hosting/`). A game is a stack of scenes. Each scene
owns its own `World`, `Camera` and `Atmosphere`, and reaches the rest of the engine through `Game`.

## A scene's life

| Override | Called |
|---|---|
| `Enter()` | once, when the scene comes onto the stack: build the world here |
| `Update(GameTime time)` | every frame while it is the top scene; `time.DeltaSeconds` is the frame, `TotalSeconds` the game so far |
| `Draw(Canvas canvas)` | every frame after the updates: the 2D overlay (see [[Screen]]) |
| `Pause()`, `Resume()` | a full scene was pushed on top of it, and popped off again |
| `Exit()` | once, when it leaves the stack; its world is freed after this |

`IsOverlay => true` makes a scene a layer on top: the scene below keeps being shown and its camera stays on screen
(a menu over the battle). A full scene hides the one below and pauses it.

## What a scene can use

| Member | What |
|---|---|
| `Game` | the game: see the next table |
| `World` | the scene's own `WorldObject`: `World.Add(...)` puts things in it (see [[World]]) |
| `Camera` | the scene's camera, already in its world and on screen (see [[Cameras]]) |
| `Atmosphere` | the scene's sky, ambient light and fog (see [[Lighting]]) |
| `Pick(screenPoint)` | the nearest world object under a pixel (see [[Input]]) |

These are `protected`: they belong to the scene. They throw a clear error when used before `Enter` or after
`Exit`, because then the scene has no world.

## `Game`: the game context

| Member | What |
|---|---|
| `Keyboard`, `Mouse` | input, read in `Update` (see [[Input]]) |
| `Motion` | moves, hops and turns, queued per object (see [[Motion]]) |
| `Audio` | sounds, music and volume groups (see [[Audio]]) |
| `Content` | models, textures, fonts and sounds from the content folder (see [[Content]]) |
| `Display` | window title, size, full screen (see [[Screen]]) |
| `Scenes` | the stack: `Push(scene)`, `Pop()`, `Switch(scene)`, `Current`; changes happen after the frame's updates |
| `Time`, `ScreenSize` | this frame's time and the screen size in pixels |
| `Exit()` | quit after this frame; `CaptureFrame(path)` saves the next frame as a picture |

## Helper classes

Split a scene into classes with one job each, and hand them what they need when the scene builds them in
`Enter`: a game piece gets `World`, a rules class gets `Game.Motion`, a click handler gets `Camera` and `World`.

```csharp
protected override void Enter()
{
    _board = new BoardView(World, Grid.Centered(8, 8, 1f));
    _turns = new TurnOrder(Game.Motion);
    _clicks = new ClickReader(Camera, World);
    Camera.Frame(_board.Grid.Bounds, 55f);
}
```

A helper that picks without the scene's `Pick` uses `Picker.Pick(camera, world, screenPoint)`, which is what `Pick`
calls. The demo is split this way: see "Read the code" in [[Run the Sample]].

## Logging

`Console.WriteLine` is the log: on the server it lands in the file `godot.sh`'s output goes to, on a desktop in
Godot's output panel. The engine writes one line itself, before the first scene enters:
`Evil Engine 0.3.0: "My Game" 1280x720, first scene MapScene, content res://content`.
