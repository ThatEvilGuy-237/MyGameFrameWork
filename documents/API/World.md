---
tags: [api, world]
aliases: [WorldObject, Shapes, Materials]
---

# World

Library `Evil.Engine.World` (`src/03_World`). Everything placed in 3D is a `WorldObject`. A scene's `World` is
one, and so are shapes, cameras and lights. Objects inside another object follow its move, turn and scale:
your [[Grouped Transforms]] idea, now as a tree.

```csharp
Sphere planet = World.Add(new Sphere(0.55f, Color.FromHex("#FFC53D")));
Torus ring = planet.Add(new Torus(0.8f, 0.95f, Color.FromHex("#E8D9A8")));
ring.Rotation = new Vector3(20f, 0f, 10f);
Sphere moon = new Sphere(0.22f, Color.White);
moon.Material = Material.Glowing(Color.White);
moon.Position = new Vector3(1.4f, 0f, 0f);
planet.Add(moon);
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

### Your own objects

Derive from `WorldObject` (or a shape) to give a thing its game data, and build its parts in the constructor:

```csharp
internal sealed class Unit : WorldObject
{
    public Unit(string name, Color team)
        : base(name)
    {
        Capsule body = Add(new Capsule(0.35f, 1.4f, team));
        body.Position = new Vector3(0f, 0.7f, 0f);
    }

    public int Moves { get; set; } = 3;
}
```

A click on the body or on the unit itself finds the unit with `hit.Target.SelfOrAncestor<Unit>()` (see [[Input]]);
`Ancestor<Unit>()` starts at the parent, so it skips the object that was clicked.

## Shapes

| Shape | Made with |
|---|---|
| `Box` | `new Box(size, color)` |
| `Sphere` | `new Sphere(radius, color)` |
| `Floor` | `new Floor(new Vector2(width, depth), color)`, flat and facing up |
| `Cylinder` | `new Cylinder(radius, height, color)` |
| `Cone` | `new Cone(radius, height, color)` |
| `Capsule` | `new Capsule(radius, height, color)`; the height includes both round ends |
| `Torus` | `new Torus(innerRadius, outerRadius, color)`: a ring lying flat (around `Y`), the hole is `innerRadius` wide, the ring reaches `outerRadius` |

Shapes and model parts are both `MeshObject`s: they share `CastsShadow`, `Pickable` and `LocalBounds`, and both
can be picked (see [[Input]]). Every shape also has `Color` and `Material`. Sizes are checked before anything is created, so a bad
size never leaves a half-made object behind.

`Cylinder` and `Cone` are both a `RoundShape`: a round mesh with a `Height`; a cylinder's `Radius` sets both
ends, a cone's only the bottom.

## Materials

A `Material` can be shared: give ten pillars the same `stone` and recolour them all at once.

| Member | What |
|---|---|
| `Color` | base colour; an alpha below 1 makes it see-through |
| `Roughness`, `Metallic` | 0 to 1 |
| `Glow`, `GlowStrength` | light the object gives off itself |
| `Unshaded` | ignore lights (flat colour): the screen shows exactly the colour you gave, good for board tiles and markers |
| `Material.Matte`, `Shiny`, `Metal`, `Glowing` | ready-made starting points |

| `Texture`, `TextureScale` | an image on the surface (see [[Content]]); scale above 1 tiles it |

## Models

`Game.Content.Model(path)` gives a `Model`: a `WorldObject` whose meshes are `ModelPart`s and whose empty nodes are
groups, under the names they had in Blender.

| Member | What |
|---|---|
| `Parts`, `Part(name)` | every mesh in the model, or one by name |
| `part.Material` | paint one part; `null` goes back to the model's own material |
| `Paint(material)` | paint every part at once |
| `Find(name)`, `Descendants<T>()`, `SelfOrAncestor<T>()`, `Ancestor<T>()` | the same tree tools as any world object |
| `Source` | where it was loaded from |
