---
tags: [api, scenes]
aliases: [Scene, SceneStack]
---

# Scenes

Namespace `Evil.Engine.Scenes`. A scene is one screen of the game: the map, a battle, a menu, a pause overlay.
Scenes sit on a stack. Only the **top** one updates. Drawing starts at the lowest scene that is not an overlay.

```csharp
internal sealed class BattleScene : Scene
{
    protected override void Enter() { }
    protected override void Update(GameTime time) { }
    protected override void Draw(Canvas canvas) { }
    protected override void Exit() { }
}
```

| Hook | Runs when |
|---|---|
| `Enter()` | the scene goes on the stack; `Game` is ready from here (load textures here) |
| `Update(GameTime)` | every update while it is on top |
| `Draw(Canvas)` | every frame while it is visible |
| `Pause()` | another scene is pushed on top of it |
| `Resume()` | the scene above it is popped |
| `Exit()` | it leaves the stack, or the game ends |

`IsOverlay => true` keeps the scenes below drawing (a dimmed pause screen, a dialog box over the map).

## Changing scenes

| Call | Does |
|---|---|
| `Game.Scenes.Push(scene)` | put a scene on top (map to battle) |
| `Game.Scenes.Pop()` | remove the top one (battle back to map) |
| `Game.Scenes.Switch(scene)` | replace the top one (menu to map) |

Changes wait until the update has finished, so a scene never disappears halfway through its own `Update`.
Every scene starts drawing from a clean transform: a camera move in the map never shifts the overlay above it.

> [!warning] Loud on mistakes
> A scene that leaves a `Push()` without `Pop()` stops the game with its name:
> `BattleScene.Draw left 1 Push() without a matching Pop()`. Pushing a scene that is already on the stack, or
> popping an empty stack, throws too.

For the turn-based game: the map is one scene, a fight is pushed on top and popped when it ends, and a choice
box is an overlay.
