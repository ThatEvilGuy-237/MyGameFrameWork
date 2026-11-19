using Godot;
using Vector4 = System.Numerics.Vector4;

namespace Evil.Engine.Characters;

internal sealed class PaintLayer
{
    private readonly ShaderMaterial _material;
    private readonly string _slot;
    private readonly PaintDescription _paint;
    private readonly PictureBook _pictures;

    public PaintLayer(ShaderMaterial material, string slot, PaintDescription paint, PictureBook pictures)
    {
        _material = material;
        _slot = slot;
        _paint = paint;
        _pictures = pictures;
        _material.SetShaderParameter($"{slot}_weight", Channel(paint.Weight));
        _material.SetShaderParameter($"{slot}_amount", 1f);
        Show(paint.Picture);
    }

    public void Show(IReadOnlyDictionary<string, string> swaps) =>
        Show(swaps.TryGetValue(_paint.Picture, out string? other) ? other : _paint.Picture);

    private void Show(string name)
    {
        LookPicture picture = _pictures.Of(name);
        Vector4 region = picture.Region;
        _material.SetShaderParameter(_slot, picture.Texture);
        _material.SetShaderParameter($"{_slot}_region", new Godot.Vector4(region.X, region.Y, region.Z, region.W));
    }

    private static Godot.Vector4 Channel(PaintWeight weight) => weight switch
    {
        PaintWeight.Front => new Godot.Vector4(1f, 0f, 0f, 0f),
        PaintWeight.Back => new Godot.Vector4(0f, 1f, 0f, 0f),
        _ => new Godot.Vector4(0f, 0f, 0f, 0f),
    };
}
