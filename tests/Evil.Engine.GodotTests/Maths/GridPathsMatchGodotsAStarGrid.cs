using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal sealed class GridPathsMatchGodotsAStarGrid : Property<GridBoard>
{
    protected override GridBoard Generate(Random random) => RandomGrids.Board(random);

    protected override void Verify(GridBoard sample)
    {
        if (sample.Blocked.Contains(sample.From))
        {
            return;
        }

        using Godot.AStarGrid2D godots = GodotsGrid(sample);
        Godot.Collections.Array<Godot.Vector2I> path = godots.GetIdPath(Spot(sample.From), Spot(sample.To));
        int godotsSteps = Math.Max(0, path.Count - 1);
        int ourSteps = sample.Grid.Path(sample.From, sample.To, sample.Blocked).Count;
        Check.Equal(godotsSteps, ourSteps, $"steps from {sample.From} to {sample.To}");
    }

    private static Godot.AStarGrid2D GodotsGrid(GridBoard sample)
    {
        Godot.AStarGrid2D grid = new Godot.AStarGrid2D();
        grid.Region = new Godot.Rect2I(0, 0, sample.Grid.Columns, sample.Grid.Rows);
        grid.DiagonalMode = sample.Grid.Moves == GridMoves.Eight
            ? Godot.AStarGrid2D.DiagonalModeEnum.Always
            : Godot.AStarGrid2D.DiagonalModeEnum.Never;
        grid.DefaultComputeHeuristic = Godot.AStarGrid2D.Heuristic.Chebyshev;
        grid.DefaultEstimateHeuristic = Godot.AStarGrid2D.Heuristic.Chebyshev;
        grid.Update();
        foreach (Cell cell in sample.Blocked)
        {
            grid.SetPointSolid(Spot(cell), true);
        }

        return grid;
    }

    private static Godot.Vector2I Spot(Cell cell) => new Godot.Vector2I(cell.Column, cell.Row);
}
