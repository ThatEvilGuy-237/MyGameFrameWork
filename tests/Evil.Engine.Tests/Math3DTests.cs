using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests;

internal static class Math3DTests
{
    [Test]
    public static void QuarterTurnLeftSwingsXToMinusZ()
    {
        var turn = Rotations.FromEulerDegrees(new Vector3(0f, 90f, 0f));
        Check.Near(new Vector3(0f, 0f, -1f), Rotations.Rotate(Vector3.UnitX, turn), "+X after 90 degrees around Y");
        Check.Near(new Vector3(-1f, 0f, 0f), Rotations.Rotate(Rotations.Forward, turn), "forward turns left");
    }

    [Test]
    public static void LookAlongPointsTheFront()
    {
        foreach (var direction in new[] { new Vector3(1f, 0f, 0f), new Vector3(1f, -2f, 3f), new Vector3(0f, 1f, 0f), new Vector3(0f, -5f, 0f) })
        {
            var rotation = Rotations.LookAlong(direction);
            Check.Near(Vector3.Normalize(direction), Rotations.Rotate(Rotations.Forward, rotation), $"front along {direction}");
        }

        Check.Throws<ArgumentException>(() => Rotations.LookAlong(Vector3.Zero), "no length");
    }

    [Test]
    public static void PoseChainsLikeAGroup()
    {
        var sun = Pose.Identity.TurnedY(90f);
        var planet = sun.ToWorld(Pose.At(10f, 0f, 0f));
        var moon = planet.ToWorld(Pose.At(5f, 0f, 0f));
        Check.Near(new Vector3(0f, 0f, -15f), moon.Position, "moon follows the sun's turn");
        Check.Near(new Vector3(-1f, 0f, 0f), moon.Forward, "and faces the same way");
    }

    [Test]
    public static void PoseToLocalUndoesToWorld()
    {
        var pose = new Pose(new Vector3(3f, 1f, -2f), Rotations.FromEulerDegrees(new Vector3(20f, 75f, -10f)), new Vector3(2f, 2f, 2f));
        var local = new Vector3(0.5f, -1f, 4f);
        Check.Near(local, pose.ToLocal(pose.ToWorld(local)), "round trip");
        Check.Near(pose.ToWorld(local), Vector3.Transform(local, pose.ToMatrix()), "matrix agrees");
    }

    [Test]
    public static void BoundsMeasureAndTest()
    {
        var box = Bounds3.FromCenter(new Vector3(0f, 1f, 0f), new Vector3(2f, 2f, 2f));
        Check.Near(new Vector3(-1f, 0f, -1f), box.Min, "min");
        Check.Near(new Vector3(2f, 2f, 2f), box.Size, "size");
        Check.True(box.Contains(new Vector3(0.5f, 1.5f, -0.5f)), "inside");
        Check.True(!box.Contains(new Vector3(0f, 3f, 0f)), "above");
        Check.True(box.Intersects(Bounds3.FromCenter(new Vector3(1.5f, 1f, 0f), Vector3.One)), "overlap");
        Check.Equal(new Bounds3(new Vector3(-1f, 0f, -1f), new Vector3(5f, 2f, 1f)), box.Encapsulate(new Vector3(5f, 1f, 0f)), "grow to a point");
        Check.Equal(box, Bounds3.FromPoints([new Vector3(-1f, 0f, -1f), new Vector3(1f, 2f, 1f), new Vector3(0f, 1f, 0f)]), "from points");
    }

    [Test]
    public static void RaysHitBoxesPlanesAndSpheres()
    {
        var box = Bounds3.FromCenter(Vector3.Zero, new Vector3(2f, 2f, 2f));
        var ray = new Ray3(new Vector3(-5f, 0f, 0f), new Vector3(2f, 0f, 0f));
        Check.Near(4f, ray.Intersect(box) ?? -1f, "box entry");
        Check.True(new Ray3(new Vector3(-5f, 3f, 0f), Vector3.UnitX).Intersect(box) is null, "passes above");
        Check.Near(0f, new Ray3(Vector3.Zero, Vector3.UnitY).Intersect(box) ?? -1f, "starts inside");
        Check.True(new Ray3(new Vector3(5f, 0f, 0f), Vector3.UnitX).Intersect(box) is null, "points away");

        var down = new Ray3(new Vector3(2f, 10f, 3f), new Vector3(0f, -1f, 0f));
        Check.Near(10f, down.IntersectPlane(Vector3.Zero, Vector3.UnitY) ?? -1f, "hits the floor");
        Check.Near(new Vector3(2f, 0f, 3f), down.PointAt(10f), "where on the floor");
        Check.True(new Ray3(Vector3.UnitY, Vector3.UnitX).IntersectPlane(Vector3.Zero, Vector3.UnitY) is null, "parallel");

        Check.Near(4f, new Ray3(new Vector3(0f, 0f, 5f), -Vector3.UnitZ).IntersectSphere(Vector3.Zero, 1f) ?? -1f, "sphere front");
        Check.True(new Ray3(new Vector3(0f, 2f, 5f), -Vector3.UnitZ).IntersectSphere(Vector3.Zero, 1f) is null, "sphere miss");
        Check.Throws<ArgumentException>(() => _ = new Ray3(Vector3.Zero, Vector3.Zero), "direction with a length");
    }
}
