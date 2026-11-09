---
tags: [release, changes]
aliases: [Release notes, What changed, 0.3.0]
---

# Changes

What a game author needs to know between versions. The why behind each change is in [[Decisions]], the rounds that
made them in [[LOOP]].

## 0.3.0 (November 2026)

**New**

| What | Where |
|---|---|
| `dotnet new evil-game`: a new game in one command, with the code style in its build | [[Make a Game]] |
| `Grid` and `Cell`: a board on the ground, cell under a point, neighbours, shortest `Path` around blocked cells, `Reachable` for move range, `Bounds` | [[Mathematics]] |
| `Camera.Frame(bounds, pitch, yaw)`: the whole box in view in one call | [[Cameras]] |
| `SelfOrAncestor<T>()`: the clicked object or the nearest group around it of that type | [[World]], [[Input]] |
| `RoundShape`: the base of `Cylinder` and `Cone` (`Height`) | [[World]] |
| one start line from the engine, before the first scene: `Evil Engine 0.3.0: "title" WxH, first scene X, content ...` | [[Scenes]] |
| a [[Scenes]] note: a scene's life, `Game`, helper classes, logging | [[Scenes]] |

**Changed behaviour**

| What | Before | Now |
|---|---|---|
| NaN, infinity and impossible values (zero-length directions, negative radius or distance, time going back) in maths, motion, picking and world transforms | passed on silently, things vanished | refused where they enter, with the value, the object's name and what to do |
| `Pose.ToWorld(pose)` with an unevenly scaled parent and a turned child | a wrong (skewed) result | refused: use `WorldObject.ToWorld` for points |
| `Pose.ToLocal` with a zero scale | infinity | refused |
| picking while any object is scaled to zero | every click threw | that object is skipped |
| a ray grazing a small sphere far away | could miss | hit (the gap is measured at the closest point) |
| a model loaded from a `.glb` file outside the Godot project | parsed on every `Model(...)` call | parsed once, a fresh copy per call |
| error messages | said what went wrong | also say what to do next |

**Unchanged for games**: every namespace and every public type from 0.2.0. The libraries now keep their files in
folders by meaning (see [[Architecture Overview]]), which a game does not see.

**For the engine's own code**: the clean-code rules and their check ([[Clean Code]]); 92 plain and 113 Godot tests,
about a third of them property tests that try hundreds of random inputs each ([[Testing]]).

## 0.2.0 (November 2026)

The engine moved to Godot 4.7.2 for a full 3D game: eleven numbered libraries, world objects in groups, shapes,
materials, textures and models, cameras and the orbit camera, lights and atmosphere, input and picking, the screen
and its 2D overlay, motion and audio. See [[From MyGameFrameWork]].
