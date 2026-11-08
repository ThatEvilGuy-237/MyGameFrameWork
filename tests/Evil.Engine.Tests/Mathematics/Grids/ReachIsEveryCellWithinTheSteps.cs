using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Grids;

internal sealed class ReachIsEveryCellWithinTheSteps : Property<GridBoard>
{
    protected override GridBoard Generate(Random random) => RandomGrids.Board(random);

    protected override void Verify(GridBoard sample)
    {
        HashSet<Cell> reached = new HashSet<Cell>(sample.Grid.Reachable(sample.From, sample.Steps, sample.Blocked));
        GridDistances oracle = new GridDistances(sample);
        for (int column = 0; column < sample.Grid.Columns; column++)
        {
            for (int row = 0; row < sample.Grid.Rows; row++)
            {
                Cell cell = new Cell(column, row);
                bool within = oracle.Reaches(cell) && oracle.StepsTo(cell) <= sample.Steps;
                Check.Equal(within, reached.Contains(cell), $"{cell} within {sample.Steps} steps of {sample.From}");
            }
        }
    }
}
