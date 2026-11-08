---
tags: [architecture, code-style]
aliases: [Clean code, Code style, OOP]
---

# Clean Code

Owner, 2026-11-08: "more like C#, less like JavaScript style coding, aka object oriented, and preferably no
duplicate code; a file needs meaning, not just code here or there, and folders need meaning."

## The rules

| Rule | Not this | But this |
|---|---|---|
| behaviour in classes, varied by type | `switch (action) { case "Move": ... case "Attack": ... }` | `abstract class PawnAction` with `MoveAction`, `AttackAction` |
| named methods, not anonymous functions | `queue.Any(tween => !tween.IsDone)` | a loop, or `queue.Exists(IsRunning)` with `IsRunning` a named method |
| named types, not tuples | `(Easing: e, T: 0.5f)`, `(int Kind, Vector2 Value)[]` | `record EasingSample(Easing Easing, float Progress)` |
| no delegates handed around | `Func<Random, T> generate, Action<T> check` | an abstract `Property<TCase>` with `Generate` and `Verify` |
| no duplicate code | the same 8 lines in Keyboard and Mouse | one `ButtonStates<T>` both use |
| one type per file | `ObjectTweens.cs` with 8 classes | `MoveTween.cs`, `HopTween.cs`, ... in `Tweens/` |
| folders with meaning | 25 files side by side in `Evil.Engine.World` | `Objects/`, `Shapes/`, `Materials/`, `Models/` |

Explicit types, constructor then statements and no comments still hold (see the repo's `CLAUDE.md`).
Expression-bodied members (`public float Area => Width * Height;`) and switch expressions over enums are C#, not
lambdas, and stay.

## The check

`scripts/clean-code-check.py` runs in `check.sh` before the build. It counts, per file: anonymous functions, LINQ
calls, tuples used as data, `switch` on strings or numbers, `Func`/`Action`, extra types in a file, and duplicated
code (5 identical lines, or 8 lines with the same shape and other names; enum lists are skipped).

It is a **ratchet**: `scripts/clean-code-baseline.json` holds what each file had when R16 started, keyed by project and file name so a file can move to another folder of its library. A file may never
get worse; `--write-baseline` after a round locks in the progress; `--show` lists every finding. Before it scans, it
tests itself on 25 planted cases (a switch arm is not a lambda, a record's parameters are not a tuple, `MathF.Max`
is not LINQ, renamed copies are still copies, enum lists are not copies), so a broken check cannot pass broken code.

| When | Findings | Of which in `src/` |
|---|---|---|
| R16 start (baseline) | 291 | 125 (86 of them the typed-out Godot key table) |
| R16 end | 285 | 125 |
| R17 end | 272 | 112 |
