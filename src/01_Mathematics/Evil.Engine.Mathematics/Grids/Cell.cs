namespace Evil.Engine.Mathematics;

public readonly record struct Cell(int Column, int Row)
{
    public Cell Offset(int columns, int rows) => new Cell(Column + columns, Row + rows);

    public int StepsTo(Cell other, GridMoves moves)
    {
        int across = Math.Abs(other.Column - Column);
        int along = Math.Abs(other.Row - Row);
        return moves == GridMoves.Eight ? Math.Max(across, along) : across + along;
    }
}
