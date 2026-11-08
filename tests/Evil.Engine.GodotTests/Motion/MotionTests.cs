using Evil.Engine.Motion;

namespace Evil.Engine.GodotTests.Motion;

internal static class MotionTests
{
    [Test]
    public static void MoveToArrivesOnTime()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        Tween move = motion.MoveTo(pawn, new Vector3(4f, 0f, 0f), 1f, Easing.Linear);
        motion.Update(0.5f);
        Check.Near(new Vector3(2f, 0f, 0f), pawn.Position, "halfway after half the time");
        Check.True(!move.IsDone && motion.IsBusy(pawn), "still moving");
        motion.Update(0.5f);
        Check.Near(new Vector3(4f, 0f, 0f), pawn.Position, "there");
        Check.True(move.IsDone && move.Finished.Result && !move.WasCancelled, "finished");
        Check.True(!motion.IsBusy(pawn) && !motion.IsBusy(), "nothing left");
        pawn.Destroy();
    }

    [Test]
    public static void EasingShapesTheWay()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        motion.MoveTo(pawn, new Vector3(4f, 0f, 0f), 1f, Easing.InOutCubic);
        motion.Update(0.25f);
        Check.Near(4f * Ease.Apply(Easing.InOutCubic, 0.25f), pawn.Position.X, "slow start");
        pawn.Destroy();
    }

    [Test]
    public static void MovesOnOneObjectWaitTheirTurnAndKeepLeftoverTime()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        Tween first = motion.MoveTo(pawn, new Vector3(2f, 0f, 0f), 1f, Easing.Linear);
        Tween second = motion.MoveTo(pawn, new Vector3(2f, 0f, 2f), 1f, Easing.Linear);
        motion.Update(0.5f);
        Check.True(!second.IsStarted, "the second waits");
        motion.Update(1f);
        Check.True(first.IsDone, "first done");
        Check.Near(new Vector3(2f, 0f, 1f), pawn.Position, "the spare half second went into the second move, from where the first ended");
        motion.Update(0.5f);
        Check.True(second.IsDone, "second done");
        pawn.Destroy();
    }

    [Test]
    public static void DifferentObjectsMoveTogether()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject red = new WorldObject("pawn");
        WorldObject blue = new WorldObject("blue");
        motion.MoveTo(red, new Vector3(2f, 0f, 0f), 1f, Easing.Linear);
        motion.MoveTo(blue, new Vector3(0f, 0f, 2f), 1f, Easing.Linear);
        motion.Update(1f);
        Check.Near(new Vector3(2f, 0f, 0f), red.Position, "red");
        Check.Near(new Vector3(0f, 0f, 2f), blue.Position, "blue");
        red.Destroy();
        blue.Destroy();
    }

    [Test]
    public static void HopsArcAndLand()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        motion.Hop(pawn, new Vector3(4f, 0f, 0f), 2f, 1f);
        motion.Update(0.5f);
        Check.Near(new Vector3(2f, 2f, 0f), pawn.Position, "top of the arc");
        motion.Update(0.5f);
        Check.Near(new Vector3(4f, 0f, 0f), pawn.Position, "landed");
        pawn.Destroy();
    }

    [Test]
    public static void TurnsFacesAndSpins()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        motion.Face(pawn, new Vector3(5f, 0f, 0f), 0.2f);
        motion.Update(1f);
        Check.Near(new Vector3(1f, 0f, 0f), pawn.Forward, "faces the point");
        motion.TurnTo(pawn, 90f, 0.2f);
        motion.Update(1f);
        Check.Near(new Vector3(-1f, 0f, 0f), pawn.Forward, "yaw 90 looks along -X");
        motion.Spin(pawn, 360f, 1f, Easing.Linear);
        motion.Update(0.5f);
        Check.Near(new Vector3(1f, 0f, 0f), pawn.Forward, "half a spin faces the other way");
        motion.Update(0.5f);
        Check.Near(new Vector3(-1f, 0f, 0f), pawn.Forward, "a full spin comes back round");
        motion.Face(pawn, pawn.Position, 0.2f);
        motion.Update(1f);
        Check.Near(new Vector3(-1f, 0f, 0f), pawn.Forward, "facing its own spot changes nothing");
        pawn.Destroy();
    }

    [Test]
    public static void MoveAlongWalksThePathFacingEachStep()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        Tween last = motion.MoveAlong(pawn, [new Vector3(0f, 0f, -2f), new Vector3(2f, 0f, -2f)], 1f);
        motion.Update(1.25f);
        Check.Near(new Vector3(0f, 0f, -2f), pawn.Position, "first tile");
        Check.Near(new Vector3(0f, 0f, -1f), pawn.Forward, "facing the way it walked");
        motion.Update(0.75f);
        Check.Near(new Vector3(1f, 0f, -2f), pawn.Position, "turned and halfway to the second tile");
        Check.Near(new Vector3(1f, 0f, 0f), pawn.Forward, "facing the second step");
        motion.Update(0.5f);
        Check.True(last.IsDone, "the returned motion is the last step");
        void WalkAnEmptyPath() => motion.MoveAlong(pawn, [], 1f);
        Check.Throws<ArgumentException>(WalkAnEmptyPath, "at least one point");
        pawn.Destroy();
    }

    [Test]
    public static void StopLeavesItWhereItIsAndCompleteSkipsAhead()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        Tween move = motion.MoveTo(pawn, new Vector3(4f, 0f, 0f), 1f, Easing.Linear);
        motion.Update(0.5f);
        motion.Stop(pawn);
        motion.Update(0.5f);
        Check.Near(new Vector3(2f, 0f, 0f), pawn.Position, "stopped where it was");
        Check.True(move.WasCancelled && !move.Finished.Result, "the wait ends as cancelled");

        Tween a = motion.MoveTo(pawn, new Vector3(2f, 0f, 3f), 1f);
        Tween b = motion.MoveTo(pawn, new Vector3(5f, 0f, 3f), 1f);
        motion.Complete(pawn);
        Check.Near(new Vector3(5f, 0f, 3f), pawn.Position, "skipped to the end of everything queued");
        Check.True(a.Finished.Result && b.Finished.Result && !motion.IsBusy(pawn), "both finished");
        pawn.Destroy();
    }

    [Test]
    public static void DestroyedObjectsDropTheirMotions()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        Tween move = motion.MoveTo(pawn, new Vector3(4f, 0f, 0f), 1f);
        pawn.Destroy();
        motion.Update(0.5f);
        Check.True(move.WasCancelled && !motion.IsBusy(), "dropped without a crash");
        void MoveADestroyedPawn() => motion.MoveTo(pawn, Vector3.One, 1f);
        Check.Throws<ObjectDisposedException>(MoveADestroyedPawn);
    }

    [Test]
    public static void SpeedSlowsAndPauses()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        motion.MoveTo(pawn, new Vector3(4f, 0f, 0f), 1f, Easing.Linear);
        motion.Speed = 0f;
        motion.Update(1f);
        Check.Near(Vector3.Zero, pawn.Position, "paused");
        motion.Speed = 0.5f;
        motion.Update(1f);
        Check.Near(new Vector3(2f, 0f, 0f), pawn.Position, "half speed");
        pawn.Destroy();
    }

    [Test]
    public static void ValuesColoursAndInstantMoves()
    {
        MotionPlayer motion = new MotionPlayer();
        WorldObject pawn = new WorldObject("pawn");
        ValueTween fade = motion.Value(0f, 10f, 1f, Easing.Linear);
        Material glass = new Material(Color.Red);
        motion.ColorTo(glass, Color.Blue, 1f, Easing.Linear);
        motion.Update(0.5f);
        Check.Near(5f, fade.Current, "value halfway");
        Check.Equal(Color.Lerp(Color.Red, Color.Blue, 0.5f), glass.Color, "colour halfway");
        Tween jump = motion.MoveTo(pawn, new Vector3(9f, 0f, 0f), 0f);
        motion.Update(0f);
        Check.True(jump.IsDone, "zero seconds is instant");
        Check.Near(new Vector3(9f, 0f, 0f), pawn.Position, "and lands");
        void MoveBackInTime() => motion.MoveTo(pawn, Vector3.One, -1f);
        Check.Throws<ArgumentOutOfRangeException>(MoveBackInTime, "0 seconds or more");
        pawn.Destroy();
    }
}
