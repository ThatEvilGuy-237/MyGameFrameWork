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
src/08_Motion/Evil.Engine.Motion            tweens queued per object (added in R9)
src/09_Audio/Evil.Engine.Audio              sounds, music, volume groups (added in R10)
src/99_Engine/Evil.Engine                   GameBuilder, GameHost, GameContext, scenes (the composition root, always last)
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
- [x] **R8 Models**: textures on materials (moved here from R3, one place for file loading), load `.glb` / `.gltf` from Content at runtime, a Blender-made test model through the Blender
  MCP, model parts as WorldObjects.
- [x] **R9 Motion**: tweens for moves and turns (move to a tile over time, ease curves), pure timing in Core,
  applied each frame; waiting for a move to finish (turn-based).
- [x] **R10 Audio**: sounds and music wrappers (one-shot, loop, volume groups).
- [x] **R11 Docs and pack**: documents/ rewritten for the Godot SDK, Decisions updated, `0.2.0` packed.
- [ ] **R12 On the PC**: build and run OrbitDemo on the owner's Windows PC through the Windows lab (**ask the
  owner first**, standing rule). Waits for the owner; the loop skips it.

## Second loop: structure, readability, usability

Owner, 2026-11-08 after R11: "loop on improving the code structure and readability, also usability for people to
code with", and "do some math validators and tests to be sure you're right". Every 1 minute (session cron), one round at a time, same gate (`scripts/check.sh`, screenshots, land the
work). Readability is through names, small methods and files, never comments (house rule). Public API changes are
allowed (0.3.0) but each one gets a test and a line in Decisions.

- [x] **R13 Fresh eyes**: one agent that never saw the code writes a small turn-based game (tiled board, two teams,
  click a unit, click a tile to walk there, Space ends the turn, a HUD line) with only `documents/` and the packed
  `Evil.Engine` 0.2.0. Every place it got stuck, guessed or wished for something goes into
  `documents/Usability.md` as a numbered finding with severity; the rounds below fix them, worst first.
- [x] **R14 The sample reads like a guide**: OrbitScene (426 lines, `Update` 91 and `Enter` 85) split by concern
  (board, pawns, action bar, camera controls, sound) so a reader follows it top-down; no method over ~30 lines.
- [ ] **R15 Math proven**: owner, 22:43: "also do some math validators and tests to be sure you're right". A seeded
  random property checker in `tests/Shared` (no new packages; a failure prints the seed and the input), properties
  for every type in Core and Mathematics (round trips, inverses, invariants), brute-force validators for the
  geometry (ray hits against marching, triangulation against area and point sampling), cross-checks of the 3D maths
  against Godot's own Basis, Quaternion, Transform3D and AABB inside the Godot suite, and input validators: maths
  entry points reject NaN, infinity and impossible values with a message that says what to do.
- [ ] **R16 One type per file, folders by concern**: ObjectTweens.cs (8 types), Background.cs (4) and
  GameOptions.cs (2) split; libraries with many files get subfolders (World: shapes, content); namespaces stay.
- [ ] **R17 An API that guides**: fixes for R13's findings: consistent verbs and parameter order, shortcuts where
  every game repeats the same lines, error messages that say what to do next. From R13: an engine line when the
  game starts, `Ancestor` that can include the object itself, a `Grid` type (cells, cell to world, neighbours,
  paths) proven with property tests. Each change with a test.
- [ ] **R18 Readable internals**: the longest methods (GodotKeys.Build 105 lines, MotionPlayer.Update 49,
  GameMain._Process 47, InputFeed, GameBuilder.Build) cut into named steps.
- [ ] **R19 A new game in one command**: `dotnet new evil-game` template package in the feed that writes the Make a
  Game files plus the code style checks (`.editorconfig`, `EnforceCodeStyleInBuild`); make-a-game-check builds from it.
- [ ] **R20 Fresh eyes again**: a new agent, same task as R13; compare (stuck N times before, M now) and fix what is
  left.
- [ ] **R21 Docs and pack 0.3.0**: API notes and Decisions for every change, `0.3.0` packed, make-a-game-check on the
  feed.

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
- 2026-11-08 20:30 R8: `Game.Content` (Model, Texture, Font) from a content folder (`GameBuilder.Content`, default
  `res://`): Godot-imported resources when imported, straight from the file otherwise; textures and fonts loaded
  once; missing files name where they looked. `MeshObject` base for shapes and `ModelPart`s, so model parts pick
  and paint; `Model.Parts/Part/Paint`; `Material.Texture/TextureScale`; `canvas.Font` (reset per scene and frame).
  Demo: tiled floor, two Blender towers (made with the Blender 5.2.3 CLI from a committed script rather than the
  MCP, so it can be rebuilt), outpost flag repainted, Lato title. check.sh imports the demo first. Test slips caught
  on the way (array compare, drawing outside `_draw`, unfreed test nodes). 36 pure + 78 Godot tests green.
  ![[r8-models.png]]
