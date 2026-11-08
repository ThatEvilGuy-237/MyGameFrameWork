using Evil.Engine.Mathematics;
using Evil.Engine.Platform;
using Godot;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.World;

public class WorldObject
{
    private readonly List<WorldObject> _children = [];
    private string _name;

    public WorldObject()
        : this(new Node3D())
    {
    }

    public WorldObject(string name)
        : this(new Node3D()) => Name = name;

    private protected WorldObject(Node3D node)
    {
        Node = node;
        _name = GetType().Name;
        Node.Name = _name;
    }

    internal WorldObject(Node3D node, string adoptedName)
    {
        Node = node;
        _name = adoptedName;
    }

    public string Name
    {
        get => _name;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _name = value;
            Live.Name = value;
        }
    }

    public WorldObject? Parent { get; private set; }

    public IReadOnlyList<WorldObject> Children => _children;

    public bool IsDestroyed { get; private set; }

    public object? Tag { get; set; }

    public bool IsShown => Live.IsInsideTree() ? Live.IsVisibleInTree() : VisibleUpTheChain();

    public Vector3 Position
    {
        get => Live.Position.ToEvil();
        set => Live.Position = Finite.Point(value, $"{Name}'s Position").ToGodot();
    }

    public Vector3 Rotation
    {
        get => Live.RotationDegrees.ToEvil();
        set => Live.RotationDegrees = Finite.Point(value, $"{Name}'s Rotation").ToGodot();
    }

    public Quaternion Orientation
    {
        get => Live.Quaternion.ToEvil();
        set
        {
            if (Finite.Rotation(value, $"{Name}'s Orientation").LengthSquared() < 1e-12f)
            {
                throw new ArgumentException($"{Name}'s Orientation has no length, so it is no turn at all. Use Quaternion.Identity for no turn.", nameof(value));
            }

            Live.Quaternion = Quaternion.Normalize(value).ToGodot();
        }
    }

    public Vector3 Scale
    {
        get => Live.Scale.ToEvil();
        set => Live.Scale = Finite.Point(value, $"{Name}'s Scale").ToGodot();
    }

    public Pose Pose
    {
        get => new Pose(Position, Orientation, Scale);
        set
        {
            Position = value.Position;
            Orientation = value.Rotation;
            Scale = value.Scale;
        }
    }

    public bool Visible
    {
        get => Live.Visible;
        set => Live.Visible = value;
    }

    public Vector3 WorldPosition => WorldTransform.Origin.ToEvil();

    public Pose WorldPose
    {
        get
        {
            Transform3D world = WorldTransform;
            return new Pose(world.Origin.ToEvil(), world.Basis.GetRotationQuaternion().ToEvil(), world.Basis.Scale.ToEvil());
        }
    }

    public Vector3 Forward => (-WorldTransform.Basis.Z).Normalized().ToEvil();

    public Vector3 Right => WorldTransform.Basis.X.Normalized().ToEvil();

    public Vector3 Up => WorldTransform.Basis.Y.Normalized().ToEvil();

    internal Node3D Node { get; }

    internal Transform3D WorldTransform
    {
        get
        {
            Node3D node = Live;
            if (node.IsInsideTree())
            {
                return node.GlobalTransform;
            }

            return Parent is null ? node.Transform : Parent.WorldTransform * node.Transform;
        }
    }

    private Node3D Live => IsDestroyed
        ? throw Destroyed()
        : Node;

    public T Add<T>(T child)
        where T : WorldObject
    {
        ArgumentNullException.ThrowIfNull(child);
        child.RequireAlive();
        if (child.Parent is not null)
        {
            throw new InvalidOperationException($"'{child.Name}' already belongs to '{child.Parent.Name}'; remove it there first.");
        }

        for (WorldObject? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ReferenceEquals(ancestor, child))
            {
                throw new InvalidOperationException($"'{child.Name}' cannot go inside itself or its own children.");
            }
        }

        Live.AddChild(child.Node);
        child.Parent = this;
        _children.Add(child);
        return child;
    }

    public void Remove(WorldObject child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (!_children.Contains(child))
        {
            throw new InvalidOperationException($"'{child.Name}' is not inside '{Name}'. Remove takes direct children only: call Remove on '{child.Parent?.Name ?? "nothing"}', its parent.");
        }

        Detach(child);
    }

    public void Destroy()
    {
        if (IsDestroyed)
        {
            return;
        }

        Parent?.Detach(this);
        Node3D node = Node;
        MarkDestroyed();
        node.QueueFree();
    }

    public WorldObject? Find(string name)
    {
        Queue<WorldObject> waiting = new Queue<WorldObject>(_children);
        while (waiting.TryDequeue(out WorldObject? candidate))
        {
            if (candidate.Name == name)
            {
                return candidate;
            }

            foreach (WorldObject child in candidate._children)
            {
                waiting.Enqueue(child);
            }
        }

        return null;
    }

    internal void RequireAlive()
    {
        if (IsDestroyed)
        {
            throw Destroyed();
        }
    }

    public T? SelfOrAncestor<T>()
        where T : WorldObject => this as T ?? Ancestor<T>();

    public T? Ancestor<T>()
        where T : WorldObject
    {
        for (WorldObject? current = Parent; current is not null; current = current.Parent)
        {
            if (current is T match)
            {
                return match;
            }
        }

        return null;
    }

    public IEnumerable<T> Descendants<T>()
        where T : WorldObject
    {
        Queue<WorldObject> waiting = new Queue<WorldObject>(_children);
        while (waiting.TryDequeue(out WorldObject? candidate))
        {
            if (candidate is T match)
            {
                yield return match;
            }

            foreach (WorldObject child in candidate._children)
            {
                waiting.Enqueue(child);
            }
        }
    }

    public Vector3 ToWorld(Vector3 local) => (WorldTransform * local.ToGodot()).ToEvil();

    public Vector3 ToLocal(Vector3 world) => (WorldTransform.AffineInverse() * world.ToGodot()).ToEvil();

    public void Translate(Vector3 offset) => Position += offset;

    public void Rotate(Vector3 axis, float degrees)
    {
        if (axis.LengthSquared() < 1e-12f)
        {
            throw new ArgumentException("The axis to turn around has no length.", nameof(axis));
        }

        Live.Rotate(axis.ToGodot().Normalized(), Mathf.DegToRad(degrees));
    }

    public void RotateY(float degrees) => Live.Rotate(Godot.Vector3.Up, Mathf.DegToRad(degrees));

    public void LookAt(Vector3 target)
    {
        Godot.Vector3 direction = target.ToGodot() - WorldTransform.Origin;
        if (direction.LengthSquared() < 1e-12f)
        {
            return;
        }

        Basis parentBasis = ParentWorldBasis();
        Godot.Vector3 local = parentBasis.Inverse() * direction;
        Live.Quaternion = Facing.Along(local).GetRotationQuaternion();
    }

    private Basis ParentWorldBasis()
    {
        if (Parent is not null)
        {
            return Parent.WorldTransform.Basis;
        }

        return Node.IsInsideTree() && Node.GetParent() is Node3D parent ? parent.GlobalTransform.Basis : Basis.Identity;
    }

    internal void Adopt(WorldObject child)
    {
        child.Parent = this;
        _children.Add(child);
    }

    private bool VisibleUpTheChain()
    {
        for (WorldObject? current = this; current is not null; current = current.Parent)
        {
            if (!current.Node.Visible)
            {
                return false;
            }
        }

        return true;
    }

    private void Detach(WorldObject child)
    {
        _children.Remove(child);
        Node.RemoveChild(child.Node);
        child.Parent = null;
    }

    private void MarkDestroyed()
    {
        IsDestroyed = true;
        foreach (WorldObject child in _children)
        {
            child.MarkDestroyed();
        }
    }

    private ObjectDisposedException Destroyed() =>
        new ObjectDisposedException(_name, $"'{_name}' was destroyed: Destroy() took it out of the world for good. Make a new one, or check IsDestroyed before using it.");
}
