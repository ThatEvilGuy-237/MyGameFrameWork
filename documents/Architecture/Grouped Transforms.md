---
tags: [architecture, math]
aliases: [Transform stack, Groups, SRT]
---

# Grouped Transforms

The heart of the engine and the idea MyGameFrameWork started from: MonoGame puts the origin back at `(0, 0)` on
every draw, so moving a group of things means moving each one. Here a **group** keeps its parent's move, turn
and scale, and everything drawn inside follows.

```csharp
using (canvas.Group())
{
    canvas.Translate(0f, 100f);
    canvas.Rotate(-angle);
    canvas.FillRect(square, Color.Blue);

    using (canvas.Group())
    {
        canvas.Translate(0f, 150f);
        canvas.Rotate(angle);
        canvas.FillPolygon(star, Color.Yellow);

        using (canvas.Group())
        {
            canvas.Rotate(angle * 3f);
            canvas.Translate(90f, 0f);
            canvas.FillCircle(Vector2.Zero, 14f, Color.White);
        }
    }
}
```

## How it works now

Every group is one 3x2 matrix (`System.Numerics.Matrix3x2`). A step is put in front of the current matrix:

| Call | New current |
|---|---|
| `Translate(t)` | `T(t) × current` |
| `Rotate(d)` | `R(d) × current` |
| `Scale(s)` | `S(s) × current` |
| `Push()` / `Group()` | saves current |
| `Pop()` / end of `using` | restores the saved one |

A point drawn inside goes through the steps from the innermost outwards, so it gets **every** parent's turn
and scale, at any depth. Order matters within a group, the same as in OpenGL: translate then rotate turns in
place; rotate then translate swings around (the moon above).

## What was wrong before

MyGameFrameWork added up the parents' moves and turns as plain numbers, and only turned the innermost
offset. That held for two levels (the original demo), and broke beyond that:

| Case (tested in `TransformStackTests`) | Correct | Old engine |
|---|---|---|
| parent turns 90°, child moves 100, grandchild moves 50 | `(0, 150)` | `(100, 50)` |
| parent scales 2×, child moves 10 | `(20, 0)` | `(10, 0)` |
| the original two-level demo at 30° | `(475, 629.9)` | `(475, 629.9)`, same |

> [!tip] Picking things under the mouse
> `canvas.ToLocal(mouse)` runs the current group backwards. The sample tests the mouse against the star's own
> outline (`Polygon.Contains`) while the star turns and orbits.

The same stack exists without a window as `TransformStack` in [[Mathematics]].
