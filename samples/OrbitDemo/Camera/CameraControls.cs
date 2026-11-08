namespace OrbitDemo;

internal sealed class CameraControls
{
    private const float DriftSpeed = 12f;
    private const float KeyTurnSpeed = 60f;
    private const float ZoomPerNotch = 0.85f;

    private readonly OrbitCamera _orbit;

    public CameraControls(Camera camera)
    {
        camera.FieldOfView = 50f;
        _orbit = new OrbitCamera(camera, new OrbitLimits(MinDistance: 6f, MaxDistance: 30f));
        _orbit.Target = new Vector3(0f, 0f, 2.2f);
        _orbit.Distance = 13f;
        _orbit.Pitch = 32f;
        _orbit.Yaw = -15f;
        _orbit.Snap();
    }

    public float Yaw => _orbit.Yaw;

    public void Drift(float seconds) => _orbit.Turn(DriftSpeed * seconds);

    public void Steer(Keyboard keys, Mouse mouse, float seconds)
    {
        if (keys.IsDown(Key.Q))
        {
            _orbit.Turn(-KeyTurnSpeed * seconds);
        }

        if (keys.IsDown(Key.E))
        {
            _orbit.Turn(KeyTurnSpeed * seconds);
        }

        if (mouse.Scroll != 0f)
        {
            _orbit.Zoom(MathF.Pow(ZoomPerNotch, mouse.Scroll));
            Console.WriteLine($"zoom: {_orbit.Distance:F1}");
        }

        if (mouse.IsDown(MouseButton.Right))
        {
            _orbit.Turn(-mouse.Delta.X * 0.3f);
            _orbit.Tilt(mouse.Delta.Y * 0.2f);
        }

        _orbit.Update(seconds);
    }
}
