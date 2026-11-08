using Evil.Engine.World;
using Godot;

namespace Evil.Engine.Cameras;

public class Camera : WorldObject
{
    public Camera()
        : base(new Camera3D())
    {
    }

    public float FieldOfView
    {
        get => Lens.Fov;
        set => Lens.Fov = value;
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

    internal void MakeCurrent() => Lens.Current = true;
}
