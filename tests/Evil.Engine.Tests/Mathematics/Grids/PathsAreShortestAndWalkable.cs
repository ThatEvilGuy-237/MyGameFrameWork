using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Grids;

internal sealed class PathsAreShortestAndWalkable : Property<GridBoard>
{
    protected override GridBoard Generate(Random random) => RandomGrids.Board(random);

    protected override void Verify(GridBoard sample)
    {
        IReadOnlyList<Cell> path = sample.Grid.Path(sample.From, sample.To, sample.Blocked);
        GridDistances oracle = new GridDistances(sample);
        if (sample.From == sample.To || !oracle.Reaches(sample.To))
        {
            Check.Equal(0, path.Count, "no path to walk");
            return;
        }

        Check.Equal(oracle.StepsTo(sample.To), path.Count, "as short as the brute-force distance");
        Check.Equal(sample.To, path[^1], "ends on the goal");
        Cell at = sample.From;
        foreach (Cell next in path)
        {
            Check.Equal(1, at.StepsTo(next, sample.Grid.Moves), $"{at} to {next} is one step");
            Check.True(!sample.Blocked.Contains(next), $"{next} is not blocked");
            at = next;
        }
    }
}
