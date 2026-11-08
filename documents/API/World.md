---
tags: [api, world]
aliases: [WorldObject, Shapes, Materials]
---

# World

Library `Evil.Engine.World` (`src/03_World`). Everything placed in 3D is a `WorldObject`. A scene's `World` is
one, and so are shapes, cameras and lights. Objects inside another object follow its move, turn and scale:
your [[Grouped Transforms]] idea, now as a tree.

```csharp
var planet = World.Add(new Sphere(0.55f, Color.FromHex("#FFC53D")));
var ring = planet.Add(new Torus(0.8f, 0.95f, Color.FromHex("#E8D9A8")));
ring.Rotation = new Vector3(20f, 0f, 10f);
var moon = planet.Add(new Sphere(0.22f, Color.White) { Material = Material.Glowing(Color.White) });
moon.Position = new Vector3(1.4f, 0f, 0f);
```

## WorldObject

| Member | What |
|---|---|
| `new WorldObject("name")` | an empty group: a place to hang other objects on |
| `Add(child)` / `Remove(child)` | put an object inside this one or take it out; one parent each, no loops |
| `Destroy()` | remove it and everything inside it; using it afterwards throws |
| `Find(name)`, `Descendants<T>()` | look inside, nearest first |
| `Position`, `Rotation` (degrees), `Orientation` (quaternion), `Scale`, `Visible` | relative to the parent |
| `Pose` | position, orientation and scale in one value (see [[Mathematics]]) |
| `WorldPosition`, `WorldPose`, `Forward`, `Right`, `Up` | where it really is and which way it faces |
| `ToWorld(local)`, `ToLocal(world)` | convert points through this object |
| `Translate(offset)`, `Rotate(axis, degrees)`, `RotateY(degrees)`, `LookAt(target)` | move and turn |

Directions follow Godot: `+Y` is up, `-Z` is forward, a positive turn around `Y` turns left.

## Shapes

| Shape | Made with |
|---|---|
| `Box` | `new Box(size, color)` |
| `Sphere` | `new Sphere(radius, color)` |
| `Floor` | `new Floor(new Vector2(width, depth), color)`, flat and facing up |
| `Cylinder` | `new Cylinder(radius, height, color)` |
| `Cone` | `new Cone(radius, height, color)` |
| `Capsule` | `new Capsule(radius, height, color)`; the height includes both round ends |
| `Torus` | `new Torus(innerRadius, outerRadius, color)` |

Every shape has `Color`, `Material` and `CastsShadow`. Sizes are checked before anything is created, so a bad
size never leaves a half-made object behind.

## Materials

A `Material` can be shared: give ten pillars the same `stone` and recolour them all at once.

| Member | What |
|---|---|
| `Color` | base colour; an alpha below 1 makes it see-through |
| `Roughness`, `Metallic` | 0 to 1 |
| `Glow`, `GlowStrength` | light the object gives off itself |
| `Unshaded` | ignore lights (flat colour) |
| `Material.Matte`, `Shiny`, `Metal`, `Glowing` | ready-made starting points |

> [!note] Coming later
> Textures arrive with file loading in round 8. Shiny and metal surfaces look their best once the sky and
> ambient light of round 5 are in.
