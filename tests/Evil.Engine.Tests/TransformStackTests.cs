using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests;

internal static class TransformStackTests
{
    [Test]
    public static void TranslateThenRotateTurnsAroundTheNewOrigin()
    {
        TransformStack stack = new TransformStack();
        stack.Translate(100f, 100f);
        stack.Rotate(90f);
        Check.Near(new Vector2(100f, 110f), stack.ToWorld(new Vector2(10f, 0f)));
    }

    [Test]
    public static void ThirdLevelFollowsEveryParentRotation()
    {
        TransformStack stack = new TransformStack();
        stack.Rotate(90f);
        stack.Push();
        stack.Translate(100f, 0f);
        stack.Push();
        stack.Translate(50f, 0f);
        Check.Near(new Vector2(0f, 150f), stack.ToWorld(Vector2.Zero), "old MyGameFrameWork put this at (100, 50)");
    }

    [Test]
    public static void ParentScaleMovesChildren()
    {
        TransformStack stack = new TransformStack();
        stack.Scale(2f);
        stack.Push();
        stack.Translate(10f, 0f);
        Check.Near(new Vector2(20f, 0f), stack.ToWorld(Vector2.Zero), "child offset doubles with the parent");
        Check.Near(new Vector2(22f, 0f), stack.ToWorld(new Vector2(1f, 0f)), "child size doubles too");
    }

    [Test]
    public static void TwoLevelsMatchTheOriginalDemo()
    {
        TransformStack stack = new TransformStack();
        stack.Push();
        stack.Translate(400f, 400f);
        stack.Push();
        stack.Translate(0f, 100f);
        stack.Rotate(-30f);
        stack.Push();
        stack.Translate(0f, 150f);
        stack.Rotate(30f);
        Check.Near(new Vector2(475f, 629.904f), stack.ToWorld(Vector2.Zero), "yellow orbits blue");
        Check.Near(new Vector2(485f, 629.904f), stack.ToWorld(new Vector2(10f, 0f)), "yellow's own turn cancels blue's");
    }

    [Test]
    public static void PopRestoresTheParent()
    {
        TransformStack stack = new TransformStack();
        stack.Translate(5f, 5f);
        stack.Push();
        stack.Rotate(45f);
        stack.Translate(100f, 0f);
        stack.Pop();
        Check.Near(new Vector2(5f, 5f), stack.ToWorld(Vector2.Zero), "back to the parent");
        Check.Equal(0, stack.Depth, "depth");
    }

    [Test]
    public static void PopAtTheRootThrows()
    {
        TransformStack stack = new TransformStack();
        Check.Throws<InvalidOperationException>(stack.Pop, "Pop() without a matching Push()");
    }

    [Test]
    public static void GroupPopsWhenDisposed()
    {
        TransformStack stack = new TransformStack();
        using (stack.Group())
        {
            stack.Translate(50f, 50f);
            Check.Equal(1, stack.Depth, "depth inside the group");
        }

        Check.Equal(0, stack.Depth, "depth after the group");
        Check.True(stack.Current.IsIdentity, "transform is back to identity");
    }

    [Test]
    public static void ToLocalUndoesToWorld()
    {
        TransformStack stack = new TransformStack();
        stack.Translate(300f, 200f);
        stack.Rotate(33f);
        stack.Scale(1.5f, 0.5f);
        Vector2 local = new Vector2(12f, -7f);
        Check.Near(local, stack.ToLocal(stack.ToWorld(local)), "round trip");
    }
}
