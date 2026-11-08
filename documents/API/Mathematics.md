---
tags: [api, math]
aliases: [Math, Polygon, RectF, Transform2D]
---

# Mathematics

Package and namespace `Evil.Engine.Mathematics`. No dependencies beyond .NET: points are
`System.Numerics.Vector2`, matrices are `Matrix3x2`.

## Rectangles

`RectF(X, Y, Width, Height)` for drawing, `RectI` for pixels in a texture (it turns into a `RectF` by itself).

| Member | What |
|---|---|
| `Left`, `Top`, `Right`, `Bottom`, `Center`, `Size`, corners | edges and points |
| `Contains(point)`, `Contains(rect)`, `Intersects(rect)` | tests (right and bottom edges are outside) |
| `Intersect`, `Union`, `Offset`, `Inflate`, `Scale` | new rectangles |
| `FromCenter(center, size)`, `FromCorners(a, b)` | builders |

## Angle

`Angle.ToRadians`, `Angle.ToDegrees`, `Angle.Wrap(degrees)` (to 0..360). The engine speaks degrees.

## Transform2D

An entity's place: `Transform2D.At(x, y).Rotated(45f).Scaled(2f).Around(origin)`. `ToMatrix()` gives the
matrix; `canvas.Apply(transform)` draws with it. Start from `Identity` or `At`: `default` has scale zero.

## TransformStack

The [[Grouped Transforms]] stack without a window: `Push`, `Pop`, `Group`, `Translate`, `Rotate`, `Scale`,
`Apply`, `ToWorld`, `ToLocal`, `Current`, `Depth`, `Reset`. Game logic can use it to work out where something
attached to something else is.

## Polygon

| Member | What |
|---|---|
| `new Polygon(a, b, c, ...)`, `Polygon.From(points)` | any outline, either direction |
| `Rectangle(rect)`, `Regular(center, radius, sides)`, `Star(center, outer, inner, points)` | builders |
| `Points`, `Count`, `Bounds`, `Area`, `SignedArea`, `Centroid`, `IsConvex` | measures |
| `Triangles` | indices, three per triangle (ear clipping; worked out once) |
| `Contains(point)` | inside the real outline (good for hex tiles and clicking shapes) |
| `Translate`, `Transform(matrix)`, `CenteredOnOrigin()` | new polygons |

`Triangulator.Triangulate(points)` does the cutting: concave shapes and points in a straight line work. A
self-crossing outline still gives whole triangles, though the result is a best guess.
