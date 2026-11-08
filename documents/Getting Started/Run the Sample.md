---
tags: [getting-started, how-to, testing]
aliases: [Sample, OrbitDemo, Check]
---

# Run the Sample

`samples/OrbitDemo` uses every part of the engine: groups orbiting groups, Blender towers, a tiled floor, dusk
lighting, picking, motion, a HUD with text and buttons, music and sound effects.

## On a desktop

Open `samples/OrbitDemo` in Godot 4.7.2 (.NET) and press play.

| Input | Does |
|---|---|
| hover and click a pawn | glows, then selected: a ring at its feet and the spotlight on it |
| Move / Attack / Wait | walk two tiles, hop at the other pawn, or spin; the bar locks while it plays |
| Space | pause the spinning and the camera's slow turn |
| Q / E, right-drag, wheel | turn, orbit and zoom the camera |
| M | mute the music |
| F10 / F11 | small window / full screen |
| F12 | screenshot next to the game |
| Escape | quit |

## On the server

```bash
./scripts/check.sh
```

| Step | Passes when |
|---|---|
| code style | `scripts/style-check.py` finds no `var`, no initializer block, no target-typed `new()` (code and doc examples) |
| build | everything builds with warnings as errors and explicit types enforced |
| pure tests | Core and Mathematics, without Godot |
| Godot tests | every wrapper against real Godot nodes, headless; no Godot leaks at exit |
| import | Godot imports the demo's models, textures, fonts and sounds |
| frame 90 | the demo renders under xvfb, `documents/screenshots/orbit.png` is saved, no leaks |
| driven run | real X11 keys and mouse: pause, find a pawn, select it, zoom, resize to 1024x640, Move, Attack, mute; every step logged; the recorded game audio is not silent; no leaks |

Open the two screenshots after a green run. A green run with a wrong picture is not done.

![[r10-audio.png]]
