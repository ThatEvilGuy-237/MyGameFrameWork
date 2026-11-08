using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Testing;

internal static class RandomGrids
{
    public static Grid Create(Random random)
    {
        GridMoves moves = Gen.Chance(random, 0.5) ? GridMoves.Eight : GridMoves.Four;
        return new Grid(random.Next(1, 13), random.Next(1, 13), Gen.Number(random, 0.25f, 3f), Gen.Point3(random, 20f), moves);
    }

    public static Cell CellOf(Random random, Grid grid) => new Cell(random.Next(grid.Columns), random.Next(grid.Rows));

    public static GridBoard Board(Random random)
    {
        Grid grid = Create(random);
        HashSet<Cell> blocked = new HashSet<Cell>();
        double share = Gen.Number(random, 0f, 0.45f);
        for (int column = 0; column < grid.Columns; column++)
        {
            for (int row = 0; row < grid.Rows; row++)
            {
                if (Gen.Chance(random, share))
                {
                    blocked.Add(new Cell(column, row));
                }
            }
        }

        return new GridBoard(grid, CellOf(random, grid), CellOf(random, grid), blocked, random.Next(0, 8));
    }
}
