---
tags: [api, lighting]
aliases: [Lights, Sun, Lamp, Spotlight, Atmosphere, Sky]
---

# Lighting

Library `Evil.Engine.Lighting` (`src/05_Lighting`). Lights are `WorldObject`s (see [[World]]), so a lamp hung
inside a torch follows the torch. The sky, ambient light and haze of a scene are its `Atmosphere`.

```csharp
protected override void Enter()
{
    Atmosphere = Atmosphere.Dusk();
    World.Add(Sun.Golden());

    var flame = pillar.Add(new Sphere(0.18f, Color.White) { Material = Material.Glowing(Color.FromHex("#FFB45E"), 2.5f) });
    flame.Add(Lamp.Warm(7f));

    var spot = World.Add(new Spotlight(Color.FromHex("#CFE3FF"), 14f, 24f) { Shadows = true });
    spot.Position = new Vector3(0f, 9f, 3.5f);
}
```

## Lights

Every light has `Color`, `Intensity` (never below 0) and `Shadows`.

| Light | What | Own settings |
|---|---|---|
| `Sun` | light from far away, the same everywhere; shadows on by default | `Direction`, `ShadowDistance`; presets `Sun.Noon()`, `Sun.Golden()`, `Sun.Moonlight()` |
| `Lamp` | a point that shines all around (torch, candle, glowing gem) | `Range`, `Falloff`; presets `Lamp.Warm(range)`, `Lamp.Cool(range)` |
| `Spotlight` | a cone of light; points down by default | `Direction`, `Range`, `Angle` (middle to edge, 0 to 180), `Softness` |

A bad spotlight angle is refused before anything is made, so nothing is left behind.

## Atmosphere

Each scene has its own, shown through its own camera: switching scenes switches the sky. A new scene starts with
`new Atmosphere()`: the game's clear colour as background and a soft ambient light, so the side of an object
away from the sun is never pitch black.

| Member | What |
|---|---|
| `Background` | `Background.ClearColor`, `Background.Solid(color)` or `Background.Gradient(top, horizon, ground)` |
| `AmbientColor`, `AmbientStrength` | the light that reaches everywhere |
| `AmbientFromSky` | take the ambient light from a gradient sky instead |
| `Fog` | `new Fog(color, density)` or `null` for none |
| `Glow` | 0 is off; makes glowing materials bloom |
| `ToneMapping`, `Exposure` | `Linear`, `Filmic` (default) or `Aces`; overall brightness |

Presets: `Atmosphere.Studio()`, `Daylight()`, `Dusk()`, `Night()`.

> [!note] Server pictures
> Screenshots on the server use Godot's compatibility renderer. On the PC, Godot's Forward+ renderer draws the same
> scene with nicer glow, fog and shadows.
