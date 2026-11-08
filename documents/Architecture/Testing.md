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
ends. `stage.AddCamera(position)` adds a current camera looking at the origin and `Stage.ScreenMiddle` is the pixel
under it, so no test builds its own camera. The Godot suite sits in folders by library (`WorldObjects`, `Cameras`,
`Lighting`, `Input`, `Screen`, `Motion`, `Audio`, `Content`, `Hosting`, `Scenes`, `Maths`), and each helper is its
own type: `RecordingTarget` (a canvas target that records what was drawn as `DrawnText`), `MapAndBattle` (two
scenes on a stack with their hooks logged), `EmptyScene`, `ProbeScene`, `SkyScene`.

A test that expects a call to fail names it: `void ZoomToNothing() => orbit.Zoom(0f);` then
`Check.Throws<ArgumentOutOfRangeException>(ZoomToNothing, "factor above 0");`, never an anonymous `() =>`.

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

## Property tests: proving the maths

A test with fixed numbers proves a handful of cases. The maths is also checked with **properties**: rules that must
hold for any input, tried on hundreds of seeded random inputs. A property is a class: it makes a case and checks
it, and the runner finds it by itself (`tests/Shared/PropertyOfCase.cs`, generators in `tests/Shared/Gen.cs`, no
package). The case is a named record next to it.

```csharp
internal sealed record PosePoint(Pose Pose, Vector3 Point);

internal sealed class ToLocalUndoesToWorld : Property<PosePoint>
{
    protected override PosePoint Generate(Random random) =>
        new PosePoint(Gen.Placement(random, false), Gen.Point3(random, 20f));

    protected override void Verify(PosePoint sample)
    {
        Check.Near(sample.Point, sample.Pose.ToLocal(sample.Pose.ToWorld(sample.Point)), "there and back", 1e-3f);
    }
}
```

The pure suite keeps them in folders by meaning (`Core/EaseCurves`, `Core/Colors`, `Mathematics/Angles`,
`Bounds`, `GoalSmoothing`, `Orbits`, `Polygons`, `Poses`, `Rays`, `Rectangles`, `Transforms`, `Turns`,
`Validators`). A folder never takes the name of an engine type (`Turns`, not `Rotations`): C# would let the folder's
namespace hide the type in every test next to it. Plain example tests stay `[Test]` methods; both kinds run as a
`TestCase`.

Each property gets 300 cases with a seed made from its class name, so a run is the same every time. A failure prints the
case, the seed and the input. To search harder, change the seed or the count:

```bash
EVIL_PROPERTY_SEED=4 EVIL_PROPERTY_CASES=20000 dotnet tests/Evil.Engine.Tests/bin/Debug/net10.0/Evil.Engine.Tests.dll Properties
```

Three kinds of checks, each answering "how do we know it is right" a different way:

| Kind | Example | Where |
|---|---|---|
| rules that must hold | to world and back is the same point; two half steps of smoothing are one full step; the triangles of a polygon cover its area once | `tests/Evil.Engine.Tests/Core`, `.../Mathematics` |
| brute force | walking along a ray in 1 cm steps finds the box where `Intersect` says it is; points sampled in a polygon agree with its triangles (a `Triangulation` helper that measures them) | the same folders |
| a second opinion | our Euler turns, LookAlong, poses, nested objects, bounds and ray hits against Godot's own Basis, Transform3D, Aabb, Plane and sphere code | `tests/Evil.Engine.GodotTests/MathAgainstGodotTests.cs` |

What they found in R15: a ray grazing a small sphere far away missed it (`IntersectSphere` subtracted two numbers
near 2500 to get one near 0.0004, and floats lose that; it now measures the gap at the closest point), any
object scaled to zero made every click throw (Godot cannot invert its transform; picking now skips it), and one of
our own properties was wrong (a 3-point star with fat arms is convex).

## The gate

`scripts/check.sh` runs, in order:

| Step | Fails when |
|---|---|
| code style | `scripts/style-check.py` finds `var`, an initializer block or a target-typed `new()`, in code (new files too) or in a doc's C# example |
| clean code | `scripts/clean-code-check.py` finds a file worse than its baseline: more lambdas, LINQ, tuples, value switches, delegates, types per file or duplicates (see [[Clean Code]]); it also fails if its own self-test fails |
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

