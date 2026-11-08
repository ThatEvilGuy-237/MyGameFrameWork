---
tags: [loop, plan]
aliases: [Loop, Rounds]
---

# Godot SDK loop

Owner, 2026-11-08: "we gonna be full 3D ... implement these things ourselves cleanly, well split class libs,
folders like 00_x ... use Godot as our 3D engine and the other things they support we build the wrapper for ...
camera, light, 3D object, screen". Then: "implement this into well structured code, loop every 30 min".

**Goal:** Evil Engine becomes a clean C# SDK over Godot 4.7.2. Games talk only to our API (System.Numerics and
our own types); Godot types stay inside the SDK. Mathematics and Core stay free of Godot.

**Every round:** take the next unchecked round, build it, `scripts/check.sh` green, look at the screenshots
(saved in `documents/screenshots/`, linked from the round's log line), land it (commit, merge into main, push),
tick the box and add one log line. Test only what the round touched plus one driven run. Owner rules: no code
comments, values-only builder calls, packages vetted, .NET 10, short commits without trailers.

## Layout target

```
src/00_Core/Evil.Engine.Core                no dependencies: GameTime, Color, small shared types
src/01_Mathematics/Evil.Engine.Mathematics  no dependencies: 2D + 3D math, transform stacks, polygons, rays
src/02_Platform/Evil.Engine.Platform        the Godot glue: conversions, node helpers (only internal to the SDK)
src/03_World/Evil.Engine.World              WorldObject (3D groups), shapes (Box, Sphere, Floor), models, materials
src/04_Cameras/Evil.Engine.Cameras          Camera, orbit camera for a turn-based view
src/05_Lighting/Evil.Engine.Lighting        Sun, lamps, spotlights, sky and ambient
src/06_Input/Evil.Engine.Input              keyboard, mouse, picking objects under the mouse
src/07_Screen/Evil.Engine.Screen            window, capture, 2D overlay (our Group() canvas) and text
src/08_Engine/Evil.Engine                   GameBuilder, GameHost, GameContext, scenes (the composition root)
samples/OrbitDemo                           Godot project: the orbit demo in 3D
tests/Evil.Engine.Tests                     pure tests (Core, Mathematics)
tests/Evil.Engine.GodotTests                tests that run inside Godot against real nodes
```

## Rounds

- [x] **R0 spike**: Godot 4.7.2 .NET on the server (`/mnt/hdd/tools/godot-4.7.2`, intake clean, SHA-512 ok),
  packages vetted (Godot.NET.Sdk, GodotSharp, Godot.SourceGenerators, GodotSharpEditor 4.7.2). A node class in a
  separate library gets `_Ready`, `_Process` and `_Input` inside Godot; 3D renders under xvfb (llvmpipe,
  compatibility renderer); viewport capture works; .NET 10 target works.
- [x] **R1 skeleton**: tag the OpenTK engine `v0.1.0`, remove its runtime, numbered layout, Core + Mathematics +
  Platform + World (WorldObject group, Box, Sphere, Plane) + Camera (basic) + Lighting (Sun) + Engine (builder,
  host, scenes with their own world); OrbitDemo renders through our API; check.sh runs Godot under xvfb.
- [x] **R2 Godot tests**: `tests/Evil.Engine.GodotTests` runs `[Test]` methods inside Godot and exits with a code;
  WorldObject and scene worlds covered; check.sh runs both suites.
- [x] **R3 World deep**: 3D math in Mathematics (Transform3D, Ray, Bounds, Quaternion helpers, LookAt), WorldObject
  `ToWorld/ToLocal`, more shapes (Cylinder, Capsule, Torus?), materials (colour, roughness, metal, texture),
  `Destroy`, names and finding.
- [x] **R4 Camera**: perspective and orthographic, field of view, near/far, `LookAt`, `OrbitCamera` (turn, tilt,
  zoom, pan, smooth) for a tactics view; screen point to world ray.
- [x] **R5 Lighting**: Sun with shadows, Lamp (point), Spotlight, Environment (sky colour, ambient, fog, glow),
  light presets.
- [x] **R6 Input**: event-based keyboard and mouse (taps never lost, same API as the 2D engine), mouse picking
  of WorldObjects through the camera ray.
- [x] **R7 Screen**: window settings applied (title, size, fullscreen, VSync), `CaptureFrame`, `CaptureAndExit`,
  2D overlay with our `Group()` canvas and text (Godot fonts), screen size changes.
- [ ] **R8 Models**: textures on materials (moved here from R3, one place for file loading), load `.glb` / `.gltf` from Content at runtime, a Blender-made test model through the Blender
  MCP, model parts as WorldObjects.
- [ ] **R9 Motion**: tweens for moves and turns (move to a tile over time, ease curves), pure timing in Core,
  applied each frame; waiting for a move to finish (turn-based).
- [ ] **R10 Audio**: sounds and music wrappers (one-shot, loop, volume groups).
- [ ] **R11 Docs and pack**: documents/ rewritten for the Godot SDK, Decisions updated, `0.2.0` packed.
- [ ] **R12 On the PC**: build and run OrbitDemo on the owner's Windows PC through the Windows lab (**ask the
  owner first**, standing rule).

