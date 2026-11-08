using Godot;

namespace Evil.Engine.Hosting;

internal sealed class SceneLoader : ContentLoader<PackedScene, PackedScene>
{
    protected override string Kind => "a model";

    protected override PackedScene ReadFile(ContentLocation location)
    {
        GltfDocument document = new GltfDocument();
        GltfState state = new GltfState();
        Error error = document.AppendFromFile(location.Full, state);
        if (error != Error.Ok)
        {
            throw location.Unreadable("a glTF model", error);
        }

        Node generated = document.GenerateScene(state) ?? throw location.Unreadable("a 3D model");
        PackedScene scene = new PackedScene();
        Error packed = scene.Pack(generated);
        generated.Free();
        return packed == Error.Ok ? scene : throw location.Unreadable("a 3D model", packed);
    }

    protected override PackedScene Wrap(PackedScene native, ContentLocation location) => native;

    protected override PackedScene NativeOf(PackedScene content) => content;
}
