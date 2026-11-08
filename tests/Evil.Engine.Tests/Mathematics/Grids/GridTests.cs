using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Grids;

internal static class GridTests
{
    [Test]
    public static void ACenteredBoardHasTheOriginInTheMiddle()
    {
        Grid board = Grid.Centered(8, 8, 1f);
        Check.Equal(new Cell(4, 4), board.CellAt(new Vector3(0.5f, 0f, 0.5f)), "the cell just past the middle");
        Check.Equal(new Cell(3, 3), board.CellAt(new Vector3(-0.5f, 0f, -0.5f)), "the cell just before it");
        Check.Near(new Vector3(-3.5f, 0f, -3.5f), board.CenterOf(new Cell(0, 0)), "the first cell's center");
        Check.True(board.CellAt(new Vector3(4.5f, 0f, 0f)) is null, "off the board");
    }

    [Test]
    public static void AWalledOffGoalHasNoPath()
    {
        Grid board = new Grid(5, 1, 1f, Vector3.Zero);
        HashSet<Cell> wall = [new Cell(2, 0)];
        Check.Equal(0, board.Path(new Cell(0, 0), new Cell(4, 0), wall).Count, "the wall cuts the row");
        Check.Equal(4, board.Path(new Cell(0, 0), new Cell(4, 0), new HashSet<Cell>()).Count, "four steps without it");
    }

    [Test]
    public static void ImpossibleBoardsAreRefused()
    {
        void NoColumns() => _ = new Grid(0, 3, 1f, Vector3.Zero);
        void FlatCells() => _ = new Grid(3, 3, 0f, Vector3.Zero);
        void StartOffTheBoard() => _ = new Grid(3, 3, 1f, Vector3.Zero).Path(new Cell(5, 0), new Cell(0, 0), new HashSet<Cell>());
        Check.Throws<ArgumentOutOfRangeException>(NoColumns);
        Check.Throws<ArgumentOutOfRangeException>(FlatCells, "bigger than 0");
        Check.Throws<ArgumentOutOfRangeException>(StartOffTheBoard, "outside the 3x3 grid");
    }
}
