# MyGameFrameWork: the Evil Engine

Our own game engine SDK in C#. Since 0.2.0 it is a set of class libraries over **Godot 4.7.2 (.NET)** for a
full 3D game; 0.1.0 (tag `v0.1.0`) was the owner's OpenTK 2D engine. Read `README.md`, then
`documents/Home.md`. The build plan and progress live in `documents/LOOP.md` (a loop round every 30 minutes);
why things are the way they are is in `documents/Decisions.md`.

## Rules

- `scripts/check.sh` is the gate: build with warnings as errors, the pure tests, the Godot tests (inside Godot,
  headless, `tests/Evil.Engine.GodotTests`), then the OrbitDemo in Godot under xvfb with a captured frame in `documents/screenshots/`. A round is done when it is green and the
  screenshot looks right (open it, do not trust the exit code alone). Every wrapper gets Godot tests.
- Godot binary: `/mnt/hdd/tools/godot-4.7.2` (came in through `evil-repo-check intake`, SHA-512 checked).
  Run projects with `scripts/godot.sh <project> -- <args>` (compatibility renderer, 2 GB cap, xvfb).
- Numbered libraries in `src/NN_Area/Evil.Engine.Area` (00 Core to 09 Audio, the engine itself is always `99_Engine`); a library only references lower numbers. Core and
  Mathematics never reference Godot. In the others Godot types stay `internal`: games see Evil Engine types and
  `System.Numerics` only. A game's only Godot line is `Main : GameMain`.
- **Code style (owner, 2026-11-08), checked by the build and `scripts/style-check.py`:**
  - Write the real type, never `var`: `string name = "x";`, `foreach (Capsule pawn in _pawns)`,
    `using Stage stage = Stage.Open();`, `out PackedScene? scene`. The build fails on `var` (IDE0008 in
    `.editorconfig`, `EnforceCodeStyleInBuild`).
  - Build objects with their constructor, then set properties as separate statements. No initializer blocks
    (`new Box { Name = "x", Position = p }`, `new Dictionary<,> { [k] = v }`) and no target-typed `new()`:
    write `new Box(...)` with its type every time.

    ```csharp
    Capsule pawn = new Capsule(0.35f, 1.4f, team);
    pawn.Name = name;
    pawn.Tag = name;
    World.Add(pawn);
    ```
  - Allowed: constructor arguments and named arguments (`new OrbitLimits(MinDistance: 6f)`), collection
    expressions (`[a, b]`), `with` copies of records. A preset or helper that sets several properties is a small
    method with statements, not an expression-bodied initializer.
- **Clean code (owner, 2026-11-08): C#, object oriented, not JavaScript style**, checked by
  `scripts/clean-code-check.py` (a ratchet: counts per file may only go down, see `documents/Architecture/Clean Code.md`):
  - Behaviour lives in classes with one job; vary it with types (an interface, an abstract class), not with a
    `switch` on strings or numbers, not with lambdas or `Func`/`Action` handed around. Name the method instead of
    writing an anonymous function; write the loop or a named method instead of a LINQ chain.
  - Data gets a named type (a record or class), never a tuple (`(A: x, B: y)`, `(int Kind, Vector2 Value)`).
  - No duplicate code: shared logic gets one home (a base class, a shared type, one method).
  - One type per file, named after it. Folders group by meaning (`Shapes/`, `Materials/`), never "misc".
  - Static classes only for pure maths like `System.Math` (Angle, Rotations, Smoothing, Ease) and extension glue.
- The builder takes values or settings records, never lambdas; the engine reads no config and no environment
  variable. Fail early with the call's name. No code comments.
- Packages only from the checked list (`evil-packages one nuget <name> <version> --save`), exact versions in
  `Directory.Packages.props`, lock files committed; game projects pin Godot through `Godot.NET.Sdk/4.7.2`.
- Release: bump `<Version>` in `Directory.Build.props`, commit, `./scripts/pack.sh`, push. Running anything on
  the owner's Windows PC (Windows lab) needs the owner's OK first.
- No real names in code, tests or data: the handle is Thatevilguy. Short commit subjects, the user's git
  identity only, no trailers. No GitHub Actions.
