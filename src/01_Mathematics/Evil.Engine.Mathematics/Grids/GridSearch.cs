namespace Evil.Engine.Mathematics;

internal sealed class GridSearch
{
    private readonly Grid _grid;
    private readonly Cell _from;
    private readonly Dictionary<Cell, int> _steps = new Dictionary<Cell, int>();
    private readonly Dictionary<Cell, Cell> _cameFrom = new Dictionary<Cell, Cell>();

    public GridSearch(Grid grid, Cell from, IReadOnlySet<Cell> blocked)
    {
        ArgumentNullException.ThrowIfNull(blocked);
        if (!grid.Contains(from))
        {
            throw new ArgumentOutOfRangeException(nameof(from), from, $"The start {from} is outside the {grid.Columns}x{grid.Rows} grid.");
        }

        _grid = grid;
        _from = from;
        Spread(blocked);
    }

    public IReadOnlyList<Cell> PathTo(Cell to)
    {
        if (!_grid.Contains(to))
        {
            throw new ArgumentOutOfRangeException(nameof(to), to, $"The goal {to} is outside the {_grid.Columns}x{_grid.Rows} grid.");
        }

        List<Cell> path = new List<Cell>();
        if (to == _from || !_steps.ContainsKey(to))
        {
            return path;
        }

        for (Cell at = to; at != _from; at = _cameFrom[at])
        {
            path.Add(at);
        }

        path.Reverse();
        return path;
    }

    public IReadOnlyList<Cell> Within(int steps)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(steps);
        List<Cell> cells = new List<Cell>();
        foreach (KeyValuePair<Cell, int> entry in _steps)
        {
            if (entry.Value <= steps)
            {
                cells.Add(entry.Key);
            }
        }

        return cells;
    }

    private void Spread(IReadOnlySet<Cell> blocked)
    {
        Queue<Cell> frontier = new Queue<Cell>();
        _steps[_from] = 0;
        frontier.Enqueue(_from);
        while (frontier.TryDequeue(out Cell at))
        {
            foreach (Cell next in _grid.Neighbours(at))
            {
                if (blocked.Contains(next) || _steps.ContainsKey(next))
                {
                    continue;
                }

                _steps[next] = _steps[at] + 1;
                _cameFrom[next] = at;
                frontier.Enqueue(next);
            }
        }
    }
}
