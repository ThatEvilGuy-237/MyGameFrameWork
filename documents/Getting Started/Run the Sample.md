---
tags: [getting-started, how-to, testing]
aliases: [Sample, Check]
---

# Run the Sample

`samples/Evil.Engine.Sample` is the old MyGameFrameWork demo on the new engine, plus the parts the old one
could not do.

## On a desktop

```bash
dotnet run --project samples/Evil.Engine.Sample
```

| Key | Does |
|---|---|
| Space | stop or start the spin |
| Up / Down | spin faster or slower |
| P | pause overlay (a second scene on top), P or Escape to leave it |
| F12 | save a screenshot next to the game in `screenshots/` |
| Escape | quit |
| mouse | the star turns orange under the pointer, even while it turns |

What is on screen:

| Thing | Shows |
|---|---|
| red and green rectangles | the original demo: groups that turn on their own |
| blue square, yellow star, white moon | three nested groups: the star follows the square, the moon follows the star (see [[Grouped Transforms]]) |
| the star | a concave polygon filled by triangulation, and mouse picking through `canvas.ToLocal` |
| gem bottom-left | a sprite sheet cut with `Grid(16, 16)`, two frames, pixel-sharp (`TextureFilter.Nearest`) |
| hexagon bottom-right | a texture stretched over a polygon's bounds |

## On the server (no screen)

```bash
./scripts/check.sh
```

1. Builds the whole solution in Release with warnings as errors.
2. Runs the test runner (`tests/Evil.Engine.Tests`, no packages).
3. Runs the sample under `xvfb` and saves frame 90 to `documents/screenshots/sample.png`.
4. Runs it again and drives it with real input (`scripts/drive.sh`: `xdotool` presses Space, finds the yellow
   star on screen, moves the mouse onto it, presses P) and saves `documents/screenshots/sample-driven.png`.

**Done when** the script prints `60 of 60 tests passed` and both screenshots exist. The driven one must show a
dimmed scene, the pause bars and an orange (dimmed: brown) star.

![[sample-driven.png]]

> [!note] Any game can do this
> `GameBuilder.CaptureAndExit(path, frame)` saves that frame and closes the game, so every game built on the
> engine gets screenshot checks for free.
