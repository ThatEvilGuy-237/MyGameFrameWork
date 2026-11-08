---
tags: [api, audio]
aliases: [Sound, Music, AudioMixer, Volume]
---

# Audio

Library `Evil.Engine.Audio` (`src/09_Audio`). `Game.Audio` plays sounds and music. `Game.Content.Sound(path)` loads
them (see [[Content]]).

```csharp
Sound click = Game.Content.Sound("sounds/click.wav");
Game.Audio.SetVolume(AudioGroup.Music, 0.35f);
Game.Audio.PlayMusic(Game.Content.Sound("sounds/music.wav"), 2f);

Game.Audio.Play(click);
Game.Audio.PlayAt(hop, pawn);
```

## Playing

| Call | Does |
|---|---|
| `Play(sound, group = Effects, volume = 1, pitch = 1)` | plays once, everywhere at the same loudness; frees itself when done |
| `PlayAt(sound, worldObject, group, volume)` | plays from an object in the 3D world and follows it as it moves; louder when the camera is close |
| `PlayMusic(sound, fadeSeconds = 1)` | loops on the Music group; a new track crossfades with the old one; asking for the playing track changes nothing |
| `StopMusic(fadeSeconds = 1)` | fades the music out |
| `StopAll()` | silence now: music and one-shots |

`Play` and `PlayAt` return a `SoundPlayback`: `IsPlaying`, `Stop()`. A sound attached to an object that is destroyed
stops with it.

## Volume groups

Three groups, each its own Godot audio bus into the master: `AudioGroup.Music`, `Effects`, `Ambience`.

| Call | What |
|---|---|
| `SetVolume(group, 0..1)`, `GetVolume(group)` | loudness of a group (a settings screen) |
| `Mute(group, true)`, `IsMuted(group)` | silence a group, for example the music |
| `MasterVolume` | everything at once |

## Recording what is heard

`StartRecording()` and `SaveRecording(path)` capture the final mix as a `.wav`. The driven check uses this to prove
the game makes sound on a server without speakers: 22.7 seconds recorded, level 0.044, loudest 0.88 (silence
would fail the check).

> [!note] Clean shutdown
> When the game quits, the engine stops all audio first and quits three frames later. Stopping a sound in Godot
> only takes effect on the audio thread's next mix. Quitting at once left the music and its playback behind in 4 of
> 6 runs; with the wait it was 6 of 6 clean. `check.sh` fails if any demo run leaves Godot objects behind.

## The demo's sounds

`content/sounds/make_sounds.py` (plain Python, no packages) writes `click.wav`, `hop.wav`, `hit.wav` and a 4-second
`music.wav` loop. In the demo, the music plays quietly, M mutes it, buttons click, the attacker's hop sounds from the
attacker, and the hit sounds from the enemy.
