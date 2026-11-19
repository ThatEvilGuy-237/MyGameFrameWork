namespace Evil.Engine.GodotTests.Content;

internal sealed class ModelFile
{
    private readonly string _path;

    public ModelFile(string path) => _path = path;

    public void Write(Godot.Node3D root)
    {
        Godot.GltfDocument document = new Godot.GltfDocument();
        Godot.GltfState state = new Godot.GltfState();
        Godot.Error appended = document.AppendFromScene(root, state);
        Godot.Error written = document.WriteToFilesystem(state, _path);
        root.Free();
        if (appended != Godot.Error.Ok || written != Godot.Error.Ok)
        {
            throw new CheckFailed($"could not write the test model {_path}: {appended}, {written}");
        }
    }
}
