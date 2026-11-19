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
    RectF panel = new RectF(16f, 16f, 480f, 112f);
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
with the scene's name. Text uses Godot's built-in font unless you set `canvas.Font` to a font from `Game.Content.Font(path)` (see
[[Content]]); it resets to the built-in font for every scene and every frame. `MeasureText` takes an optional font.

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

### A game laid out for one size

By default the overlay is in window pixels and `Game.ScreenSize` follows the window: a game that lays its screen
out from `canvas.Size` fits any window. A game whose screen is laid out for one size asks for the picture to be
scaled instead:

```csharp
GameBuilder.Create(new WindowSettings("Turn-Based Game", 1280, 720, ScalesWithWindow: true))
```

Then `Game.ScreenSize` stays 1280 by 720 whatever the window does. The overlay is drawn at that size and scaled
to the window, the 3D world is drawn at the window's own sharpness, and a window of another shape gets bars at
its sides. Mouse positions, `Camera.WorldToScreen` and `Camera.ScreenToRay` are all in the same 1280 by 720, so
no game code changes. The turn-based game's screen was built for 1280 by 720 and fell apart in a maximised
window until it asked for this.

### Room beside the picture

`Game.Display.SideRoom = 320f` keeps a strip of that width free at the right of the window, for a panel of
tools. The game's picture moves over and gets smaller to make the room: `Game.ScreenSize` is still 1280 by 720,
every scene's camera still looks at the middle of that, clicks and `WorldToScreen` still agree, and the overlay
may draw the panel from `canvas.Size.X` to `canvas.Size.X + SideRoom`. Set it back to 0 and the picture fills
the window again. It needs a window that scales (`ScalesWithWindow`); a camera a game adds itself is not moved.

## Graphics (`Game.Graphics`)

How smooth the 3D world is drawn. A game gives its first choice to the builder and may change it while it runs:

```csharp
GameBuilder.Create(window).Graphics(new GraphicsSettings(EdgeSmoothing.X4, PictureSmoothing.Off, 1.5f))
Game.Graphics.Edges = EdgeSmoothing.X8;
```

| Member | What |
|---|---|
| `Edges` | smooths the outlines of shapes with more samples per pixel: `Off`, `X2`, `X4`, `X8`. The classic cure for stair-steps |
| `Picture` | smooths the finished picture: `Off`, `Fxaa` (cheap, softens everything a little), `Smaa` (keeps more detail) |
| `RenderScale` | draws the world at this share of the window's size and scales it to fit: 1 as it is, 1.5 and 2 bigger and scaled down, which smooths edges and fine drawn lines alike; below 1 is faster and coarser. From 0.25 to 2 |

All off and a scale of 1 unless the game says otherwise. The overlay is not touched by any of them.

![[r30-smoothing.png]]

Three characters about 85 pixels tall, enlarged three times, with nine settings (edges, picture, scale).
Without smoothing the outlines step and thin stripes break up; `X4` cleans the outlines; a scale of 1.5 or 2
also cleans what is drawn on the cloth; `Fxaa` blurs faces at this size.

## Screenshots

| Call | What |
|---|---|
| `Game.CaptureFrame(path)` | saves the next drawn frame as a `.png`, 3D world and overlay together |
| `GameBuilder.CaptureAndExit(path, frame)` | saves frame number `frame` and quits: a picture of a game nobody plays |

`CaptureAndExit` takes one picture. For pictures along the way, call `CaptureFrame` from your own key (the demo saves
on F12) or capture the window from outside: see "Driving a game on the server" in [[Testing]].

> [!note] Checked on the server
> The driven check presses F10 (resize to 1024x640), waits for the demo to report `screen: 1024x640`, then clicks
> "Attack" at the button position the new layout logged. Full screen (F11) cannot be shown on the server: the
> virtual screen has no window manager to grant it. Try it on the PC.

![[r7-screen.png]]
