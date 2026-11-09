using Evil.Engine.Mathematics;
using Evil.Engine.Platform;
using Evil.Engine.World;
using Godot;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Cameras;

public class Camera : WorldObject
{
    private const float BreathingRoom = 1.05f;

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

    public void Frame(Bounds3 bounds, float pitchDegrees = 50f, float yawDegrees = 0f)
    {
        RequireInWorld(nameof(Frame));
        float radius = MathF.Max(bounds.Size.Length() / 2f, 0.01f) * BreathingRoom;
        Pose eye = Orbit.PoseFor(bounds.Center, yawDegrees, pitchDegrees, DistanceToFit(radius));
        Position = Parent is null ? eye.Position : Parent.ToLocal(eye.Position);
        LookAt(bounds.Center);
        Far = MathF.Max(Far, Vector3.Distance(eye.Position, bounds.Center) + radius * 2f);
        if (Projection == CameraProjection.Orthographic)
        {
            OrthographicSize = radius * 2f * MathF.Max(1f, 1f / ScreenAspect());
        }
    }

    internal void MakeCurrent() => Lens.Current = true;

    private float DistanceToFit(float radius)
    {
        if (Projection == CameraProjection.Orthographic)
        {
            return radius + Near + 1f;
        }

        float verticalHalf = Angle.ToRadians(FieldOfView) / 2f;
        float horizontalHalf = MathF.Atan(MathF.Tan(verticalHalf) * ScreenAspect());
        return radius / MathF.Sin(MathF.Min(verticalHalf, horizontalHalf)) + Near;
    }

    private float ScreenAspect()
    {
        Godot.Vector2 screen = Lens.GetViewport().GetVisibleRect().Size;
        return screen.X / MathF.Max(1f, screen.Y);
    }

    private void RequireInWorld(string call)
    {
        if (!Lens.IsInsideTree())
        {
            throw new InvalidOperationException($"Camera.{call} needs the camera in a scene's world: use the scene's own Camera, or World.Add this one first.");
        }
    }
}
