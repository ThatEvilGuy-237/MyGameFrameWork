---
tags: [release, changes]
aliases: [Release notes, What changed, 0.4.4, 0.4.3, 0.4.2, 0.4.1, 0.4.0, 0.3.0]
---

# Changes

What a game author needs to know between versions. The why behind each change is in [[Decisions]], the rounds that
made them in [[LOOP]].

## 0.4.4 (November 2026)

**New**

| What | Where |
|---|---|
| `WindowSettings(..., ScalesWithWindow: true)`: the game keeps the size it was laid out for, and the picture is scaled to the window (bars at the sides for a window of another shape) | [[Screen]] |

Off by default: a window that does not ask for it behaves as before, and `Game.ScreenSize` follows it.

## 0.4.3 (November 2026)

**Changed behaviour**

| What | Before | Now |
|---|---|---|
| a character whose clip is shorter or longer in its body file than its description says | loaded, and played what the file held | refused when it is loaded: the clip's name, its length in the file, the length described |
| a character or a clothing piece that is refused | left its loaded nodes behind | leaves nothing behind |
| `Wear` of a piece that is refused | had already taken off what was in its slot | the character stays as it was |

The turn-based game's first packages held one pose per clip: an export fault that nothing measured. A package that
loaded before and is refused now has that fault; export it again. Everything else is as in 0.4.2.

## 0.4.2 (November 2026)

**Removed**

| What | Instead |
|---|---|
| `CharacterModel.Wash(colour, amount)` | nothing: a character shows what its package holds. A different look is made in Blender |

The owner saw a character greyed by `Wash` and asked for the models as made in Blender, "not any extra effects".
Everything else is as in 0.4.1.

## 0.4.1 (November 2026)

**Changed behaviour**

| What | Before (0.4.0) | Now |
|---|---|---|
| how a character is drawn | two tones picked by a `KeyLight`, with an outline | the plain colours and pictures of its package, no light and shade, no outline |
| `KeyLight` | a light for characters only | gone: nothing lights a character |
| parts that only throw a shadow onto the character | threw it | not drawn |

The owner asked for the models to be loaded first and shading later. Everything else from 0.4.0 stays: packages,
clothing, clips, faces, `Wash`. The shading of 0.4.0 is at tag `v0.4.0`.

## 0.4.0 (November 2026)

**New**

| What | Where |
|---|---|
| `Game.Content.Character(folder)`: a character made in Blender, loaded with its rig, its look and its clips | [[Characters]], [[Content]] |
| `CharacterModel`: `Wear`, `WearOutfit`, `TakeOff` (clothing as separate files on the character's own rig, the covered skin hidden), `Wash`, `Height` | [[Characters]] |
| `CharacterClips`: `Play`, `Then`, `Current`, `Blend`; a clip also shows the faces it names | [[Characters]] |
| `KeyLight`: the light that decides which side of a character is lit, and falls on characters only | [[Characters]] |
| a twelfth library and package, `Evil.Engine.Characters` | [[Architecture Overview]] |

**Unchanged for games**: every namespace and every public type from 0.3.0. A game that shows no characters only
changes its version number.

**For the engine's own code**: 92 plain and 124 Godot tests; the character tests build their own small character
in code, so the engine carries no art.

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
