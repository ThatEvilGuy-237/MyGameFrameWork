---
tags: [architecture, graphics]
aliases: [Batching, Shaders]
---

# Rendering

## The path of a rectangle

1. `canvas.FillRect(rect, color)` writes six vertices (two triangles) into a scratch buffer.
2. The canvas moves them through the current group matrix on the CPU ([[Grouped Transforms]]).
3. `BatchRenderer.Submit` copies them into one big vertex array (12,288 vertices).
4. The batch is drawn (**flushed**) when the texture changes, the array is full, or the frame ends: one
   `glDrawArrays` for everything since the last flush.

So a frame of plain shapes is **one draw call**. The old engine made, filled and deleted a GPU buffer for every
shape. `Game.LastFrame` tells you `DrawCalls` and `Triangles`.

## One shader, one white texture

| Piece | What it does |
|---|---|
| `sprite.vert.glsl` | pixels to screen: `x / width * 2 - 1`, `1 - y / height * 2` (top-left is `(0, 0)`) |
| `sprite.frag.glsl` | `texture(u_texture, uv) * color` |
| white 1x1 texture | plain shapes sample it, so colour times white is the colour |

Every draw is "texture times colour", so plain shapes and sprites share one shader and one batch. The old
fragment shader tried to fall back to the colour when no texture was bound. An unbound texture reads as black
in OpenGL, so that fallback could not work.

The shaders are **embedded** in `Evil.Engine.dll`. The old engine read them from `Framework\ShadersGLSL\...`
three folders above `bin/`. That broke on Linux and in any published build.

## Vertex layout

| Field | Type | Offset |
|---|---|---|
| position | 2 floats | 0 |
| texture coordinate | 2 floats | 8 |
| colour | 4 floats | 16 |

32 bytes per vertex (`CanvasTests.VertexMatchesTheShaderLayout` checks it).

## Window and context

OpenGL 3.3 core, forward compatible (works on Windows, Linux and macOS). Blending is on (straight alpha). The
viewport follows the framebuffer, and drawing uses window coordinates, so resizing and Windows display scaling
keep working. The old engine kept drawing at the starting size.

> [!note] Draw order
> Later draws land on top. There is no depth buffer: a 2D game decides the order by drawing back to front.
