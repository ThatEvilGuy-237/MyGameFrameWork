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
- [x] **R15 Math proven**: owner, 22:43: "also do some math validators and tests to be sure you're right". A seeded
  random property checker in `tests/Shared` (no new packages; a failure prints the seed and the input), properties
  for every type in Core and Mathematics (round trips, inverses, invariants), brute-force validators for the
  geometry (ray hits against marching, triangulation against area and point sampling), cross-checks of the 3D maths
  against Godot's own Basis, Quaternion, Transform3D and AABB inside the Godot suite, and input validators: maths
  entry points reject NaN, infinity and impossible values with a message that says what to do.
Owner, 23:20, what clean code means here: "more like C#, less like JavaScript style coding, aka object oriented,
and preferably no duplicate code; a file needs meaning, not just code here or there, and folders need meaning".
Measured before R16 in `src/`: 45 lambdas, 49 LINQ calls, 12 tuple types, 14 static classes, 4 files with several
types, and repeated code in Keyboard/Mouse, ContentLibrary's loaders, GodotKeys' table, the shapes and the tests.

- [x] **R16 Clean-code rules and checks**: the owner's rules in CLAUDE.md and an Architecture note; a duplicate
  detector (`scripts/duplicate-check.py`, exact and same-shape-with-other-names) and JS-style rules in the style
  check (lambda logic, LINQ chains, tuples as data, `switch` on strings, `Func`/`Action` parameters, several types in
  one file). check.sh runs them as a ratchet: a committed baseline per file, a count may only go down, the rounds
  below bring it to zero.
- [x] **R17 Folders and files with meaning**: one type per file in every library, folders by meaning inside each
  (World: Objects, Shapes, Materials, Models; Input: Keyboard and Mouse, Picking; Screen: Window, Canvas, Capture;
  Motion: Tweens), namespaces unchanged so games do not break.
- [x] **R18 No duplicate code in the engine**: one button-state class for Keyboard and Mouse, one loader per
  content kind behind one base class in ContentLibrary, the Godot key table derived instead of typed out, the shapes'
  checks shared, MusicTrack's repeats folded. Each with its tests still green.
- [x] **R19 Object-oriented engine**: lambdas, LINQ chains and tuples in `src/` replaced by named methods and named
  types, long methods (GodotKeys.Build, MotionPlayer.Update, GameMain._Process, InputFeed, GameBuilder.Build) cut
  into named steps; static classes only where they are pure maths like `System.Math`.
- [x] **R20 Object-oriented sample and tests**: the demo's actions become classes (`MoveAction`, `AttackAction`,
  `WaitAction` : `PawnAction`) instead of a `switch` on strings; property tests become classes with named case
  records that the runner finds by itself; shared test setup (camera, stage) in one place; test folders by meaning.
- [x] **R21 An API that guides**: R13's findings: an engine line when the game starts, `Ancestor` that can include
  the object itself, a `Grid` type (cells, cell to world, neighbours, paths) proven with property tests, error
  messages that say what to do. Each change with a test.
- [x] **R22 A new game in one command**: `dotnet new evil-game` template package in the feed that writes the Make a
  Game files plus the code style checks (`.editorconfig`, `EnforceCodeStyleInBuild`); make-a-game-check builds from it.
- [x] **R23 Fresh eyes again**: a new agent, same task as R13; compare (stuck N times before, M now) and fix what is
  left.
