---
tags: [api, screen]
aliases: [Canvas, Overlay, HUD, Text, Display, Window]
---

# Screen

Library `Evil.Engine.Screen` (`src/07_Screen`). It covers the window (`Game.Display`) and the **2D overlay**:
everything drawn flat on top of the 3D world, such as panels, buttons, choice boxes and text. The overlay uses
the same canvas as the 0.1.0 engine, with your [[Grouped Transforms]] `Group()`, now on top of Godot's 2D drawing.

```csharp
protected override void Draw(Canvas canvas)
{
    var panel = new RectF(16f, 16f, 380f, 112f);
    canvas.FillRect(panel, Color.Black.WithAlpha(0.45f));
    canvas.DrawText("Selected: pawn-red", new Vector2(30f, 60f), 16f, Color.FromHex("#FFC53D"));

    using (canvas.Group())
    {
        canvas.Translate(canvas.Size.X - 70f, 70f);
        canvas.Rotate(_view.Yaw);
        canvas.FillTriangle(new Vector2(0f, -32f), new Vector2(-9f, 0f), new Vector2(9f, 0f), Color.Red);
        canvas.DrawText("N", new RectF(-10f, -52f, 20f, 16f), 13f, Color.White, TextAlign.Center);
    }

    canvas.FillRect(attackButton, Color.FromHex("#3E63DD"));
    canvas.DrawText("Attack", attackButton, 18f, Color.White, TextAlign.Center);
}
```

## Canvas

A scene draws its overlay in `protected override void Draw(Canvas canvas)`, called once per frame after
`Update`. Scenes draw bottom first. A full scene hides the 2D below it, an overlay scene draws on top, and every
scene starts from a clean transform.

| Call | Draws |
|---|---|
| `Group()`, `Push()`/`Pop()`, `Translate`, `Rotate`, `Scale`, `Apply(Transform2D)` | groups, the same as 0.1.0 |
| `FillRect(rect, color)`, `DrawRect(rect, color, thickness)` | filled and outlined rectangles |
| `DrawLine(from, to, color, thickness)`, `FillCircle(center, radius, color)` | lines and circles |
| `FillTriangle(a, b, c, color)`, `FillPolygon(polygon, color)` | triangles and any simple polygon |
| `DrawText(text, topLeft, size, color)` | one line from its top-left corner |
| `DrawText(text, box, size, color, align)` | one line centred up and down in a box, aligned left, centre or right (buttons) |
| `DrawParagraph(text, box, size, color)` | text that wraps inside the box width (dialogue, choices) |
| `MeasureText(text, size)`, `MeasureParagraph(text, size, width)` | sizes in pixels; usable at any time |

Sizes are in pixels and coordinates are screen pixels with `(0, 0)` at the top-left, the same as
`Game.ScreenSize` and `Game.Mouse.Position`. Drawing outside `Draw` throws. A forgotten `Pop()` stops the game
with the scene's name. Text uses Godot's built-in font; your own fonts come with file loading in round 8.

> [!tip] Clicks on the overlay first
> Test the mouse against your button rectangles before picking in 3D, so a click on "Attack" never also selects
> the pawn behind it. The demo does this in `Update`.

## Display (`Game.Display`)

| Member | What |
|---|---|
| `Title` | the window title |
| `Fullscreen` | on and off at run time |
| `VSync` | wait for the screen's refresh |
| `Resize(width, height)` | a new window size (leaves full screen first) |

The display drives Godot's root `Window` node. Changing the size through the lower-level `DisplayServer`
shrank the OS window, but Godot never told the game, so `Game.ScreenSize` stayed at the old size. With the root
`Window` the game sees the new size on the next frame.

> [!note] Checked on the server
> The driven check presses F10 (resize to 1024x640), waits for the demo to report `screen: 1024x640`, then clicks
> "Attack" at the button position the new layout logged. Full screen (F11) cannot be shown on the server: the
> virtual screen has no window manager to grant it. Try it on the PC.

![[r7-screen.png]]
