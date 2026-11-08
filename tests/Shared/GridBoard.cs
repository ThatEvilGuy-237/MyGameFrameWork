using Evil.Engine.Mathematics;

namespace Evil.Engine.Testing;

internal sealed record GridBoard(Grid Grid, Cell From, Cell To, HashSet<Cell> Blocked, int Steps);
