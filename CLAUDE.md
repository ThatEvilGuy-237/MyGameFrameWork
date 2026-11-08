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
- Numbered libraries in `src/NN_Area/Evil.Engine.Area` (00 Core to 09 Engine); a library only references lower numbers. Core and
  Mathematics never reference Godot. In the others Godot types stay `internal`: games see Evil Engine types and
  `System.Numerics` only. A game's only Godot line is `Main : GameMain`.
- The builder takes values or settings records, never lambdas; the engine reads no config and no environment
  variable. Fail early with the call's name. No code comments.
- Packages only from the checked list (`evil-packages one nuget <name> <version> --save`), exact versions in
  `Directory.Packages.props`, lock files committed; game projects pin Godot through `Godot.NET.Sdk/4.7.2`.
- Release: bump `<Version>` in `Directory.Build.props`, commit, `./scripts/pack.sh`, push. Running anything on
  the owner's Windows PC (Windows lab) needs the owner's OK first.
- No real names in code, tests or data: the handle is Thatevilguy. Short commit subjects, the user's git
  identity only, no trailers. No GitHub Actions.
