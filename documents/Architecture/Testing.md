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
    using var stage = Stage.Open();
    var arm = stage.Root.Add(new WorldObject("arm"));
    arm.Position = new Vector3(1f, 0f, 0f);
    arm.RotateY(90f);
    var hand = arm.Add(new WorldObject("hand"));
    hand.Position = new Vector3(2f, 0f, 0f);
    Check.Near(new Vector3(1f, 0f, -2f), hand.WorldPosition);
}
```

## The gate

`scripts/check.sh`: build with warnings as errors, the pure suite, the Godot suite (fails on any Godot leak at
exit), the OrbitDemo rendered under xvfb with frame 90 saved to `documents/screenshots/orbit.png`, then the demo
driven with real keys and mouse (`scripts/drive-orbit.sh`, see [[Input]]) saved to `orbit-driven.png`. Open the screenshot: a green run with a
wrong picture is not done.
