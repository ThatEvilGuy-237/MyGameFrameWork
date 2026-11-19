---
tags: [api, characters]
aliases: [CharacterModel, Clothing, Clips]
---

# Characters

Library `Evil.Engine.Characters` (`src/10_Characters`). A character is designed in Blender: its body, rig, clothing,
colours and animations. The engine loads what was designed and uses it. It never shapes, colours or
animates a character itself.

```csharp
CharacterModel detective = World.Add(Game.Content.Character("characters/young-man"));
detective.WearOutfit("detective");
detective.Scale = new Vector3(0.5f, 0.5f, 0.5f);
detective.Clips.Play("guard-enter");
detective.Clips.Then("guard");
```

![[r27-characters.png]]

Seven looks from four packages, three stances from the clips (guarding, standing, sheltered). Every colour is the
package's own.

> [!note] Only what Blender made
> The owner: "I just want the original models we made in Blender load in, that's all. Not any extra effects."
> A character is drawn in the plain colours and pictures of its package. No light changes it, and since 0.4.2 no
> code can either: `Wash`, which faded a character toward a colour, is gone. A game that wants a character to look
> different asks for it in Blender.
>
> No shading and no outline yet (owner: "just load model, shading is for later"). The package already carries what
> shading needs (a shade colour per surface, the rim, the outline's colour and width), and the engine reads and
> checks it. The shading that 0.4.0 had (two tones, an outline, a light of its own for characters) is at tag
> `v0.4.0`.

## CharacterModel

A `WorldObject` like any other: `Position`, `Rotation`, `Scale`, `Visible`, `Destroy()`. It faces `-Z`, as
everything in the engine does, whichever way it faced in Blender. Each `Game.Content.Character(...)` call gives a
character of its own; two from one package share the files, not the clothing or the clip.

| Member | What |
|---|---|
| `Height` | how tall it stands in metres at scale 1, to size it for the game |
| `Clothing`, `Outfits` | the names its package brings |
| `Wear(piece)` | puts one piece on. A piece has a slot (`head`, `outer`, `feet`, ...): a second piece for the same slot takes the first one off |
| `WearOutfit(outfit)` | puts on every piece of a named outfit |
| `TakeOff(piece)`, `Worn` | takes a piece off; what it wears now |
| `Clips` | the animations on its rig, see below |
| `Source` | where it was loaded from |

Clothing is not a paint job and not part of the body. Each piece is its own file, skinned to the same bones, and
`Wear` hangs it on the character's own rig, so it moves with every clip. A piece also says which skin it covers
and which shapes it needs (a hat presses the hair): the engine hides that skin and sets those shapes while the
piece is worn, and undoes both when it comes off.

## Clips

| Member | What |
|---|---|
| `Names`, `Has(name)` | the clips of this character |
| `Play(name)` | crosses over to a clip and drops anything that was waiting. A clip loops when its package says so; otherwise it stops on its last pose |
| `Then(name)` | plays this clip when the current one ends: `Play("guard-enter")`, `Then("guard")` |
| `Current` | the clip that plays, `null` when none does |
| `Seconds(name)`, `Loops(name)` | what the package says about a clip |
| `Blend` | seconds to cross from one clip into the next, 0.25 at first |

A clip moves the bones and the shapes of the face. It also names the face to show at each moment (a blink, a
frown): the engine swaps the eye and face pictures at those moments.

## The package

`Game.Content.Character("characters/young-man")` names a folder:

```
young-man/
  character.json     what Blender says about this character
  body.glb           rig, skin, head, hair, eyes, and the clips
  clothing/*.glb     one file per piece, on the same bones
  pictures/*.png     what is painted on face, eyes, skin and cloth
```

`character.json` is written by the export step, never by hand. It lists, per mesh, each surface's colours, its
inside colour and its painted pictures; per piece its slot, the skin it hides and the shapes it sets;
the outfits; the clips with their faces; the outline's colour and width. A file in a format this engine does not
read, a part the model file does not have, a bone the rig does not have: each is refused when the character is
loaded, with the name of what is wrong.

The packages of the turn-based game are made by `character/game-export.sh` in the character design research
project, from the Blender files of the saved models.

## What Godot does not do, and how the engine does it

| Godot has no | The engine |
|---|---|
| clothing on another file's rig | binds each piece to the character's bones by name |
| skin hidden under clothing | every skin triangle carries a patch number; the engine draws the skin without the patches the worn pieces cover |
| material for a colour with painted layers | a small shader of its own: one colour, an inside colour, two painted layers with their weights; no light in it |
| face pictures that follow an animation | reads the clip's faces and swaps the pictures |

A pixel of a surface is the colour its package names, whatever the scene's light, fog or time of day. Measured on
the strongman in the picture above: the leotard is `#BF3438` on screen and `#BE3438` in Blender's own picture of
him, the skin `#F9E4D4` in both. A scene that changes the whole picture afterwards (a tone mapping other than
`Linear`, glow, fog) still changes a character with it, so a scene that must show the colours as made uses none of
those. The colours come out the same on Godot's three renderers (Forward+, Mobile, Compatibility); compared side by
side. A character still throws its shadow on the ground in the scene's `Sun`. Parts that exist only to throw a
shadow onto the character itself are not drawn.

Not built: shading and the outline, effects on a character, showing an expression on demand (the packages carry them, only clips use
them), physics on hair or cloth, weapons in the hand.
