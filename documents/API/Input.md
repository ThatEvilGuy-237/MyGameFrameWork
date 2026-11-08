---
tags: [api, input]
aliases: [Keyboard, Mouse, Picking, Pick]
---

# Input

Library `Evil.Engine.Input` (`src/06_Input`). Read it in a scene's `Update` through `Game.Keyboard` and
`Game.Mouse`. Find what is under the mouse with `Pick`.

```csharp
protected override void Update(GameTime time)
{
    if (Game.Keyboard.WasPressed(Key.Space)) { TogglePause(); }
    if (Game.Mouse.Scroll != 0f) { _view.Zoom(MathF.Pow(0.85f, Game.Mouse.Scroll)); }

    Hit? hit = Pick(Game.Mouse.Position);
    if (Game.Mouse.WasPressed(MouseButton.Left) && hit?.Target.Ancestor<Unit>() is { } unit)
    {
        Select(unit);
    }
}
```

## Keyboard and mouse

| Call | True when |
|---|---|
| `IsDown(key)` / `IsDown(button)` | held now |
| `WasPressed(...)` | went down since the last update (key repeat does not count) |
| `WasReleased(...)` | came up since the last update |
| `Keyboard.IsAnyDown` | any key is held |
| `Mouse.Position`, `Delta`, `Scroll` | pointer in screen pixels, movement and wheel steps since the last update |

> [!important] Taps are never lost
> Godot's input events are recorded as they arrive and cleared after each update. A press and release inside
> one frame still shows as pressed and released. When the window loses focus, everything held is let go.

`Key` covers letters, digits, F1-F12, arrows, editing keys, Shift/Control/Alt, punctuation and the keypad. Keys
are read by their **place on the keyboard** (Godot's physical key), so WASD stays WASD on any layout. Buttons:
`Left`, `Right`, `Middle`, `Back`, `Forward`.

## Picking

`Pick(screenPoint)` (on a scene) or `Picker.Pick(camera, root, screenPoint)` sends the camera ray through the
pixel and returns the nearest `Hit(Target, Point, Distance)`, or `null`.

| Rule | Why |
|---|---|
| shapes are tested in their own space | a turned box is hit exactly, not by a loose box around it |
| spheres use the real sphere | the corners of their bounding box do not count |
| `Pickable = false` and hidden objects are skipped | rings, markers and effects do not steal clicks |
| `Ancestor<T>()` and `Tag` on any world object | a click on a pawn's hat finds the pawn and its game data |

## Proven with real input

`scripts/check.sh` runs `scripts/drive-orbit.sh` in a virtual screen: it presses Space, sweeps the real mouse
until the demo reports a pawn under it, clicks it, scrolls three times and presses F12. The check fails unless
the demo logged `spinning: False`, `selected: pawn-...` and `zoom:`.

![[orbit-driven.png]]
