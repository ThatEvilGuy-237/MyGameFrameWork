---
tags: [history, migration]
aliases: [Old engine, MyGameFrameWork, Migration]
---

# From MyGameFrameWork

The code before the engine is in git: commit `8b1fb3f` ("Update README.md", July 2025). Everything it did is
still here, under new names and in a clearer place.

## Old call to new call

| MyGameFrameWork | Evil Engine |
|---|---|
| `new Window(800, 800, "test").Run()` | `GameBuilder.Create("test", 800, 800).Start(scene).Build().Run()` |
| `Window.TestDrawing()` | a scene's `Draw(Canvas canvas)` |
| `Window.OnUpdateFrame` game logic | a scene's `Update(GameTime time)` |
| `EvilUtils.NewPush()` | `canvas.Push()` or `using var g = canvas.Group();` |
| `EvilUtils.PopOrgin()` | `canvas.Pop()` or the end of the `using` |
| `EvilUtils.PushTranslate(x, y)` | `canvas.Translate(x, y)` |
| `EvilUtils.PushRotate(0, 0, r)` | `canvas.Rotate(r)` |
| `EvilUtils.PushScale(x, y, z)` | `canvas.Scale(x, y)` |
| `EvilUtils.SetColor(r, g, b, a)` then draw | pass the colour: `canvas.FillRect(rect, new Color(r, g, b, a))` |
| `EvilUtils.DrawRectanlge(x, y, w, h)` | `canvas.FillRect(new RectF(x, y, w, h), color)` |
| `EvilUtils.Draw(Shape)` | `canvas.FillPolygon(polygon, color)` |
| `EvilUtils.SetTexture(path, rect)` then draw | `canvas.Draw(Game.Textures.Load(path).Region(rect), destination)` |
| `Shape.CenterAroundOrigin()` | `polygon.CenteredOnOrigin()` |
| `Shape.GetBoundingRectangle()` | `polygon.Bounds` |
| `Rect`, `RectF`, `RectD` | `RectI`, `RectF` (doubles were not used) |
| `SRTVector`, `SRTVectorsStack` | `Transform2D`, `TransformStack` |
| `ShadersHandeler`, `EvilShader` | internal `ShaderProgram` (games never see it) |
| `TextureHandeler` | `TextureLibrary`, `Texture2D`, `TextureRegion` |

## What got fixed on the way

| Problem in the old code | Now |
|---|---|
| groups deeper than two levels lost their parents' turn | real matrices, tested at three levels ([[Grouped Transforms]]) |
| a parent's scale did not move its children | scale is part of the matrix |
| `PushRotate(5f)` turned X, Y and Z and squashed shapes | `Rotate(degrees)` turns in 2D only |
| a new GPU buffer and VAO for every shape, every frame | one batch ([[Rendering]]) |
| shader files found by a Windows path from `bin/Debug` | shaders embedded |
| resizing kept the old size | viewport and canvas follow the window |
| a point list drawn as loose triangles | ear-clipping triangulation |
| the plain-colour fallback in the shader could not work | a white texture times the colour |
| the cutout reread the image file and made a texture each time | regions on one texture |
| the console printed every frame | quiet; `Game.LastFrame` has the numbers |
| alpha passed as `100.0f` | colours are 0 to 1, `Color.FromBytes` for 0 to 255 |
| ImageSharp 3.1.6 with open advisories | StbImageSharp 2.30.16 |
| `net8.0` | `net10.0`, like every other app |
