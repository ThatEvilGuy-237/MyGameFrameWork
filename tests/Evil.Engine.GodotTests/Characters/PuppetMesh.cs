namespace Evil.Engine.GodotTests.Characters;

internal sealed class PuppetMesh
{
    private readonly int _quads;
    private readonly int _bone;

    public PuppetMesh(int quads, int bone)
    {
        _quads = quads;
        _bone = bone;
    }

    public Godot.ArrayMesh Plain()
    {
        Godot.ArrayMesh mesh = new Godot.ArrayMesh();
        mesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, Arrays(0f));
        return mesh;
    }

    public Godot.ArrayMesh Shaped(string shape)
    {
        Godot.ArrayMesh mesh = new Godot.ArrayMesh();
        mesh.AddBlendShape(shape);
        Godot.Collections.Array moved = new Godot.Collections.Array();
        moved.Resize((int)Godot.Mesh.ArrayType.Max);
        moved[(int)Godot.Mesh.ArrayType.Vertex] = Corners(0.5f);
        moved[(int)Godot.Mesh.ArrayType.Normal] = Facing();
        Godot.Collections.Array<Godot.Collections.Array> shapes = new Godot.Collections.Array<Godot.Collections.Array>();
        shapes.Add(moved);
        mesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, Arrays(0f), shapes);
        return mesh;
    }

    private Godot.Collections.Array Arrays(float lift)
    {
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Godot.Mesh.ArrayType.Max);
        arrays[(int)Godot.Mesh.ArrayType.Vertex] = Corners(lift);
        arrays[(int)Godot.Mesh.ArrayType.Normal] = Facing();
        arrays[(int)Godot.Mesh.ArrayType.TexUV] = Spread(false);
        arrays[(int)Godot.Mesh.ArrayType.TexUV2] = Spread(true);
        arrays[(int)Godot.Mesh.ArrayType.Bones] = Bones();
        arrays[(int)Godot.Mesh.ArrayType.Weights] = Weights();
        arrays[(int)Godot.Mesh.ArrayType.Index] = Order();
        return arrays;
    }

    private Godot.Vector3[] Corners(float lift)
    {
        Godot.Vector3[] corners = new Godot.Vector3[_quads * 4];
        for (int quad = 0; quad < _quads; quad++)
        {
            corners[quad * 4] = new Godot.Vector3(quad, lift, 0f);
            corners[quad * 4 + 1] = new Godot.Vector3(quad + 1f, lift, 0f);
            corners[quad * 4 + 2] = new Godot.Vector3(quad + 1f, 1f + lift, 0f);
            corners[quad * 4 + 3] = new Godot.Vector3(quad, 1f + lift, 0f);
        }

        return corners;
    }

    private Godot.Vector3[] Facing()
    {
        Godot.Vector3[] normals = new Godot.Vector3[_quads * 4];
        Array.Fill(normals, Godot.Vector3.Back);
        return normals;
    }

    private Godot.Vector2[] Spread(bool patches)
    {
        Godot.Vector2[] spread = new Godot.Vector2[_quads * 4];
        for (int corner = 0; corner < spread.Length; corner++)
        {
            spread[corner] = patches ? new Godot.Vector2(corner / 4 + 1, 0f) : new Godot.Vector2(corner % 2, corner / 2 % 2);
        }

        return spread;
    }

    private int[] Bones()
    {
        int[] bones = new int[_quads * 16];
        for (int corner = 0; corner < _quads * 4; corner++)
        {
            bones[corner * 4] = _bone;
        }

        return bones;
    }

    private float[] Weights()
    {
        float[] weights = new float[_quads * 16];
        for (int corner = 0; corner < _quads * 4; corner++)
        {
            weights[corner * 4] = 1f;
        }

        return weights;
    }

    private int[] Order()
    {
        int[] order = new int[_quads * 6];
        int[] quad = [0, 1, 2, 0, 2, 3];
        for (int corner = 0; corner < order.Length; corner++)
        {
            order[corner] = corner / 6 * 4 + quad[corner % 6];
        }

        return order;
    }
}
