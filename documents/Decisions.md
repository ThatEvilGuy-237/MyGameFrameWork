---
tags: [decisions, architecture]
aliases: [ADR, Why]
---

# Decisions

Why the engine is built the way it is. Change a decision here first, then the code.

> [!note] Two engines in one table
> Rows 1-17 were made for 0.1.0, the OpenTK 2D engine (tag `v0.1.0`). Rows 1-4, 12-13 and 16-17 still hold; the
> OpenTK rendering rows (5-11, 14) describe that engine only. Rows 18 and up are the Godot SDK (0.2.0).

| # | Decision | Why | Instead of |
|---|---|---|---|
| 1 | **Two packages**: Mathematics apart from the engine | game rules and tests use the math without a window or OpenGL | one package with folders |
| 2 | Groups are **real matrices** (`Matrix3x2`) | right at any depth, scale moves children; the old sums broke at three levels | adding moves and turns as numbers |
| 3 | A **canvas instance** passed to `Draw` | no hidden global state, testable with a fake renderer | the static `EvilUtils` |
| 4 | `Group()` returns a **using scope**, and an open group fails the frame with the scene's name | a forgotten `Pop()` is found at once | silently clamping the depth to 0 |
| 5 | **One batch**, one shader, a white texture for plain shapes | one draw call per texture run instead of a GPU buffer per shape | a buffer and draw per shape |
| 6 | Shaders **embedded** in the dll | works on Windows, Linux and in published builds | files read three folders above `bin/` |
| 7 | **Regions** on one texture | a sprite sheet is one texture and one batch | a new GPU texture per cutout, read from disk again |
| 8 | **StbImageSharp** for images | Unlicense or MIT, no open advisories; ImageSharp 3.1.6 had several HIGH ones | SixLabors.ImageSharp |
| 9 | OpenTK split into **Graphics + Windowing.Desktop** 4.9.4 | only what we use; every version checked by `evil-packages` | the whole `OpenTK` 4.8.2 bundle |
| 10 | **Our own `Key` and `MouseButton`** | games never touch OpenTK; the engine could change backend later | OpenTK's `Keys` in game code |
| 11 | Input is **event-based**, cleared per update | a tap shorter than a frame is not lost | comparing key state once per frame |
| 12 | **Scenes on a stack**, changes applied after the update | map, battle and dialog layers for the turn-based game; no scene vanishes mid-update | one big game class |
| 13 | The builder takes **values**, validates in `Build()` | the owner's rule for every SDK; wiring visible in `Program.cs` | lambdas or config sections |
| 14 | **`CaptureAndExit`** in the engine | every game gets screenshot checks on the server and on the Windows lab | per-game capture code |
| 15 | A **package-free test runner** | xunit is still marked review in `evil-packages`; switch when the owner approves it | blocking tests on approval |
| 16 | **.NET 10**, warnings as errors, lock files | one .NET for every app; restores are repeatable | net8.0 |
| 17 | **2D only** for now | the owner's scope (2026-11-08); the turn-based game needs 2D first | 3D cameras and depth |
| 18 | **Godot 4.7.2 underneath**, our API on top | full 3D (camera, lights, shadows, models) without writing a renderer; free, MIT, text scene files | our own OpenTK renderer (0.1.0) |
| 19 | Games see only **Evil Engine types and System.Numerics** | game code does not depend on Godot's API; wrappers keep nodes `internal`, constructors `private protected` | exposing `Node3D` and friends |
| 20 | **Numbered class libraries** 00-08 | one job per library; a library only uses lower numbers | one big library |
| 21 | **Each scene owns a world and a camera** | popping a scene removes its objects; overlays keep the world below visible | one shared world |
| 22 | One Godot line in a game: **`Main : GameMain`** | Godot needs a script on the main scene; everything else goes through the builder | games writing Godot nodes |
| 23 | Server checks use the **compatibility renderer** under xvfb | llvmpipe has OpenGL, not Vulkan; the PC renders with Forward+ | no screenshots on the server |
| 24 | Game projects pin Godot packages **through `Godot.NET.Sdk/4.7.2`** (central versions off there) | the SDK adds its packages itself (NU1009 otherwise) | duplicating versions |
| 25 | **Tests run inside Godot** (`tests/Evil.Engine.GodotTests`, headless) next to plain .NET tests | wrappers are only proven against real nodes; a planted failure gives exit code 1 | testing wrappers with fakes only |
| 26 | **Any Godot leak at exit fails the check** (tests and demo runs) | it caught a throwing Capsule, Torus and Spotlight leaving nodes behind, and music held by the audio thread at quit | ignoring exit warnings |
| 27 | **3D math lives in Mathematics and is cross-checked against Godot** | game rules use poses, rays and orbits without Godot; the tests prove they match what Godot draws | trusting two maths to agree |
| 28 | Settings that limit each other are **one settings record** (`OrbitLimits`) | `{ Pitch = 0, MinPitch = 0 }` gave 5 degrees because order mattered | separate min/max properties |
| 29 | **Atmosphere per scene, shown through its camera** (not `Environment`) | switching scenes switches the sky; the name does not clash with `System.Environment` | one global WorldEnvironment |
| 30 | Input from **Godot events**, cleared per frame; keys by **physical place** | taps inside a frame are kept; WASD works on any layout | polling Godot's Input singleton |
| 31 | **Picking in each object's own space** (exact turned boxes, real spheres), no physics | precise enough for tiles and units, nothing to set up | collision shapes on every object |
| 32 | `Display` drives **Godot's root Window** | a resize through DisplayServer never reached the game's screen size | DisplayServer calls |
| 33 | Content: **Godot-imported resources when imported, the file otherwise** | exported games work, mods and loose files work, one call | only res:// or only runtime loading |
| 34 | **Motion queued per object**, leftover time carried, starts read when a tween begins | turn-based sequences (face, hop, hop back) read like a script and match at any frame rate | one global list of tweens |
| 35 | Composition root is **`99_Engine`** | new libraries slot in below it without renumbering again | renumbering for every new library |
| 36 | **Audio proven by recording the final mix** in the driven run | the server has no speakers; silence fails the check | trusting that a player says it is playing |
| 37 | **Quit = stop audio, then quit three frames later**; window close goes the same way | stopping a sound only takes effect on the audio thread's next mix | quitting at once |
| 38 | **Code style: real types, constructor then statements**; no `var`, no initializer blocks, no target-typed `new()` | the owner reads the type on every line; enforced by the build (IDE0008) and `scripts/style-check.py`, docs included | `var` and JSON-looking object blocks |
| 39 | **`AmbientStrength` from the sky is the sky's share** (0 to 1); `Daylight()` takes a tenth | Godot ignores ambient energy for sky light, and the whole bright sky turned a green floor turquoise | a strength that silently did nothing |
| 40 | **The `Evil.Engine` package copies its libraries next to the game** (`build/Evil.Engine.props`) | Godot loads the game from its build folder and found no Evil DLLs there; found by building the Make a Game example from the packed feed | one more line in every game's project file |
| 41 | **Maths proven by seeded property tests**, brute force and Godot as a second opinion, with our own 60-line checker | rules over hundreds of inputs find what fixed examples miss (it found a sphere miss and a picking crash); no package to vet; the same seed every run | a property-testing package, or examples only |
| 42 | **Validators at the entry points**, naming the value (and the object) and where NaN comes from | NaN spreads silently and an object just disappears; failing where it enters points at the cause | checking nothing, or checking deep inside |
| 43 | **Ray and sphere by the gap at the closest point** | the textbook formula cancels two big numbers and missed small far spheres | the textbook quadratic |
| 44 | **Clean code is C#, object oriented, not JavaScript style** (owner), checked as a **ratchet** per file | 285 findings cannot all be fixed in one round; a ratchet lets every round lower them and stops any new one; the checker tests itself first | a one-off cleanup, or a rule nobody checks |
| 45 | **The Godot key table is derived from the names** (two rules, five exceptions) | 86 typed-out pairs were the biggest duplicate; checked equal to the old table pair by pair before it was deleted | a typed-out table |
| 46 | **One `ContentLoader` base, one small loader per kind**; files read at runtime become a cached `PackedScene` like imported ones | the four loaders repeated cache, imported-or-file and dispose; models from files are now parsed once, not on every call | four copies of the same recipe |
| 47 | **`RoundShape` under Cylinder and Cone**; each shape keeps one typed mesh field | the two shared their constructor and Height; every shape cast its mesh in every property | casts in every accessor; a generic `ShapeObject<TMesh>` would put a Godot type in the public API |
| 48 | **State lives in objects, not in parameters passed around**: `InputFeed`, `EarClipper`, `TweenQueue`/`LooseTweens` | functions that took the same two or three things on every call were a class waiting to happen | static helpers with long parameter lists |
| 49 | **Refactors prove they changed nothing**: the old code next to the new on random inputs, then removed | valid output is not the same as unchanged output; triangles and ray hits were compared on 50,000 inputs each | trusting that green tests mean no change |
| 50 | **Tests follow the same clean-code rules as the engine**: tests are `TestCase` objects, properties are classes with named case records, setup is shared in fixtures | tests are read as often as the code; the old ones passed lambdas, tuples and copied camera setups around | a lower bar for test code |
| 51 | **A `Grid` in Mathematics** with cells, paths and move range, breadth-first, blocked cells as a set | every tile game writes it; pure maths, so it is proven with properties, a brute-force oracle and Godot's AStarGrid2D | each game writing its own |
| 52 | **One start line from the engine** | a run that prints nothing looks the same whether it works or hangs | silence |
| 53 | **Every error says what to do next**, and the tests check that part of the message | a newcomer reads the message, not the source | messages that only say what went wrong |
| 54 | **`dotnet new evil-game`** from an `Evil.Engine.Templates` package in the same feed, lock-step version (pack.sh refuses a mismatch) | five setup files in every game were the biggest boilerplate (R13); one command, the same files every time | copying from the docs |
| 55 | **A new game builds only in the house style**: `.editorconfig` for `var`, a visible `CodeStyle.targets` for `new()` and initializer blocks | the compiler has no rule against those two; a file in the game, not hidden in the package, so its owner sees and can change it | a style that only the engine repo checks |
