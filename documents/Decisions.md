---
tags: [decisions, architecture]
aliases: [ADR, Why]
---

# Decisions

Why the engine is built the way it is. Change a decision here first, then the code.

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
