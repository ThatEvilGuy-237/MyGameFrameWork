using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed class TheWindingOnlyFlipsTheSign : Property<Vector2[]>
{
    protected override Vector2[] Generate(Random random) =>
        Gen.StarShape(random);

    protected override void Verify(Vector2[] sample)
    {
        Vector2[] backwards = new Vector2[sample.Length];
        for (int i = 0; i < sample.Length; i++)
        {
            backwards[i] = sample[sample.Length - 1 - i];
        }

        Polygon shape = new Polygon(sample);
        Polygon flipped = new Polygon(backwards);
        Check.Near(-shape.SignedArea, flipped.SignedArea, "signed area", 1e-3f * shape.Area + 1e-3f);
        Check.Near(shape.Centroid, flipped.Centroid, "centroid", 1e-2f);
    }
}
