using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed class MovingTurningAndScalingChangeTheAreaAsExpected : Property<PolygonChange>
{
    protected override PolygonChange Generate(Random random) =>
        new PolygonChange(Gen.StarShape(random), Gen.Point2(random, 30f), Gen.Number(random, -180f, 180f), Gen.Number(random, 0.2f, 4f));

    protected override void Verify(PolygonChange sample)
    {
        Polygon shape = new Polygon(sample.Corners);
        float tolerance = 1e-3f * shape.Area + 1e-3f;
        Polygon moved = shape.Translate(sample.Move);
        Check.Near(shape.Centroid + sample.Move, moved.Centroid, "the centroid moves along", 1e-2f);
        Check.Near(shape.Area, moved.Area, "moving keeps the area", tolerance);
        Check.Near(shape.Area, shape.Transform(Matrix3x2.CreateRotation(Angle.ToRadians(sample.Degrees))).Area, "turning keeps the area", tolerance);
        Polygon scaled = shape.Transform(Matrix3x2.CreateScale(sample.Scale));
        Check.Near(shape.Area * sample.Scale * sample.Scale, scaled.Area, "scaling multiplies the area", 1e-3f * scaled.Area + 1e-3f);
        Check.True(shape.CenteredOnOrigin().Centroid.Length() < 1e-2f, "centered on the origin");
    }
}
