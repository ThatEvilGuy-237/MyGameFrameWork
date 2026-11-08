# MyGameFrameWork: the Evil Engine

Our own 2D game engine SDK in C# (packages `Evil.Engine` and `Evil.Engine.Mathematics`), grown from the owner's
MyGameFrameWork (original code: commit `8b1fb3f`). It is the base for the turn-based game. Read `README.md`,
then `documents/Home.md`; why things are the way they are is in `documents/Decisions.md`.

## Rules

- `scripts/check.sh` is canonical: Release build with warnings as errors, the test runner, a still frame and a
  frame driven with real keys and mouse under xvfb. A change is done when it is green and the screenshots in
  `documents/screenshots/` look right (open them, do not trust the exit code alone).
- Every engine change gets a test in `tests/Evil.Engine.Tests` (a static method with `[Test]`, `Check.*`).
  The runner has no packages because xunit is still `review` in `evil-packages`; move to xunit only after the
  owner approves it.
- 2D only for now (owner, 2026-11-08). Math that game rules may need goes in `Evil.Engine.Mathematics`, which
  never references OpenTK or OpenGL. Games never see OpenTK types: wrap them (see `Key`, `MouseButton`).
- One folder is one namespace. GL objects stay `internal`. The builder takes values or settings records,
  never lambdas; the engine reads no config and no environment variable. Fail early with the call's name.
- Packages only from the checked list (`evil-packages one nuget <name> <version> --save`), exact versions in
  `Directory.Packages.props`, lock files committed. .NET 10 for everything.
- Release: bump `<Version>` in `Directory.Build.props` (both packages share it), commit, `./scripts/pack.sh`
  (feed `/mnt/hdd/projects/MyGameFrameWork-data/feed`), push. A version in the feed never changes.
- No code comments. No real names in code, tests or data: the handle is Thatevilguy. Short commit subjects,
  the user's git identity only, no trailers. No GitHub Actions.
