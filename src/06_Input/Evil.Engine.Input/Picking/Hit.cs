using System.Numerics;
using Evil.Engine.World;

namespace Evil.Engine.Input;

public sealed record Hit(WorldObject Target, Vector3 Point, float Distance);
