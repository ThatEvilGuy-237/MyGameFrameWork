---
tags: [api, input]
aliases: [Keyboard, Mouse, Keys]
---

# Input

Namespace `Evil.Engine.Input`. Read it in `Update` through `Game.Keyboard` and `Game.Mouse`.

```csharp
if (Game.Keyboard.WasPressed(Key.Enter)) { Confirm(); }
if (Game.Mouse.WasPressed(MouseButton.Left) && tile.Contains(Game.Mouse.Position)) { Select(tile); }
```

## Keyboard

| Call | True when |
|---|---|
| `IsDown(key)` | the key is held now |
| `WasPressed(key)` | it went down since the last update (key repeat does not count) |
| `WasReleased(key)` | it came up since the last update |
| `IsAnyDown` | any key is held |

## Mouse

| Member | What |
|---|---|
| `Position` | pointer in window pixels |
| `Delta` | how far it moved since the last update |
| `Scroll` | wheel steps since the last update |
| `IsDown`, `WasPressed`, `WasReleased` | `MouseButton.Left`, `Right`, `Middle`, `Back`, `Forward` |

> [!important] Taps are never lost
> Presses and releases are recorded as they happen and cleared after each update. A tap shorter than one frame
> still shows as pressed (and released). The old way, comparing key state once per frame, missed those: the
> driven check in [[Run the Sample]] found it.

`Key` has letters, digits, F1-F12, arrows, the keypad, modifiers and the usual punctuation, with the same codes
as GLFW. `InputTests.EveryKeyMatchesOpenTK` keeps them in line.
