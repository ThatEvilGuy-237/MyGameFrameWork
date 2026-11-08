using System.Numerics;
using Evil.Engine.Cameras;
using Evil.Engine.World;

namespace Evil.Engine.Input;

public static class Picker
{
    public static Hit? Pick(Camera camera, WorldObject root, Vector2 screenPoint)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(root);
        var ray = camera.ScreenToRay(screenPoint);
        Hit? nearest = null;
        foreach (var shape in root.Descendants<ShapeObject>())
        {
            if (!shape.Pickable || !shape.IsShown)
            {
                continue;
            }

            if (shape.HitDistance(ray) is { } distance && (nearest is null || distance < nearest.Distance))
            {
                nearest = new Hit(shape, ray.PointAt(distance), distance);
            }
        }

        return nearest;
    }
}
