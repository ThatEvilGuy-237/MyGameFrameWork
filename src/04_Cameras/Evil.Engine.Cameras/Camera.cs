using Evil.Engine.Mathematics;
using Evil.Engine.Platform;
using Evil.Engine.World;
using Godot;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Cameras;

public class Camera : WorldObject
{
    public Camera()
        : base(new Camera3D())
    {
    }

    public CameraProjection Projection
    {
        get => Lens.Projection == Camera3D.ProjectionType.Orthogonal ? CameraProjection.Orthographic : CameraProjection.Perspective;
        set => Lens.Projection = value == CameraProjection.Orthographic
            ? Camera3D.ProjectionType.Orthogonal
            : Camera3D.ProjectionType.Perspective;
    }

    public float FieldOfView
    {
        get => Lens.Fov;
        set => Lens.Fov = value;
    }

    public float OrthographicSize
    {
        get => Lens.Size;
        set => Lens.Size = value;
    }

    public float Near
    {
        get => Lens.Near;
        set => Lens.Near = value;
    }

    public float Far
    {
        get => Lens.Far;
        set => Lens.Far = value;
    }

    public bool IsCurrent => Lens.IsInsideTree() && Lens.Current;

    internal Camera3D Lens => (Camera3D)Node;

    public Ray3 ScreenToRay(Vector2 screenPoint)
    {
        RequireInWorld(nameof(ScreenToRay));
        Godot.Vector2 point = screenPoint.ToGodot();
        return new Ray3(Lens.ProjectRayOrigin(point).ToEvil(), Lens.ProjectRayNormal(point).ToEvil());
    }

    public Vector3? PointOnGround(Vector2 screenPoint, float groundHeight = 0f)
    {
        Ray3 ray = ScreenToRay(screenPoint);
        float? distance = ray.IntersectPlane(new Vector3(0f, groundHeight, 0f), Vector3.UnitY);
        return distance is { } along ? ray.PointAt(along) : null;
    }

    public Vector2 WorldToScreen(Vector3 worldPoint)
    {
        RequireInWorld(nameof(WorldToScreen));
        return Lens.UnprojectPosition(worldPoint.ToGodot()).ToEvil();
    }

    public bool CanSee(Vector3 worldPoint)
    {
        RequireInWorld(nameof(CanSee));
        Godot.Vector3 point = worldPoint.ToGodot();
        return !Lens.IsPositionBehind(point) && Lens.IsPositionInFrustum(point);
    }

    internal void MakeCurrent() => Lens.Current = true;

    private void RequireInWorld(string call)
    {
        if (!Lens.IsInsideTree())
        {
            throw new InvalidOperationException($"Camera.{call} needs the camera in a scene's world.");
        }
    }
}
