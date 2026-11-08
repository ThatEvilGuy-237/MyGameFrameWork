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

## 3D (since 0.2)

The same library, still without Godot, so game rules can work out positions and facing on their own. Directions
follow Godot (`+Y` up, `-Z` forward, Euler turns in Y-X-Z order). The Godot tests check every one of these
against Godot's own maths.

| Type | What |
|---|---|
| `Pose(Position, Rotation, Scale)` | where something is and which way it faces; `ToWorld(local pose)` chains poses like groups; `Forward`, `Right`, `Up`, `TurnedY`, `Moved`, `ToMatrix` |
| `Rotations` | `FromEulerDegrees`, `AroundAxis`, `LookAlong(direction)`, `Rotate(vector, rotation)` |
| `Bounds3(Min, Max)` | an axis-aligned box: `Contains`, `Intersects`, `Encapsulate`, `Union`, `FromCenter`, `FromPoints` |
| `Orbit.PoseFor(target, yaw, pitch, distance)` | where an orbiting camera sits and how it faces (see [[Cameras]]) |
| `Smoothing.Toward(current, goal, sharpness, seconds)` | glide towards a goal, the same at any frame rate |
| `Ray3(origin, direction)` | `Intersect(bounds)`, `IntersectPlane(point, normal)`, `IntersectSphere(center, radius)`; distances, `null` on a miss |

```csharp
Pose sun = Pose.Identity.TurnedY(90f);
Pose planet = sun.ToWorld(Pose.At(10f, 0f, 0f));
Pose moon = planet.ToWorld(Pose.At(5f, 0f, 0f));
```

Here `moon.Position` is `(0, 0, -15)`: the moon follows the sun's turn, just like nested world objects.