- 2026-11-08 21:00 R9: easing curves in Core (10 curves, pure tests); new library `08_Motion` (engine moved to
  `09_Engine` so the numbers still only point down): `Game.Motion` with MoveTo, Hop, MoveAlong (faces each step),
  Face, TurnTo, Spin, ScaleTo, ColorTo, Wait, Value; one queue per object, leftover time carried over, starting
  values read when a motion's turn comes; `Tween.IsDone/Progress/Finished` (awaitable), Stop, Complete, Speed;
  destroyed targets dropped. Demo actions now act: Move walks two tiles, Attack hops in and knocks the enemy back,
  Wait spins; the bar locks while a pawn is busy. Driven check clicks Move, waits for `arrived:`, clicks Attack,
  waits for `done: attack`. 38 pure + 89 Godot tests green (motion tests passed on the first run).
  ![[r9-motion.png]]
- 2026-11-08 21:30 style pass (owner): explicit types instead of `var` everywhere (559 rewritten by `dotnet format`,
  the build now fails on `var` via IDE0008); objects built with their constructor and then statements, no
  initializer blocks and no target-typed `new()` (150 rewritten; `scripts/style-check.py` runs first in check.sh and
  was proven to catch a planted block). Rules in CLAUDE.md. All suites green afterwards (38 + 89, driven run).
- 2026-11-08 22:00 R10: library `09_Audio` (engine moved to `99_Engine`, so it stays last whatever comes next):
  `Game.Audio` with Play, PlayAt (follows a world object), PlayMusic with crossfade and loop, StopMusic, StopAll,
  volume groups as Godot buses (Music, Effects, Ambience) with SetVolume/Mute/MasterVolume, StartRecording and
  SaveRecording; `Game.Content.Sound` (.wav, .ogg). Demo: music, button click, hop from the attacker, hit from the
  enemy, M mutes. Proof without speakers: the driven run records the final mix, 22.7 s at level 0.044, and
  check.sh fails on silence. Found: music and its playback leaked at quit in 4 of 6 runs (audio thread race); the
  engine now stops all audio and quits 3 frames later (6 of 6 clean), window close goes the same way, and
  check.sh fails if any demo run leaks. 38 pure + 95 Godot tests green. ![[r10-audio.png]]
- 2026-11-08 22:13 R11: documents/ rewritten for 0.2.0: Home, README, Getting Started ([[Make a Game]], [[Run the Sample]]),
  Architecture (start and quit order, the check table), every API note's examples in the code style (style-check now
  reads C# blocks in Markdown too), [[From MyGameFrameWork]] as three versions with the original calls mapped, Decisions
  25-40. `pack.sh` packs all 11 libraries. A new game made only from the Make a Game note, restored from the packed
  feed, found two bugs: Godot could not load a packaged game (no Evil DLLs next to it; the `Evil.Engine` package now
  ships `build/Evil.Engine.props` to copy them) and `Daylight()` turned the green floor turquoise (hue 149° to 175°;
  Godot ignores ambient energy for sky light, so `AmbientStrength` from the sky is now the sky's share and Daylight
  takes 0.1, floor 159°). That run is now `scripts/make-a-game-check.sh`. 38 pure + 96 Godot tests green, driven run
  green, Dusk sample unchanged. ![[make-a-game.png]]
- 2026-11-08 23:42 R14: OrbitScene split by job into 9 small classes (Scenery, SpinningShapes, Pawns, ActionBar,
  PawnActions, CameraControls, Hud, DemoSounds); the scene went from 426 to 164 lines, `Update` from 91 to 21, the
  longest method in the sample is now 30 lines (HandleKeys, one key per block). Named constants instead of bare
  numbers for speeds and button layout; the attack reads as HopAtAndBack + KnockBack. Found on the way: F12 only
  worked in the scripted run although README and Run the Sample promise a screenshot; now F12 saves
  `orbit-<time>.png` next to the game on a desktop (tested: 1280x720 saved, exit 0), git ignores them. Run the
  Sample has a file-by-file reading guide. Behaviour unchanged: every driven-run line seen, 38 + 96 tests green,
  audio 24.5 s at 0.043. ![[orbit-driven.png]]
- 2026-11-08 23:47 R13: a Sonnet agent that never saw the code built an 8x8 turn-based board game (two teams, select,
  animated path walk, Space ends the turn, HUD, Escape) from only the docs and the packed 0.2.0: it works, every
  bullet met, 5 builds, no blocker, nothing read from the source. 13 findings in [[Usability]] (3 major, 9 minor):
  fixed now the docs-only ones (screenshot calls in Screen, driving a game on the server in Testing, the usings as
  a code block with `System.Collections.Generic`, the script path, deriving from WorldObject, Torus, MoveAlong's
  path, Unshaded for exact colours, the tile-under-the-mouse recipe, an honest style note); the API and tooling
  ones went into R17 and R19. Found while checking: `Ancestor<T>()` skips the object itself. Make a Game check
  green on the edited note. ![[r13-fresh-eyes.png]]
