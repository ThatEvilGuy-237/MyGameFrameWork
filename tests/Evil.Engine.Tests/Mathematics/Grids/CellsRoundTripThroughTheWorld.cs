using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Grids;

internal sealed class CellsRoundTripThroughTheWorld : Property<GridCell>
{
    protected override GridCell Generate(Random random)
    {
        Grid grid = RandomGrids.Create(random);
        return new GridCell(grid, RandomGrids.CellOf(random, grid));
    }

    protected override void Verify(GridCell sample)
    {
        Check.Equal(sample.Cell, sample.Grid.CellAt(sample.Grid.CenterOf(sample.Cell)), "the center is in its own cell");
        Check.Near(sample.Grid.Corner.Y, sample.Grid.CenterOf(sample.Cell).Y, "on the board's height");
    }
}
