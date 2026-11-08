---
tags: [api, textures]
aliases: [Texture2D, TextureRegion, Sprites, Sprite sheets]
---

# Textures

Namespace `Evil.Engine.Textures`. Images become **textures**, and a part of a texture is a **region**: one
sprite in a sheet.

```csharp
protected override void Enter()
{
    var sheet = Game.Textures.Load("units.png");
    _frames = sheet.Grid(32, 32);
    _portrait = sheet.Region(0, 64, 64, 64);
}
```

## TextureLibrary (`Game.Textures`)

| Call | Does |
|---|---|
| `Load(path)` / `Load(path, filter)` | reads PNG, JPG, BMP, TGA or GIF under the content folder; the same path twice gives the same texture |
| `Create(name, width, height, rgba)` | a texture from your own pixels (4 bytes per pixel, rows top to bottom) |
| `Unload(texture)` | frees one texture now |
| `Root`, `DefaultFilter`, `Count` | where it looks, default sampling, how many are loaded |

Everything still loaded is freed when the game ends.

## Texture2D and TextureRegion

| Member | What |
|---|---|
| `Width`, `Height`, `Name`, `Filter` | size, the path or name it came from, sampling |
| `Full` | a region covering the whole texture |
| `Region(x, y, w, h)` / `Region(RectI)` | one part; throws if it is outside the texture |
| `Grid(cellWidth, cellHeight)` | every cell of a sheet, row by row |
| `region.Region(RectI)` | a part of a part, relative to it |

`TextureFilter.Nearest` keeps pixel art sharp; `Linear` smooths.

> [!note] Different from the old cutouts
> MyGameFrameWork loaded the image again from disk and made a new GPU texture for every cutout. A region now
> is just a rectangle on the one texture, so a whole sprite sheet is one texture and draws in one batch.

Images are decoded by **StbImageSharp** (Unlicense or MIT). It replaces SixLabors.ImageSharp 3.1.6, which had
open security advisories and a licence that needs reading for commercial use.
