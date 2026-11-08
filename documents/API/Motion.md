---
tags: [api, motion]
aliases: [Tween, Tweens, MotionPlayer, Easing, Animation]
---

# Motion

Library `Evil.Engine.Motion` (`src/08_Motion`), easing in `Evil.Engine.Core`. `Game.Motion` moves, hops, turns,
spins, scales and fades things over time, and tells you when they are done.

```csharp
case "Move":
    _busy = Game.Motion.MoveAlong(pawn, [start + new Vector3(0f, 0f, -2f), start + new Vector3(2f, 0f, -2f)], 0.6f);
    break;
case "Attack":
    Game.Motion.Face(pawn, enemy.Position, 0.25f);
    Game.Motion.Hop(pawn, reach, 1.2f, 0.45f);
    _busy = Game.Motion.Hop(pawn, start, 0.5f, 0.4f);
    Game.Motion.Wait(enemy, 0.7f);
    Game.Motion.Hop(enemy, push, 0.35f, 0.2f);
    Game.Motion.MoveTo(enemy, enemy.Position, 0.35f);
    break;
```

## One queue per object

Motions on the **same object run one after another**: face, then hop, then hop back. Motions on **different
objects run at the same time**: the enemy waits, then reacts, while the attacker hops. Each motion reads its
starting point when its turn comes, so a move after a move starts where the first ended. Time left over when a
motion ends goes into the next one, so the result is the same at any frame rate.

| Call | Does |
|---|---|
| `MoveTo(obj, position, seconds, easing)` | slide to a position (relative to the parent, like `Position`) |
| `Hop(obj, position, height, seconds)` | jump there in an arc |
| `MoveAlong(obj, path, secondsPerStep, face: true)` | walk a list of points (tiles), turning to face each step |
| `Face(obj, point, seconds)` | turn around the vertical axis to face a point |
| `TurnTo(obj, yawDegrees, seconds)` | turn to a heading (0 looks along -Z, 90 along -X) |
| `Spin(obj, degrees, seconds)` | turn by an amount, more than 180 included |
| `ScaleTo(obj, scale, seconds)` | grow or shrink |
| `ColorTo(material, color, seconds)` | fade a material's colour |
| `Wait(obj, seconds)` | a pause in that object's queue |
| `Value(from, to, seconds)` | a number to read each frame (`.Current`), for fades and bars in the overlay |

## Waiting for a move

Every call returns a `Tween`: `IsDone`, `Progress`, `Elapsed`, `WasCancelled`, `Cancel()`, and `Finished`, a task
that ends with `true` (done) or `false` (cancelled). A turn-based game can poll, or `await`:

```csharp
if (_busy is { IsDone: true }) { NextTurn(); }

await Game.Motion.MoveAlong(pawn, path, 0.6f).Finished;
await Game.Motion.Hop(pawn, reach, 1.2f, 0.45f).Finished;
```

| Player call | Does |
|---|---|
| `IsBusy(obj)`, `IsBusy()` | anything still moving (lock input while a turn plays) |
| `Stop(obj)` | cancel what is queued; the object stays where it is |
| `Complete(obj)` | jump to the end of everything queued (a "skip" button) |
| `Speed` | 1 normal, 0.5 slow motion, 0 paused |

Destroyed objects drop their motions quietly. Queuing a motion on a destroyed object throws.

## Easing

`Easing.Linear`, `InQuad`, `OutQuad`, `InOutQuad`, `InCubic`, `OutCubic`, `InOutCubic` (the default), `InOutSine`,
`OutBack` (overshoots a little), `OutBounce`. `Ease.Apply(easing, t)` is plain maths in Core, so game rules can use
it too.

![[r9-motion.png]]
