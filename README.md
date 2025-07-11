# MyGameFramework

This is a small project I made to prepare for my internship at AZTEQ.  
It helped me understand how rendering works, and I learned a lot along the way.  
I might expand on it in the future as a personal framework or testing ground.

---

## Interesting Files

Some notable parts of the project:

- [`Rect.cs`](https://github.com/ThatEvilGuy-237/MyGameFrameWork/blob/main/MyGameFrameWork/Framework/Utils/Structs/Rect.cs)  
  Custom rectangle struct for handling drawable objects.

- [`EvilUtils.cs`](https://github.com/ThatEvilGuy-237/MyGameFrameWork/blob/main/MyGameFrameWork/Framework/Utils/EvilUtils.cs)  
  Manages the render window, drawing logic, and transformation calculations.

- [`SRTVectorsStack.cs`](https://github.com/ThatEvilGuy-237/MyGameFrameWork/blob/main/MyGameFrameWork/Framework/Utils/SRTVectorsStack.cs)  
  Handles stacking of Scale, Rotate, and Translate (SRT) steps to simplify complex movement chains.

---

## Project Idea & Concept

This project was inspired by the [MonoGame](https://monogame.net/) framework.  
One thing I didn’t like about MonoGame is how every draw call resets the origin back to `(0, 0)`.  
In this project, I wanted to try something different: drawing objects that stay grouped and follow shared movement without having to manually update each one’s position.

### Example Code

```C#
EvilUtils.SetColor(0.0f, 1.0f, 0.0f, 100.0f);
EvilUtils.SetTexture("!#need image path", new Rect(200, 200,370 -200,370-200 ));
EvilUtils.NewPush();                         // Start group A
EvilUtils.PushTranslate(5, 5);               // Move group A
EvilUtils.PushRotate(0, 0, (r * 2));         // Rotate group A

EvilUtils.DrawRectanlge(-50f, -50f, 100f, 200f); // Object in group A

EvilUtils.NewPush();                         // Start nested group B
EvilUtils.PushRotate(0, 0, -(r * 2));        // Rotate group B in opposite direction

EvilUtils.DrawRectanlge(-50f, -50f, 100f, 200f); // Objects in group B
EvilUtils.DrawRectanlge(-50f, -50f, 100f, 200f);
EvilUtils.DrawRectanlge(-50f, -50f, 100f, 200f);

EvilUtils.PopOrgin();                        // End group B
EvilUtils.PopOrgin();                        // End group A
```

