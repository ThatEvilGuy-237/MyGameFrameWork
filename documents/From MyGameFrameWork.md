---
tags: [history, migration]
aliases: [Old engine, MyGameFrameWork, Migration, History]
---

# From MyGameFrameWork

| Version | What | Where |
|---|---|---|
| MyGameFrameWork (July 2025) | the owner's OpenGL learning project before the AZTEQ internship: `EvilUtils` with grouped transforms | commit `8b1fb3f` |
| 0.1.0 (November 2026) | that idea as a clean 2D SDK on OpenTK: builder, scenes, batched canvas | tag `v0.1.0` |
| 0.2.0 (November 2026) | a 3D game engine SDK on Godot 4.7.2: numbered libraries, world, cameras, lights, input, screen, content, motion, audio | `main` |

The idea that started it all made it through both rewrites: things in a group follow their parent's move, turn and
scale ([[Grouped Transforms]]). In 0.2.0 that is `WorldObject` in 3D and `Canvas.Group()` on the 2D overlay.

## Original call to 0.2.0

| MyGameFrameWork | Evil Engine 0.2.0 |
|---|---|
| `new Window(800, 800, "test").Run()` | `public partial class Main : GameMain` with `GameBuilder.Create("test", 800, 800).Start(scene).Build()` |
| `Window.TestDrawing()` | a scene's `Enter()` builds the 3D world once; `Draw(Canvas canvas)` draws the 2D overlay each frame |
| `Window.OnUpdateFrame` game logic | a scene's `Update(GameTime time)` |
| `EvilUtils.NewPush()` ... `PopOrgin()` | 3D: `WorldObject group = World.Add(new WorldObject("group"));` then `group.Add(child)`; overlay: `using (canvas.Group())` |
| `EvilUtils.PushTranslate(x, y)` | 3D: `group.Position = new Vector3(x, 0f, z);`; overlay: `canvas.Translate(x, y)` |
| `EvilUtils.PushRotate(0, 0, r)` | 3D: `group.RotateY(r)`; overlay: `canvas.Rotate(r)` |
| `EvilUtils.PushScale(x, y, z)` | 3D: `group.Scale = new Vector3(x, y, z);`; overlay: `canvas.Scale(x, y)` |
| `EvilUtils.SetColor(r, g, b, a)` then draw | `new Box(size, new Color(r, g, b, a))`, or `shape.Material.Color = ...`; overlay calls take the colour |
| `EvilUtils.DrawRectanlge(x, y, w, h)` | 3D: `new Box(...)`; overlay: `canvas.FillRect(new RectF(x, y, w, h), color)` |
| `EvilUtils.Draw(Shape)` | overlay: `canvas.FillPolygon(polygon, color)` |
| `EvilUtils.SetTexture(path, rect)` | 3D: `material.Texture = Game.Content.Texture(path);` |
| `Shape.CenterAroundOrigin()`, `GetBoundingRectangle()` | `polygon.CenteredOnOrigin()`, `polygon.Bounds` |
| `Rect`, `RectF`, `RectD` | `RectI`, `RectF`; in 3D `Bounds3` |
| `SRTVector`, `SRTVectorsStack` | `Transform2D`, `TransformStack` (2D), `Pose` (3D) |
| `ShadersHandeler`, `EvilShader`, `TextureHandeler` | Godot renders; games never see shaders or GPU textures |

## What got fixed along the way

| In MyGameFrameWork | Now |
|---|---|
| groups deeper than two levels lost their parents' turn | Godot node trees in 3D, matrices on the overlay; tested three levels deep |
| a parent's scale did not move its children | scale is part of the transform |
| a GPU buffer per shape per frame, shaders found by a Windows path | Godot does the rendering |
| no input, scenes, sound, text, models or lights | all of them, each with tests (38 pure + 95 inside Godot) |
| ImageSharp 3.1.6 with open advisories, `net8.0` | only Godot's own packages (checked), .NET 10 |
