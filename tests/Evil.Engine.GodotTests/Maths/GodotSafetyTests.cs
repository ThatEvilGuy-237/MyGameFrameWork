using Evil.Engine.Input;
using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal static class GodotSafetyTests
{
    [Test]
    public static void AZeroScaledShapeCannotBeClickedAndDoesNotBreakPicking()
    {
        using Stage stage = Stage.Open();
        Camera camera = stage.AddCamera(new Vector3(0f, 0f, 10f));
        Box popping = stage.Root.Add(new Box());
        popping.Name = "popping";
        popping.Scale = Vector3.Zero;
        Box behind = stage.Root.Add(new Box());
        behind.Name = "behind";
        behind.Position = new Vector3(0f, 0f, -4f);
        Vector2 middle = Stage.ScreenMiddle;
        Hit? hit = Picker.Pick(camera, stage.Root, middle);
        Check.True(hit is not null && ReferenceEquals(behind, hit.Target), "the click goes to the box behind");
    }

    [Test]
    public static void BrokenTransformsAreRefusedWithTheObjectsName()
    {
        WorldObject pawn = new WorldObject("pawn-red");
        void MovePawnToNaN() => pawn.Position = new Vector3(float.NaN, 0f, 0f);
        Check.Throws<ArgumentException>(MovePawnToNaN, "pawn-red's Position must be finite");
        void TurnPawnForever() => pawn.Rotation = new Vector3(0f, float.PositiveInfinity, 0f);
        Check.Throws<ArgumentException>(TurnPawnForever, "pawn-red's Rotation");
        void ScalePawnByNaN() => pawn.Scale = new Vector3(1f, float.NaN, 1f);
        Check.Throws<ArgumentException>(ScalePawnByNaN, "pawn-red's Scale");
        void OrientPawnByNaN() => pawn.Orientation = new Quaternion(float.NaN, 0f, 0f, 1f);
        Check.Throws<ArgumentException>(OrientPawnByNaN, "pawn-red's Orientation");
        void OrientPawnByNothing() => pawn.Orientation = new Quaternion(0f, 0f, 0f, 0f);
        Check.Throws<ArgumentException>(OrientPawnByNothing, "no length");
        Check.Near(Vector3.Zero, pawn.Position, "a refused value changes nothing");
        pawn.Scale = Vector3.Zero;
        Check.Near(Vector3.Zero, pawn.Scale, "a zero scale is allowed: things grow from nothing");
        pawn.Destroy();
    }
}
