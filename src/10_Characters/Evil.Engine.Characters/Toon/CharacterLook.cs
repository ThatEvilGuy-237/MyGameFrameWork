using Evil.Engine.Platform;
using Godot;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Characters;

internal sealed class CharacterLook
{
    private static readonly IReadOnlyDictionary<string, string> NoSwaps = new Dictionary<string, string>();

    private readonly Dictionary<MeshInstance3D, List<ToonSurface>> _painted = new Dictionary<MeshInstance3D, List<ToonSurface>>();
    private readonly ShaderMaterial _line = new ShaderMaterial();
    private readonly ToonShaders _shaders;
    private readonly PictureBook _pictures;
    private IReadOnlyDictionary<string, string> _swaps = NoSwaps;
    private Vector3 _washColour;
    private float _wash;

    public CharacterLook(OutlineDescription outline, ToonShaders shaders, PictureBook pictures)
    {
        _shaders = shaders;
        _pictures = pictures;
        _line.Shader = shaders.Outline;
        _line.SetShaderParameter("line_color", outline.Colour.ToGodot());
        _line.SetShaderParameter("width", outline.Width);
    }

    public void Paint(MeshInstance3D mesh, PartDescription part)
    {
        mesh.CastShadow = Casting(part.Shadow);
        mesh.Layers = CharacterLayer.Mask;
        if (part.Shadow == ShadowRole.Only)
        {
            return;
        }

        if (mesh.Mesh.GetSurfaceCount() != part.Surfaces.Count)
        {
            throw new InvalidDataException($"Character: the part '{part.Name}' has {mesh.Mesh.GetSurfaceCount()} surfaces in its file, its description lists {part.Surfaces.Count}.");
        }

        List<ToonSurface> surfaces = new List<ToonSurface>();
        for (int index = 0; index < part.Surfaces.Count; index++)
        {
            ToonSurface surface = new ToonSurface(part.Surfaces[index], _shaders.Toon, _pictures);
            surface.Material.NextPass = part.Outline ? _line : null;
            surface.Show(_swaps);
            surface.Wash(_washColour, _wash);
            mesh.SetSurfaceOverrideMaterial(index, surface.Material);
            surfaces.Add(surface);
        }

        _painted[mesh] = surfaces;
    }

    public void Forget(MeshInstance3D mesh) => _painted.Remove(mesh);

    public void Show(IReadOnlyDictionary<string, string> swaps)
    {
        _swaps = swaps;
        foreach (List<ToonSurface> surfaces in _painted.Values)
        {
            foreach (ToonSurface surface in surfaces)
            {
                surface.Show(swaps);
            }
        }
    }

    public void Wash(Vector3 colour, float amount)
    {
        _washColour = colour;
        _wash = amount;
        _line.SetShaderParameter("wash_color", colour.ToGodot());
        _line.SetShaderParameter("wash", amount);
        foreach (List<ToonSurface> surfaces in _painted.Values)
        {
            foreach (ToonSurface surface in surfaces)
            {
                surface.Wash(colour, amount);
            }
        }
    }

    private static GeometryInstance3D.ShadowCastingSetting Casting(ShadowRole role) => role switch
    {
        ShadowRole.Only => GeometryInstance3D.ShadowCastingSetting.ShadowsOnly,
        ShadowRole.Off => GeometryInstance3D.ShadowCastingSetting.Off,
        _ => GeometryInstance3D.ShadowCastingSetting.On,
    };
}
