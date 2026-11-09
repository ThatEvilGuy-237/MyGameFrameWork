using System.Numerics;

namespace Evil.Engine.Mathematics;

public sealed class Grid
{
    private static readonly Cell[] Straight = [new Cell(1, 0), new Cell(-1, 0), new Cell(0, 1), new Cell(0, -1)];
    private static readonly Cell[] Diagonal = [new Cell(1, 1), new Cell(1, -1), new Cell(-1, 1), new Cell(-1, -1)];

    public Grid(int columns, int rows, float cellSize, Vector3 corner, GridMoves moves = GridMoves.Four)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);
        if (!(Finite.Number(cellSize, "A grid's cell size") > 0f))
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "A grid needs cells bigger than 0.");
        }

        Columns = columns;
        Rows = rows;
        CellSize = cellSize;
        Corner = Finite.Point(corner, "A grid's corner");
        Moves = moves;
    }

    public int Columns { get; }

    public int Rows { get; }

    public float CellSize { get; }

    public Vector3 Corner { get; }

    public GridMoves Moves { get; }

    public Bounds3 Bounds => new Bounds3(Corner, Corner + new Vector3(Columns * CellSize, 0f, Rows * CellSize));

    public override string ToString() => $"{Columns}x{Rows} grid of {CellSize} cells at {Corner}, {Moves} moves";

    public static Grid Centered(int columns, int rows, float cellSize, GridMoves moves = GridMoves.Four) =>
        new Grid(columns, rows, cellSize, new Vector3(-columns * cellSize / 2f, 0f, -rows * cellSize / 2f), moves);

    public bool Contains(Cell cell) => cell.Column >= 0 && cell.Column < Columns && cell.Row >= 0 && cell.Row < Rows;

    public Vector3 CenterOf(Cell cell) =>
        Corner + new Vector3((cell.Column + 0.5f) * CellSize, 0f, (cell.Row + 0.5f) * CellSize);

    public Cell? CellAt(Vector3 point)
    {
        Vector3 local = Finite.Point(point, "The point to find a cell at") - Corner;
        Cell cell = new Cell((int)MathF.Floor(local.X / CellSize), (int)MathF.Floor(local.Z / CellSize));
        return Contains(cell) ? cell : null;
    }

    public IReadOnlyList<Cell> Neighbours(Cell cell)
    {
        List<Cell> found = new List<Cell>(8);
        AddInside(cell, Straight, found);
        if (Moves == GridMoves.Eight)
        {
            AddInside(cell, Diagonal, found);
        }

        return found;
    }

    public IReadOnlyList<Cell> Path(Cell from, Cell to, IReadOnlySet<Cell> blocked) =>
        new GridSearch(this, from, blocked).PathTo(to);

    public IReadOnlyList<Cell> Reachable(Cell from, int steps, IReadOnlySet<Cell> blocked) =>
        new GridSearch(this, from, blocked).Within(steps);

    private void AddInside(Cell cell, Cell[] directions, List<Cell> found)
    {
        foreach (Cell direction in directions)
        {
            Cell next = cell.Offset(direction.Column, direction.Row);
            if (Contains(next))
            {
                found.Add(next);
            }
        }
    }
}
