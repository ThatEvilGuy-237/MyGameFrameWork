using Evil.Engine.Lighting;
using Godot;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Characters;

public sealed class KeyLight : Light
{
    private const float Reach = 30f;

    public KeyLight(Vector3 direction)
        : base(new DirectionalLight3D())
    {
        Direction = direction;
        Shadows = true;
        Native.LightCullMask = CharacterLayer.Mask;
        Native.ShadowCasterMask = CharacterLayer.Mask;
        Native.DirectionalShadowMaxDistance = Reach;
        Native.DirectionalShadowMode = DirectionalLight3D.ShadowMode.Orthogonal;
    }

    public Vector3 Direction
    {
        get => Pointing;
        set => Pointing = value;
    }

    public float ShadowDistance
    {
        get => Native.DirectionalShadowMaxDistance;
        set => Native.DirectionalShadowMaxDistance = Math.Max(1f, value);
    }

    private DirectionalLight3D Native => (DirectionalLight3D)Node;
}
