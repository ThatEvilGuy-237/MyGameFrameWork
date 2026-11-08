---
tags: [api, graphics]
aliases: [Drawing, Color]
---

# Canvas

Namespace `Evil.Engine.Graphics`. A scene's `Draw(Canvas canvas)` gets the canvas. Coordinates are pixels,
`(0, 0)` is the top-left, `y` goes down, and positive angles turn clockwise on screen.

## Groups

| Call | Does |
|---|---|
| `Group()` | `using var g = canvas.Group();` starts a group that ends with the block |
| `Push()` / `Pop()` | the same by hand |
| `Translate(x, y)`, `Rotate(degrees)`, `Scale(f)` / `Scale(x, y)` | change the current group |
| `Apply(Transform2D)` | an entity's position, turn, scale and origin in one call |
| `ToWorld(local)`, `ToLocal(screen)` | convert points through the current group |
| `Size`, `Bounds`, `Depth`, `Transform` | screen size, screen rectangle, open groups, current matrix |

Details in [[Grouped Transforms]].

## Shapes

| Call | Draws |
|---|---|
| `FillRect(rect, color)` | a filled rectangle |
| `DrawRect(rect, color, thickness = 1)` | an outline, inside the rectangle |
| `DrawLine(from, to, color, thickness = 1)` | a thick line |
| `FillTriangle(a, b, c, color)` | one triangle |
| `FillCircle(center, radius, color, segments = 32)` | a circle |
| `FillPolygon(polygon, color)` | any simple polygon, concave too |
| `FillPolygon(polygon, region, tint)` | a polygon with a texture stretched over its bounds |

## Sprites

| Call | Draws |
|---|---|
| `Draw(texture, position)` | the whole texture at its size |
| `Draw(texture, rect)` | the whole texture stretched into `rect` |
| `Draw(region, position)` / `Draw(region, rect)` | one cell of a sheet ([[Textures]]) |
| `Draw(region, rect, tint)` | the same, coloured (white = unchanged) |

A negative width or height mirrors the sprite.

## Color

`Color(R, G, B, A = 1)` with channels from 0 to 1. `Color.FromHex("#FF9F1C")`, `FromHex("#RRGGBBAA")`,
`FromBytes(255, 159, 28)`, `WithAlpha(0.5f)`, `Multiply`, `Lerp`, `ToHex()`, and the named colours `White`,
`Black`, `Transparent`, `Red`, `Green`, `Blue`, `Yellow`, `Gray`.

> [!tip] Draw order is paint order
> What you draw later lands on top. Draw the map, then units, then the interface.
