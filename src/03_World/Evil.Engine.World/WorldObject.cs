using Evil.Engine.Platform;
using Godot;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.World;

public class WorldObject
{
    private readonly List<WorldObject> _children = [];

    public WorldObject()
        : this(new Node3D())
    {
    }

    public WorldObject(string name)
        : this(new Node3D()) => Name = name;

    private protected WorldObject(Node3D node) => Node = node;

    public string Name
    {
        get => Node.Name.ToString();
        set => Node.Name = value;
    }

    public WorldObject? Parent { get; private set; }

    public IReadOnlyList<WorldObject> Children => _children;

    public Vector3 Position
    {
        get => Node.Position.ToEvil();
        set => Node.Position = value.ToGodot();
    }

    public Vector3 Rotation
    {
        get => Node.RotationDegrees.ToEvil();
        set => Node.RotationDegrees = value.ToGodot();
    }

    public Vector3 Scale
    {
        get => Node.Scale.ToEvil();
        set => Node.Scale = value.ToGodot();
    }

    public bool Visible
    {
        get => Node.Visible;
        set => Node.Visible = value;
    }

    public Vector3 WorldPosition => Node.IsInsideTree() ? Node.GlobalPosition.ToEvil() : Position;

    internal Node3D Node { get; }

    public T Add<T>(T child)
        where T : WorldObject
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child.Parent is not null)
        {
            throw new InvalidOperationException($"'{child.Name}' already belongs to '{child.Parent.Name}'; remove it there first.");
        }

        for (var ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ReferenceEquals(ancestor, child))
            {
                throw new InvalidOperationException($"'{child.Name}' cannot go inside itself or its own children.");
            }
        }

        Node.AddChild(child.Node);
        child.Parent = this;
        _children.Add(child);
        return child;
    }

    public void Remove(WorldObject child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (!_children.Remove(child))
        {
            throw new InvalidOperationException($"'{child.Name}' is not inside '{Name}'.");
        }

        Node.RemoveChild(child.Node);
        child.Parent = null;
    }

    public void Translate(Vector3 offset) => Position += offset;

    public void Rotate(Vector3 axis, float degrees)
    {
        if (axis.LengthSquared() < 1e-12f)
        {
            throw new ArgumentException("The axis to turn around has no length.", nameof(axis));
        }

        Node.Rotate(axis.ToGodot().Normalized(), Mathf.DegToRad(degrees));
    }

    public void RotateY(float degrees) => Node.Rotate(Godot.Vector3.Up, Mathf.DegToRad(degrees));

    public void LookAt(Vector3 target)
    {
        var direction = target - WorldPosition;
        if (direction.LengthSquared() < 1e-12f)
        {
            return;
        }

        var facing = Facing.Along(direction.ToGodot());
        if (Node.IsInsideTree())
        {
            var global = Node.GlobalTransform;
            Node.GlobalTransform = new Transform3D(facing.Scaled(global.Basis.Scale), global.Origin);
        }
        else
        {
            Node.Basis = facing.Scaled(Node.Scale);
        }
    }
}