## Log

- 2026-11-08 17:05 R0: spike in the session scratchpad; library-node callbacks, input and capture proven.
- 2026-11-08 17:15 R1: numbered libraries 00-05, 07, 08 over Godot; `Main : GameMain` drives scenes that own a world
  and a camera; OrbitDemo (3 nested groups, sun, shadows) renders through our API; check.sh green (27 pure tests,
  Godot frame). Open: no ambient light yet (dark sides, R5). ![[r1-orbit.png]]
- 2026-11-08 17:40 R2: `tests/Evil.Engine.GodotTests` (Godot project, headless) runs 26 tests inside Godot:
  WorldObject groups (3 levels, parent scale, LookAt), shapes, camera, sun, scene worlds (hide, overlay, pop frees,
  switch), builder; exit code 1 on a planted failure; shared runner in `tests/Shared`; check.sh runs both suites
  (27 + 26 green); the demo frame is unchanged from R1. ![[r1-orbit.png]]
- 2026-11-08 18:05 R3: 3D math without Godot (Pose, Rotations, Bounds3, Ray3) cross-checked against Godot;
  WorldObject Orientation, Pose, WorldPose, Forward/Right/Up, ToWorld/ToLocal, Destroy, Find, Descendants,
  LookAt in a turned parent; Material (shareable, Matte/Shiny/Metal/Glowing); Cylinder, Cone, Capsule, Torus.
  Found and fixed: a throwing Capsule/Torus leaked its Godot node; check.sh now fails on Godot leaks. 33 pure +
  37 Godot tests green. Textures moved to R8. ![[r3-world.png]]
- 2026-11-08 18:30 R4: Camera projection (perspective, orthographic), ScreenToRay, PointOnGround, WorldToScreen,
  CanSee; OrbitCamera (turn, tilt, zoom, pan, smooth glide, Snap) with an `OrbitLimits` settings record after a
  test showed `{ Pitch = 0, MinPitch = 0 }` depended on property order; pure `Orbit` and `Smoothing` maths. The demo
  circles with the orbit camera and a glowing ring marks where the screen middle hits the floor (it lands at
  640,358 of 1280x720). 36 pure + 45 Godot tests green. ![[r4-cameras.png]]
- 2026-11-08 19:00 R5: Light base (colour, intensity, shadows); Sun presets (Noon, Golden, Moonlight) and
  ShadowDistance; Lamp (range, falloff, Warm/Cool); Spotlight (direction, range, angle, softness); Atmosphere
  (clear/solid/gradient background, ambient incl. from sky, fog, glow, tone mapping, exposure; Studio, Daylight,
  Dusk, Night) per scene through its camera, default ambient so dark sides are gone. The leak check caught a real
  one: `new SpotLight3D { SpotAngle = Checked(angle) }` builds the light before the check runs. Dusk preset toned
  down after the first shot washed colours out. 36 pure + 53 Godot tests green. ![[r5-lighting.png]]
- 2026-11-08 19:30 R6: library 06_Input: Keyboard and Mouse fed from Godot events (taps inside a frame kept,
  repeats ignored, let go on focus loss), our `Key` list mapped to Godot physical keys (every key exactly once),
  `Picker`/`Scene.Pick` with exact local-space hits (turned box, real sphere), `Pickable`, hidden skipped,
  `WorldObject.Tag` and `Ancestor<T>()`. Demo is interactive (Space, Q/E, wheel, right-drag, hover glow, click
  select, F12). check.sh now drives it with real X11 input: the mouse sweep found both pawns, the click selected
  pawn-red, three wheel steps zoomed 13 to 8. 36 pure + 63 Godot tests green. ![[r6-input.png]]
- 2026-11-08 20:00 R7: 2D overlay: `Canvas` (groups, rects, lines, circles, triangles, polygons, text at a point,
  centred in a box, wrapping paragraphs, measuring) drawn over the 3D world through Godot's CanvasItem, scenes
  get `Draw(Canvas)`; `Game.Display` (title, full screen, VSync, resize). Found: resizing through `DisplayServer`
  shrank the X window but Godot never reported it (ScreenSize stuck at 1280x720 four seconds later); now the root
  `Window` node is driven and the game sees 1024x640 next frame. Demo HUD: info panel, compass (rotated group with
  text), Move/Attack/Wait bar with UI clicks before 3D picks. Driven check now also resizes and clicks Attack.
  Full screen cannot be shown under xvfb (no window manager). llvmpipe runs the lit demo at about 8 fps.
  36 pure + 71 Godot tests green. ![[r7-screen.png]]
