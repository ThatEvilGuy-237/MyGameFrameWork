---
tags: [api, content]
aliases: [Models, Textures, Fonts, Loading]
---

# Content

`Game.Content` loads models, textures and fonts. Paths are relative to the content folder set with
`GameBuilder.Content(...)`. The default is the game project itself (`res://`).

```csharp
GameBuilder.Create(window).Content("res://content").Start(new MapScene()).Build();

var tower = World.Add(Game.Content.Model("models/tower.glb"));
tower.Part("Flag")!.Material = Material.Matte(Color.FromHex("#E5484D"));

floor.Material.Texture = Game.Content.Texture("textures/tiles.png");
floor.Material.TextureScale = new Vector2(12f, 12f);

_title = Game.Content.Font("fonts/Lato-Bold.ttf");
```

| Call | Gives | Files |
|---|---|---|
| `Model(path)` | a new `Model` each call (see [[World]]) | `.glb`, `.gltf` |
| `Texture(path)` | a shared `Texture`, loaded once | `.png`, `.jpg`, `.webp`, ... |
| `Font(path)` | a shared `Font`, loaded once (see [[Screen]]) | `.ttf`, `.otf` |
| `Sound(path)` | a shared `Sound`, loaded once (see [[Audio]]) | `.wav`, `.ogg` |

## Two ways in, one call

| The file is... | It is loaded... | Works in an exported game |
|---|---|---|
| inside the Godot project and imported by Godot | as Godot's imported resource | yes |
| anywhere else (an absolute path, a mod folder) | straight from the file at run time | yes, if the file is there |

A file that is missing, or that is not what was asked for, throws and names both the path you gave and where
it looked: `Content: 'models/nope.glb' was not found (looked at /games/x/models/nope.glb)`.

> [!important] Import before running on a fresh checkout
> Godot imports project assets in its editor. `scripts/check.sh` runs `godot --headless --import` on the demo
> first, and the `*.import` files next to each asset are committed. On the PC, opening the project in the
> editor does the same.

## Making the demo's content

| File | Made by |
|---|---|
| `content/models/tower.glb` | `blender -b --factory-startup --python content/models/make_tower.py -- tower.glb` (Blender 5.2.3 on the server); parts Base, Body, Rampart, Roof, Pole, Flag |
| `content/textures/tiles.png` | `python3 content/textures/make_tiles.py tiles.png` |
| `content/fonts/Lato-Bold.ttf` | Lato by Łukasz Dziedzic, SIL Open Font License 1.1 (`OFL.txt` beside it) |

![[r8-models.png]]
