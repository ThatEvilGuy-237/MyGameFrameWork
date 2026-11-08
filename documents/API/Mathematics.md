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

## Validators

Every entry point refuses NaN and infinity where it enters, with a message that names the value and says where
NaN usually comes from, instead of letting it spread until an object silently vanishes:

```
A ray's direction must be finite, got <NaN, 1, 0>. NaN and infinity usually come from a division by zero or from
normalizing a zero-length vector earlier on.
```

| Refused | Reason given |
|---|---|
| NaN or infinity in rays, rotations, Euler angles, `Angle.Wrap`, smoothing, orbits, polygon corners, bounds, `TransformStack` steps, `Ease.Apply` | must be finite |
| a zero-length ray direction, turn axis, look direction or plane normal | "has no length, so it points nowhere" |
| a negative sphere radius or orbit distance, negative smoothing time | cannot be negative; time cannot run backwards |
| `Pose.ToLocal` with a zero scale | there is no way back through a flat scale |
| `Pose.ToWorld(pose)` with an unevenly scaled parent and a turned child | the result would be skewed, which a `Pose` cannot hold |

World objects check their `Position`, `Rotation`, `Scale` and `Orientation` the same way, with the object's name
(`pawn-red's Position must be finite`). Scale zero stays allowed: things grow from nothing.

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
