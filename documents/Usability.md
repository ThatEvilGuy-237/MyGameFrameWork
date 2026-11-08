---
tags: [usability, findings, loop]
aliases: [Fresh eyes, Newcomer findings]
---

# Usability

What a newcomer runs into when they make a game with Evil Engine, and what we did about it. Each finding comes
from a real attempt, not a guess.

## R13: fresh eyes (2026-11-08)

One agent that had never seen the code built a small turn-based game with only `documents/` and the packed
`Evil.Engine` 0.2.0: an 8x8 tiled board, two teams of two, click a unit of the side whose turn it is, click a tile
to walk there (a path around other units, animated step by step), Space ends the turn, a HUD line, Escape quits.

**Result: it works and meets every bullet**, in 5 builds (one failed), with no blocker and nothing read from the
source. 13 findings: 3 major, 9 minor, 1 its own mistake. Its game is kept in
`/mnt/hdd/projects/MyGameFrameWork-data/r13-fresh-eyes/` for the board recipe later.

![[r13-fresh-eyes.png]]

| # | Finding | Kind | Severity | Fix | Status |
|---|---|---|---|---|---|
| 1 | No note says how to take a screenshot: `CaptureFrame` and `CaptureAndExit` were only findable by reflection | docs missing | major | Capture section in [[Screen]] | fixed in R13 |
| 2 | Scripting input against `godot.sh` meant reading `ps` for the display; the 300 s limit was never mentioned | docs missing | major | `godot.sh` already uses an existing `DISPLAY`: the recipe and the limit are in [[Testing]] | fixed in R13 |
| 3 | A healthy run prints nothing, so "works" and "hung" look the same | wish | minor | one engine line when the game starts | R17 |
| 4 | The usings list missed `System.Collections.Generic`: the first real build failed on `List<>` | docs wrong | minor | `GlobalUsings.cs` is now a code block, and the Make a Game check builds it as written | fixed in R13 |
| 5 | The style rules are only enforced inside the engine repo; a new game has no `.editorconfig` | docs misleading | major | the note says so now; the template ships the checks | note R13, template R19 |
| 6 | Deriving from `WorldObject` for game objects works but is never said | docs missing | minor | "Your own objects" in [[World]] | fixed in R13 |
| 7 | `MoveAlong` does not say the path leaves out where the object stands | docs missing | minor | said in [[Motion]] | fixed in R13 |
| 8 | `Torus(inner, outer)` arguments and that it lies flat were guessed | docs missing | minor | said in [[World]] | fixed in R13 |
| 9 | Lit colours come out lighter than the colour given | docs missing | minor | [[World]] points to `Unshaded` for exact flat colours | fixed in R13 |
| 10 | No board helpers: about 100 lines for cells, cell to world, tiles, paths; `PointOnGround` was not linked to "tile under the mouse" | wish | minor | recipe in [[Input]] now; a `Grid` type | recipe R13, `Grid` R17 |
| 11 | Every game repeats five setup files; `.Content("res://content")` names a folder a new game does not have | boilerplate | minor | the folder note in [[Make a Game]]; `dotnet new evil-game` | note R13, template R19 |
| 12 | `scripts/godot.sh` is a path inside the engine repo, which a game folder does not have | docs wrong | minor | full path in [[Make a Game]] | fixed in R13 |
| 13 | A field initializer cannot use other fields | own mistake | - | plain C# | - |

Found while checking the findings: `Ancestor<T>()` starts at the parent, so `hit.Target.Ancestor<Unit>()` misses
when the clicked object is the `Unit` itself (a `class Unit : Capsule`). The demo loops by hand for that reason.
Fix in R17.

The three changes it said would have helped most: document capture and scripted input on the server (1, 2, 3, 12),
ship the style checks with the package (5), and make Make a Game complete plus a board recipe (4, 6 to 10).
