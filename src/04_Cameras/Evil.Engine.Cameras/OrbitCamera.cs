using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Cameras;

public sealed class OrbitCamera
{
    private Vector3 _target;
    private Vector3 _targetGoal;
    private float _yaw;
    private float _yawGoal;
    private float _pitch = 45f;
    private float _pitchGoal = 45f;
    private float _distance = 12f;
    private float _distanceGoal = 12f;
    private OrbitLimits _limits;

    public OrbitCamera(Camera camera)
        : this(camera, OrbitLimits.Default)
    {
    }

    public OrbitCamera(Camera camera, OrbitLimits limits)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(limits);
        limits.Validate();
        Camera = camera;
        _limits = limits;
        Pitch = _pitchGoal;
        Distance = _distanceGoal;
        Snap();
    }

    public Camera Camera { get; }

    public Vector3 Target
    {
        get => _targetGoal;
        set => _targetGoal = value;
    }

    public float Yaw
    {
        get => _yawGoal;
        set => _yawGoal = value;
    }

    public float Pitch
    {
        get => _pitchGoal;
        set => _pitchGoal = Math.Clamp(value, _limits.MinPitch, _limits.MaxPitch);
    }

    public float Distance
    {
        get => _distanceGoal;
        set => _distanceGoal = Math.Clamp(value, _limits.MinDistance, _limits.MaxDistance);
    }

    public OrbitLimits Limits
    {
        get => _limits;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            value.Validate();
            _limits = value;
            Pitch = _pitchGoal;
            Distance = _distanceGoal;
        }
    }

    public float Sharpness { get; set; } = 10f;

    public Pose CurrentPose => Orbit.PoseFor(_target, _yaw, _pitch, _distance);

    public void Turn(float degrees) => Yaw += degrees;

    public void Tilt(float degrees) => Pitch += degrees;

    public void Zoom(float factor)
    {
        if (factor <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "Zoom takes a factor above 0: below 1 comes closer, above 1 moves away.");
        }

        Distance *= factor;
    }

    public void Pan(Vector2 amount)
    {
        Quaternion heading = Rotations.AroundAxis(Vector3.UnitY, _yawGoal);
        Vector3 right = Rotations.Rotate(Vector3.UnitX, heading);
        Vector3 ahead = Rotations.Rotate(Rotations.Forward, heading);
        Target += right * amount.X + ahead * amount.Y;
    }

    public void Snap()
    {
        _target = _targetGoal;
        _yaw = _yawGoal;
        _pitch = _pitchGoal;
        _distance = _distanceGoal;
        Apply();
    }

    public void Update(GameTime time) => Update(time.DeltaSeconds);

    public void Update(float seconds)
    {
        _target = Smoothing.Toward(_target, _targetGoal, Sharpness, seconds);
        _yaw = Smoothing.Toward(_yaw, _yawGoal, Sharpness, seconds);
        _pitch = Smoothing.Toward(_pitch, _pitchGoal, Sharpness, seconds);
        _distance = Smoothing.Toward(_distance, _distanceGoal, Sharpness, seconds);
        Apply();
    }

    private void Apply() => Camera.Pose = CurrentPose;
}
