using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Grids;

internal sealed class GridDistances
{
    private const int Unreached = int.MaxValue;

    private readonly GridBoard _board;
    private readonly int[,] _steps;

    public GridDistances(GridBoard board)
    {
        _board = board;
        _steps = new int[board.Grid.Columns, board.Grid.Rows];
        for (int column = 0; column < board.Grid.Columns; column++)
        {
            for (int row = 0; row < board.Grid.Rows; row++)
            {
                _steps[column, row] = Unreached;
            }
        }

        _steps[board.From.Column, board.From.Row] = 0;
        while (RelaxOnce())
        {
        }
    }

    public bool Reaches(Cell cell) => _steps[cell.Column, cell.Row] != Unreached;

    public int StepsTo(Cell cell) => _steps[cell.Column, cell.Row];

    private bool RelaxOnce()
    {
        bool changed = false;
        for (int column = 0; column < _board.Grid.Columns; column++)
        {
            for (int row = 0; row < _board.Grid.Rows; row++)
            {
                changed |= RelaxAround(new Cell(column, row));
            }
        }

        return changed;
    }

    private bool RelaxAround(Cell cell)
    {
        int here = _steps[cell.Column, cell.Row];
        if (here == Unreached)
        {
            return false;
        }

        bool changed = false;
        foreach (Cell offset in StepOffsets())
        {
            Cell next = new Cell(cell.Column + offset.Column, cell.Row + offset.Row);
            if (Walkable(next) && here + 1 < _steps[next.Column, next.Row])
            {
                _steps[next.Column, next.Row] = here + 1;
                changed = true;
            }
        }

        return changed;
    }

    private List<Cell> StepOffsets()
    {
        List<Cell> offsets = new List<Cell>();
        for (int across = -1; across <= 1; across++)
        {
            for (int along = -1; along <= 1; along++)
            {
                bool still = across == 0 && along == 0;
                bool diagonal = across != 0 && along != 0;
                if (!still && (!diagonal || _board.Grid.Moves == GridMoves.Eight))
                {
                    offsets.Add(new Cell(across, along));
                }
            }
        }

        return offsets;
    }

    private bool Walkable(Cell cell) =>
        cell.Column >= 0 && cell.Column < _board.Grid.Columns && cell.Row >= 0 && cell.Row < _board.Grid.Rows && !_board.Blocked.Contains(cell);
}
