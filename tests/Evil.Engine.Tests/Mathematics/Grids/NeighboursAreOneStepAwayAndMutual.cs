using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Grids;

internal sealed class NeighboursAreOneStepAwayAndMutual : Property<GridCell>
{
    protected override GridCell Generate(Random random)
    {
        Grid grid = RandomGrids.Create(random);
        return new GridCell(grid, RandomGrids.CellOf(random, grid));
    }

    protected override void Verify(GridCell sample)
    {
        IReadOnlyList<Cell> neighbours = sample.Grid.Neighbours(sample.Cell);
        int most = sample.Grid.Moves == GridMoves.Eight ? 8 : 4;
        Check.True(neighbours.Count <= most, $"{neighbours.Count} neighbours, at most {most}");
        Check.Equal(neighbours.Count, new HashSet<Cell>(neighbours).Count, "each neighbour once");
        foreach (Cell next in neighbours)
        {
            Check.True(sample.Grid.Contains(next), $"{next} is on the board");
            Check.Equal(1, sample.Cell.StepsTo(next, sample.Grid.Moves), $"{next} is one step away");
            Check.True(Contains(sample.Grid.Neighbours(next), sample.Cell), $"{next} has {sample.Cell} as its neighbour too");
        }
    }

    private static bool Contains(IReadOnlyList<Cell> cells, Cell wanted)
    {
        foreach (Cell cell in cells)
        {
            if (cell == wanted)
            {
                return true;
            }
        }

        return false;
    }
}
