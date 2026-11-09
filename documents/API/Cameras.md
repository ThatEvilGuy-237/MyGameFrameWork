---
tags: [api, cameras]
aliases: [Camera, OrbitCamera]
---

# Cameras

Library `Evil.Engine.Cameras` (`src/04_Cameras`). Every scene gets its own `Camera` (see [[Architecture Overview]]).
It is a `WorldObject`, so it moves, turns and looks at things like any other object (see [[World]]).

## Camera

| Member | What |
|---|---|
| `Projection` | `Perspective` (things far away look smaller) or `Orthographic` (no shrinking, a board-game look) |
| `FieldOfView` | perspective: vertical view angle in degrees, 75 to start with |
| `OrthographicSize` | orthographic: how many world units fit top to bottom |
| `Near`, `Far` | the closest and furthest distance that gets drawn, 0.05 and 4000 to start with |
| `Frame(bounds, pitch = 50, yaw = 0)` | moves and turns the camera so the whole box is in view, looking down at `pitch` degrees from `yaw`; a board frames itself with `Camera.Frame(grid.Bounds)` |
| `ScreenToRay(screenPoint)` | the `Ray3` that goes from the camera through a pixel (see [[Mathematics]]) |
| `PointOnGround(screenPoint, height = 0)` | where that ray hits a flat floor; `null` if it never does |
| `WorldToScreen(point)` | which pixel a world point lands on |
| `CanSee(point)` | the point is in front of the camera and inside the view |

Screen points are pixels with `(0, 0)` at the top-left, the same space as `Game.ScreenSize`. The screen calls
need the camera in a scene's world and say so if it is not.

## OrbitCamera

A tactics view: the camera circles a target point, tilted down, at a distance. It drives a camera; it is not a
camera itself.

```csharp
_view = new OrbitCamera(Camera, new OrbitLimits(MinDistance: 6f, MaxDistance: 30f));
_view.Target = new Vector3(0f, 0f, 2f);
_view.Distance = 13f;
_view.Pitch = 32f;
_view.Snap();

protected override void Update(GameTime time)
{
    _view.Turn(12f * time.DeltaSeconds);
    _view.Update(time);
}
```

| Member | What |
|---|---|
| `Target`, `Yaw`, `Pitch`, `Distance` | where it looks, its angle around and down, how far away |
| `Turn(degrees)`, `Tilt(degrees)`, `Zoom(factor)`, `Pan(amount)` | change those; `Pan` moves along the ground the way the camera faces |
| `Update(time)` | glide towards the new values; call once per frame |
| `Sharpness` | how fast it glides (10 by default; 0 jumps at once) |
| `Snap()` | jump straight there |
| `Limits` | an `OrbitLimits(MinDistance, MaxDistance, MinPitch, MaxPitch)` settings record; values are kept inside it |

The gliding gives the same result at any frame rate. The maths is `Orbit.PoseFor` and `Smoothing.Toward` in
[[Mathematics]], tested without Godot.

> [!tip] Why limits are a settings record
> With separate `MinPitch`/`Pitch` properties, `{ Pitch = 0, MinPitch = 0 }` clamped the pitch to the old
> minimum before the new one arrived, so the order you wrote them in changed the result. A record passed first
> cannot do that.
