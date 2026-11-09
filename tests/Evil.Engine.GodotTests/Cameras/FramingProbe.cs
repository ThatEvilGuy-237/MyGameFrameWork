using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Cameras;

internal sealed record FramingProbe(Bounds3 Bounds, float Pitch, float Yaw, float FieldOfView, bool Orthographic);
