---
tags: [architecture, testing]
aliases: [Tests, Check]
---

# Testing

Two test suites share one tiny runner (`tests/Shared`: `[Test]` on a static method, `Check.*` for asserts).
No test framework package, so nothing waits on the package approval list.

| Suite | Runs | Covers | Command |
|---|---|---|---|
| `tests/Evil.Engine.Tests` | plain .NET | Core and Mathematics (no Godot) | `dotnet tests/Evil.Engine.Tests/bin/Debug/net10.0/Evil.Engine.Tests.dll [filter]` |
| `tests/Evil.Engine.GodotTests` | inside Godot, headless | every wrapper against real Godot nodes: world objects, shapes, camera, sun, scene worlds, builder | `bash scripts/godot.sh --headless tests/Evil.Engine.GodotTests -- [filter]` |

The Godot suite is a small Godot project. Its main node runs every test, prints `pass` / `FAIL` lines and quits
with exit code 0 (all passed), 1 (a test failed) or 2 (the runner broke). A planted failing test was checked to
give exit code 1.

Tests that need the scene tree open a **stage**: a fresh `WorldObject` inside the tree, freed when the test
ends.

```csharp
[Test]
public static void ChildrenFollowTheParentsTurn()
{
    using Stage stage = Stage.Open();
    WorldObject arm = stage.Root.Add(new WorldObject("arm"));
    arm.Position = new Vector3(1f, 0f, 0f);
    arm.RotateY(90f);
    WorldObject hand = arm.Add(new WorldObject("hand"));
    hand.Position = new Vector3(2f, 0f, 0f);
    Check.Near(new Vector3(1f, 0f, -2f), hand.WorldPosition);
}
```

## The gate

`scripts/check.sh` runs, in order:

| Step | Fails when |
|---|---|
| code style | `scripts/style-check.py` finds `var`, an initializer block or a target-typed `new()`, in code or in a doc's C# example |
| build | any warning, including IDE0008 (`var`) |
| pure suite | a test fails |
| Godot suite | a test fails, or Godot reports a leaked object at exit |
| import | Godot cannot import the demo's content |
| frame 90 | no frame is captured, or the demo leaks at exit |
| driven run | a step of `scripts/drive-orbit.sh` is not seen in the demo's log, the recorded game audio is silent (`scripts/audio-level.py`), or the demo leaks |

The steps print what they measured: test counts, the audio level, and where the screenshots went. Open the screenshot: a green run with a
wrong picture is not done.

## Driving a game on the server

`scripts/godot.sh <project>` makes its own virtual screen with `xvfb-run`, unless `DISPLAY` is already set: then it
uses that one. So start the screen yourself and run everything inside it:

```bash
xvfb-run -a -s "-screen 0 1600x900x24" bash my-drive.sh
```

and in `my-drive.sh`, start the game in the background, find its window by title, focus it, then send input and
take pictures of the window alone (the virtual screen is bigger than the window):

```bash
bash /mnt/hdd/projects/MyGameFrameWork/scripts/godot.sh . > game.log 2>&1 &
window="$(xdotool search --name "My Game" | head -n 1)"
xdotool windowfocus "$window"
xdotool mousemove --window "$window" 640 360 click 1
import -window "$window" -depth 8 shot.png
xdotool key Escape
```

Wait for the window in a loop before using it: Godot takes a few seconds to open it. `godot.sh` stops any run after
300 seconds and caps it at 2 GB of memory. `scripts/make-a-game-check.sh` and `scripts/drive-orbit.sh` are full
examples.

## After a pack

`scripts/make-a-game-check.sh [feed]` makes a new game from the code blocks in [[Make a Game]] and nothing else,
restores `Evil.Engine` from the feed into a private package folder, builds it with warnings as errors, and runs it
under xvfb. It fails when the build breaks, when the 11 Evil.Engine libraries are not next to the game, when Escape
does not close it, on any Godot error or leak, when the floor is not green, or when "My Game" is missing from the
corner. Its first run found two bugs no other check could: Godot did not find the engine's DLLs in a packaged game,
and `Daylight()` turned the green floor turquoise ([[Decisions]] 39 and 40).

