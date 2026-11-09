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

    Sphere flame = new Sphere(0.18f, Color.White);
    flame.Material = Material.Glowing(Color.FromHex("#FFB45E"), 2.5f);
    pillar.Add(flame);
    flame.Add(Lamp.Warm(7f));

    Spotlight spot = new Spotlight(Color.FromHex("#CFE3FF"), 14f, 24f);
    spot.Shadows = true;
    spot.Position = new Vector3(0f, 9f, 3.5f);
    World.Add(spot);
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
| `AmbientColor`, `AmbientStrength` | the light that reaches everywhere; strength 0 is none, above 1 is allowed |
| `AmbientFromSky` | take the ambient light from a gradient sky instead; `AmbientStrength` is then the sky's share, 0 to 1 |
| `Fog` | `new Fog(color, density)` or `null` for none |
| `Glow` | 0 is off; makes glowing materials bloom |
| `ToneMapping`, `Exposure` | `Filmic` (default: softer highlights, colours a little lighter), `Aces`, or `Linear` (colours as given); overall brightness |

Presets: `Atmosphere.Studio()`, `Daylight()`, `Dusk()`, `Night()`.

> [!warning] A bright sky is a strong light
> Godot's sky light is strong and blue. With the whole sky as ambient light under a noon sun, a green floor turned
> turquoise (hue 149° to 175°) and a blue box went light blue. `Daylight()` therefore takes a tenth of the sky
> (floor 159°, still green); `Dusk()`'s sky is dark, so it takes all of it. Measured on the [[Make a Game]] scene.

> [!note] Server pictures
> Screenshots on the server use Godot's compatibility renderer. On the PC, Godot's Forward+ renderer draws the same
> scene with nicer glow, fog and shadows.