- [ ] **R24 Docs and pack 0.3.0**: API notes and Decisions for every change, `0.3.0` packed, make-a-game-check on the
  feed and checking the new start line.

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
- 2026-11-08 23:59 R15: math proven three ways. Our own seeded property checker (`tests/Shared/Property.cs` + `Gen.cs`,
  300 cases per rule, the seed and count changeable): 41 rules over Core and Mathematics (easing ends, clamps,
  order and mirror symmetry; hex round trips; angles; smoothing the same at any frame rate; rects; the transform
  stack; polygons, triangles, centroids; turns against a hand-written Rodrigues; Euler order; LookAlong; poses,
  matrices and composition; orbits), brute force for rays (walking in 1 cm steps) and polygons (sampled points),
  and 8 cross-checks against Godot's own maths. Validators refuse NaN, infinity and impossible values at every
  entry point with a message that says what to do; world objects name themselves. Found and fixed: (1) a ray
  grazing a small far sphere missed it (float cancellation; now the gap at the closest point; regression test
  fails on the old code), (2) any object scaled to zero made every click throw "Matrix determinant is zero"
  (picking now skips it; the test fails without the fix), (3) my own star property was wrong (a 3-point star with
  fat arms is convex: the engine was right). Stress: 41 rules x 20,000 cases x 5 seeds = 4.1 million inputs and
  the Godot checks at 3,000 x 2 seeds, all green. Tests 38 -> 82 pure, 96 -> 104 Godot; check.sh green, driven run
  unchanged. ![[orbit-driven.png]]
- 2026-11-09 00:06 R16: the owner's clean-code rules in CLAUDE.md and [[Clean Code]]; `scripts/clean-code-check.py` counts per
  file anonymous functions, LINQ, tuples as data, switches on strings or numbers, Func/Action, extra types per file
  and duplicated code (exact 5 lines, or 8 lines of the same shape with other names; enum lists skipped). It tests
  itself on 25 planted cases first (two of its own false alarms found that way: `MathF.Max` as LINQ, a record's
  parameters as a tuple) and runs in check.sh as a ratchet: baseline 291, a planted lambda fails the check. Found:
  the style check only read files git already tracked, so R15's new tests slipped a target-typed `new (...)` in;
  both checks now read new files too, and the test's int switch over a tuple array became a `TransformStep`
  record hierarchy (Move, Turn, Scale). 285 left, 125 in `src/`. check.sh green.
- 2026-11-09 00:09 R17: every engine file holds one type, named after it, and the bigger libraries have folders by meaning
  (Mathematics 2D/3D; World Objects/Shapes/Materials/Models; Lighting Lights/Atmosphere; Input Keyboard/Mouse/Picking;
  Screen Window/Canvas/Overlay/Capture; Motion Tweens; Engine Hosting/Content/Scenes). Split: ObjectTweens.cs (8
  tweens), Background.cs (4 records), GameOptions.cs (2), SceneStack's nested change types. Moved with `git mv` so
  history follows; namespaces unchanged, so no game breaks; unused usings left by the split removed (15, by
  `dotnet format` with IDE0005 on for that run). Found on the way: my splitter deleted `Background` when a part had
  the source's own name (caught by the build, restored from git, splitter fixed); and the clean-code ratchet keyed
  files by path, so every moved file looked new: it now keys by project and file name (self-tested). Types per file
  in `src/`: 13 -> 0; clean code 285 -> 272 (112 in `src/`). check.sh green, tests and driven run unchanged.
- 2026-11-09 00:20 R18: no duplicate code left in `src/`. Keyboard and Mouse use one `ButtonStates<T>`; the Godot key table
  is derived from the names (two rules, five exceptions; a one-time test checked it equal to all 93 old pairs
  before the table went); content loading is one `ContentLoader` base with a loader per kind and wav/ogg as
  `SoundFormat` classes instead of a string switch, and runtime glTF files are packed into a cached `PackedScene`
  (a new test loads a model, deletes the file, loads again: two separate copies from one parse); Cylinder and Cone
  share `RoundShape`; every shape keeps one typed mesh field instead of a cast per accessor; MusicTrack's fade end
  calls Release instead of repeating it; Ray3 indexes `Vector3` instead of switching on the axis number. New tests:
  renamed keys, the two copies, and a property test that replays random button scripts against a history-based
  spec (5,000 cases green; a planted ReleaseAll bug fails it on case 1). The checker now also catches string arms
  in switch expressions and skips declarations and tables in its same-shape mode, each with self-test cases.
  Clean code 272 -> 177, `src/` 112 -> 31. 82 + 107 tests, check.sh and driven run green. ![[orbit.png]]
