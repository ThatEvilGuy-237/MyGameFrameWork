using Godot;

namespace Evil.Engine.Characters;

internal sealed class SkinPatches
{
    private const int Corners = 3;

    private readonly MeshInstance3D _skin;
    private readonly ArrayMesh _whole;
    private readonly Godot.Collections.Array _arrays;
    private readonly int[] _order;
    private readonly Vector2[] _patches;

    public SkinPatches(MeshInstance3D skin)
    {
        _skin = skin;
        _whole = skin.Mesh as ArrayMesh ?? throw new InvalidDataException($"Character: the skin '{skin.Name}' is not a mesh that can be cut.");
        if (_whole.GetSurfaceCount() != 1 || _whole.GetBlendShapeCount() > 0)
        {
            throw new InvalidDataException($"Character: the skin '{skin.Name}' must be one surface without shapes to hide under clothing.");
        }

        _arrays = _whole.SurfaceGetArrays(0);
        _order = _arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
        _patches = _arrays[(int)Mesh.ArrayType.TexUV2].AsVector2Array();
        Triangles = _order.Length / Corners;
    }

    public int Triangles { get; private set; }

    public void Hide(IReadOnlySet<int> hidden)
    {
        int[] kept = hidden.Count == 0 || _patches.Length == 0 ? _order : Kept(hidden);
        Triangles = kept.Length / Corners;
        _skin.Visible = kept.Length > 0;
        if (kept.Length == _order.Length)
        {
            _skin.Mesh = _whole;
        }
        else if (kept.Length > 0)
        {
            _skin.Mesh = Cut(kept);
        }
    }

    private int[] Kept(IReadOnlySet<int> hidden)
    {
        List<int> kept = new List<int>(_order.Length);
        for (int corner = 0; corner < _order.Length; corner += Corners)
        {
            if (!hidden.Contains((int)MathF.Round(_patches[_order[corner]].X)))
            {
                kept.Add(_order[corner]);
                kept.Add(_order[corner + 1]);
                kept.Add(_order[corner + 2]);
            }
        }

        return kept.ToArray();
    }

    private ArrayMesh Cut(int[] kept)
    {
        Godot.Collections.Array arrays = _arrays.Duplicate();
        arrays[(int)Mesh.ArrayType.Index] = kept;
        ArrayMesh shown = new ArrayMesh();
        shown.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays, flags: _whole.SurfaceGetFormat(0) & Mesh.ArrayFormat.FlagUse8BoneWeights);
        return shown;
    }
}
