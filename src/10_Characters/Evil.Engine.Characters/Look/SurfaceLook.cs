using Evil.Engine.Platform;
using Godot;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Characters;

internal sealed class SurfaceLook
{
    private static readonly string[] Slots = ["paint_a", "paint_b"];

    private readonly List<PaintLayer> _paints = new List<PaintLayer>();

    public SurfaceLook(SurfaceDescription surface, Shader shader, PictureBook pictures)
    {
        Material = new ShaderMaterial();
        Material.Shader = shader;
        Material.SetShaderParameter("base_color", surface.Lit.ToGodot());
        if (surface.Inside is Vector3 inside)
        {
            Material.SetShaderParameter("inside_color", inside.ToGodot());
            Material.SetShaderParameter("inside_amount", 1f);
        }

        if (surface.Paints.Count > Slots.Length)
        {
            throw new InvalidDataException($"Character: the surface '{surface.Name}' has {surface.Paints.Count} paint layers; a surface takes {Slots.Length}.");
        }

        for (int index = 0; index < surface.Paints.Count; index++)
        {
            _paints.Add(new PaintLayer(Material, Slots[index], surface.Paints[index], pictures));
        }
    }

    public ShaderMaterial Material { get; }

    public void Show(IReadOnlyDictionary<string, string> swaps)
    {
        foreach (PaintLayer paint in _paints)
        {
            paint.Show(swaps);
        }
    }
}
