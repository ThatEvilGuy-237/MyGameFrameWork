using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Grids;

internal sealed class EveryPointBelongsToTheCellUnderIt : Property<GridPoint>
{
    protected override GridPoint Generate(Random random)
    {
        Grid grid = RandomGrids.Create(random);
        float width = grid.Columns * grid.CellSize;
        float depth = grid.Rows * grid.CellSize;
        Vector3 point = grid.Corner + new Vector3(Gen.Number(random, -0.5f, 1.5f) * width, Gen.Number(random, -5f, 5f), Gen.Number(random, -0.5f, 1.5f) * depth);
        return new GridPoint(grid, point);
    }

    protected override void Verify(GridPoint sample)
    {
        Vector3 local = sample.Point - sample.Grid.Corner;
        float column = local.X / sample.Grid.CellSize;
        float row = local.Z / sample.Grid.CellSize;
        if (NearAnEdge(column) || NearAnEdge(row))
        {
            return;
        }

        bool inside = column >= 0f && column < sample.Grid.Columns && row >= 0f && row < sample.Grid.Rows;
        Cell? expected = inside ? new Cell((int)column, (int)row) : null;
        Check.Equal(expected, sample.Grid.CellAt(sample.Point), $"the cell under ({local.X:0.00}, {local.Z:0.00})");
    }

    private static bool NearAnEdge(float position) => MathF.Abs(position - MathF.Round(position)) < 1e-3f;
}
