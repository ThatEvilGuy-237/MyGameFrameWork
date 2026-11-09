using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Grids;

internal sealed class EveryCellSitsInsideTheBoardBounds : Property<GridCell>
{
    protected override GridCell Generate(Random random)
    {
        Grid grid = RandomGrids.Create(random);
        return new GridCell(grid, RandomGrids.CellOf(random, grid));
    }

    protected override void Verify(GridCell sample)
    {
        Bounds3 bounds = sample.Grid.Bounds;
        Check.True(bounds.Contains(sample.Grid.CenterOf(sample.Cell)), $"{sample.Cell} is inside {bounds}");
        Check.Near(sample.Grid.Columns * sample.Grid.CellSize, bounds.Size.X, "as wide as the columns", 1e-3f);
        Check.Near(sample.Grid.Rows * sample.Grid.CellSize, bounds.Size.Z, "as deep as the rows", 1e-3f);
    }
}
