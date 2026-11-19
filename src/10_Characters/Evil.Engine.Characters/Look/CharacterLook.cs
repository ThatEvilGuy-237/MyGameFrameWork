using Godot;

namespace Evil.Engine.Characters;

internal sealed class CharacterLook
{
    private static readonly IReadOnlyDictionary<string, string> NoSwaps = new Dictionary<string, string>();

    private readonly Dictionary<MeshInstance3D, List<SurfaceLook>> _painted = new Dictionary<MeshInstance3D, List<SurfaceLook>>();
    private readonly SurfaceShader _shader;
    private readonly PictureBook _pictures;
    private IReadOnlyDictionary<string, string> _swaps = NoSwaps;

    public CharacterLook(SurfaceShader shader, PictureBook pictures)
    {
        _shader = shader;
        _pictures = pictures;
    }

    public void Paint(MeshInstance3D mesh, PartDescription part)
    {
        if (part.Shadow == ShadowRole.Only)
        {
            mesh.Visible = false;
            return;
        }

        if (mesh.Mesh.GetSurfaceCount() != part.Surfaces.Count)
        {
            throw new InvalidDataException($"Character: the part '{part.Name}' has {mesh.Mesh.GetSurfaceCount()} surfaces in its file, its description lists {part.Surfaces.Count}.");
        }

        mesh.CastShadow = part.Shadow == ShadowRole.Off ? GeometryInstance3D.ShadowCastingSetting.Off : GeometryInstance3D.ShadowCastingSetting.On;
        List<SurfaceLook> surfaces = new List<SurfaceLook>();
        for (int index = 0; index < part.Surfaces.Count; index++)
        {
            SurfaceLook surface = new SurfaceLook(part.Surfaces[index], _shader.Native, _pictures);
            surface.Show(_swaps);
            mesh.SetSurfaceOverrideMaterial(index, surface.Material);
            surfaces.Add(surface);
        }

        _painted[mesh] = surfaces;
    }

    public void Forget(MeshInstance3D mesh) => _painted.Remove(mesh);

    public void Show(IReadOnlyDictionary<string, string> swaps)
    {
        _swaps = swaps;
        foreach (List<SurfaceLook> surfaces in _painted.Values)
        {
            foreach (SurfaceLook surface in surfaces)
            {
                surface.Show(swaps);
            }
        }
    }
}