- 2026-11-09 00:29 R19: the engine has no clean-code findings left (`src/` 31 -> 0). MotionPlayer routes to a `TweenQueue`
  per object and `LooseTweens` (both a `TweenGroup`, after the ratchet caught the two sharing code) instead of
  lambdas over dictionaries; Model.Part loops; MusicTrack owns its player and replays through a named method;
  the triangulator is an `EarClipper` object with a `Corner` record instead of static functions passing three
  lists; Ray3's box test narrows an `Interval` per axis instead of a tuple swap; InputFeed is an object that owns
  the keyboard and mouse; the game loop, the mouse feed and GameBuilder.Build are named steps. Proof the
  refactors changed nothing: the old triangulator and ray-box code ran next to the new on 50,000 random inputs
  each with identical output (then removed), 3 new seeds x 20,000 property cases green, and two behaviour drifts
  I made were caught in review and undone (the clock summed floats; a null content root would have thrown the
  wrong error). The checker learned `ToList`/`Concat`, stopped reading `Type? Name =>` as a lambda, and got a
  long-method rule (over 30 lines; 4 left in the sample and tests). 82 + 107 tests, check.sh and the driven run
  green. ![[orbit-driven.png]]
- 2026-11-09 00:33 R20 part a (the sample): zero clean-code findings in the demo (13 -> 0). The Move/Attack/Wait `switch`
  on strings became `PawnAction` classes run by an `ActionRunner`; the hit that lands after the hop is a
  `SoundCue`; the command-line switch became one class per flag; LINQ and lambdas became loops; the 36-line key
  handler became game keys and window keys; files sit in World/, Actions/, Hud/, Camera/, Audio/, Options/ with
  their Godot `.uid` files. The driven run clicked Move and Attack through the new classes (arrived, hit sound,
  done). Run the Sample's reading guide follows the folders. R20 stays open for the tests (parts b to d).
- 2026-11-09 00:40 R20 parts b and c (the pure tests): every test is a `TestCase`; a `[Test]` method runs as a `MethodTest`,
  and a property is a class `Property<TCase>` with `Generate` and `Verify` that the runner finds by itself, instead
  of `Property.ForAll` with two lambdas and a tuple. The 39 pure properties became classes with named case records,
  in folders by meaning (Core/EaseCurves, Core/Colors, Mathematics/Angles, Bounds, GoalSmoothing, Orbits, Polygons,
  Poses, Rays, Rectangles, Transforms, Turns, Validators); folder names avoid engine type names because a namespace
  `Rotations` would hide the `Rotations` class next to it. A `Triangulation` helper object replaces three loose
  helper functions; the validator tests name every refused call (`RayFromBrokenOrigin`, `WrapNaN`, ...). The runner
  itself is an object and lost its LINQ. Proof: 83 tests green (the validators now four tests), 2 more seeds at
  20,000 cases green, a planted failing property printed FAIL with its seed and value and exit code 1. Clean code
  154 -> 87. Godot tests (part d) still use `Property.ForAll`, which goes when they move.
- 2026-11-09 00:47 R20 part d (the Godot tests and the rest): **zero clean-code findings in the whole repository** (291 at
  the start of R16). 41 `Check.Throws(() => ...)` became named local functions (`ZoomToNothing`, `ReadADestroyedHand`);
  the six Godot maths cross-checks and the button-history check are property classes with named probes; five
  copied camera setups became `stage.AddCamera` and `Stage.ScreenMiddle`; nested helper types got their own files
  (`RecordingTarget` with `DrawnText` records, `SkyScene`, `ProbeScene`, one `EmptyScene` instead of two); the scene
  stack tests share a `MapAndBattle` fixture; the 33-line music test became three tests; the motion and screen
  tests lost their tuple setups; the static `Property.ForAll` is gone; the Godot suite sits in folders by library.
  The checkers now skip files deleted but not yet unstaged (they crashed on one). One named allowance: the
  `Action` that `Check.Throws` takes. 83 + 109 tests green, check.sh and the driven run green. R20 done.
- 2026-11-09 00:56 R21: the API guides. A `Grid` in Mathematics (cells on the ground, `CellAt`, `CenterOf`, 4- or
  8-way `Neighbours`, shortest `Path` around blocked cells in the shape `MoveAlong` wants, `Reachable` for move
  range), proven by 5 properties (20,000 cases x 2 seeds), a brute-force distance table built by repeated
  relaxation, and Godot's `AStarGrid2D` agreeing on step counts over 3,000 boards; planted bugs (ignoring walls,
  no diagonals) fail them on case 1 to 3. `SelfOrAncestor<T>()` finds the clicked object or its group (the demo's
  hand loop is gone). The engine prints one start line (`Evil Engine 0.2.0: "Evil Engine orbit demo" 1280x720, first
  scene OrbitScene, content res://content`) and check.sh fails without it. Ten error messages now say what to do
  next (Destroy and IsDestroyed, using canvas.Group(), World.Add, Push a scene or Game.Exit, ...) and the tests check
  that advice. Usability findings 3 and 10 and the Ancestor note fixed. The ratchet caught my own new findings
  (a renamed `RandomGrids.Create` that read as LINQ `Any`, a 35-line oracle method) and one false alarm it now
  understands (`where T : X =>`). 91 + 112 tests, check.sh and the driven run green.
- 2026-11-09 01:07 R22: `dotnet new evil-game -n Skirmish --title "Tile Skirmish"` makes a game: project file, Main.cs, MapScene.cs,
  usings, Godot project, nuget.config, `.gitignore`, and the code style in the build (`.editorconfig` refuses `var`;
  a visible `CodeStyle.targets` refuses `new()` and initializer blocks, naming the file and what to write instead,
  because the compiler has no rule for those two). `Evil.Engine.Templates` packs with the engine; pack.sh refuses a
  template that asks for another engine version. make-a-game-check now builds from the template (private template
  and package folders), plants `var`, `new()` and an initializer block and needs each to fail, needs the start
  line, and still checks floor, title, Escape and leaks: green on a trial feed. Make a Game shows the template's
  files and check.sh fails when they drift (a planted change was caught). Found: the template engine treats MSBuild
  `Condition` attributes as its own and stripped one, so every new game failed its style check; the file is
  copy-only now. pack.sh died silently on an `ls` exit code under pipefail; fixed. ![[make-a-game.png]]
- 2026-11-09 01:23 R23: a new Sonnet agent, the same task as R13, a trial pack of `main` (old packages cleared from the
  NuGet cache, the earlier findings hidden). It used `dotnet new evil-game`, `Grid.Path`, `CellAt` with
  `PointOnGround` and `SelfOrAncestor<Unit>()` "without friction"; every bullet met. R13: 13 findings, 3 major, about
  100 lines of hand-made board code. R23: 10 findings, 0 major, the board 69 lines with paths from `Grid`. All 10
  fixed: `System` in the template's usings (and `System.Linq` out); a new [[Scenes]] note (life of a scene,
  `GameContext`, helper classes, logging); the truth about `Unshaded` and tone mapping; material ownership and camera
  defaults written down; a recipe for screen positions in scripted runs; the clean-code rules for games and the
  30-line limit; the start line printed before the first scene; `CodeStyle.targets` as a small task that lists every
  problem with file and line in one build (EVIL001, EVIL002, next to the compiler's IDE0008: the Make a Game check
  plants all three in one file and needs each at its line); `Camera.Frame(bounds, pitch, yaw)` and `Grid.Bounds`,
  proven by a Godot property test that every corner of 2,000 random boxes is in view (a planted too-close camera
  fails on case 1). 92 + 113 tests, check.sh, the driven run and make-a-game-check green. ![[r23-fresh-eyes.png]]
