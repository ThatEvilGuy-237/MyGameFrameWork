using Godot;
using EvilModel = Evil.Engine.World.Model;

namespace Evil.Engine.Hosting;

internal sealed class ModelLoader
{
    private readonly SceneLoader _scenes = new SceneLoader();

    public EvilModel Load(ContentLocation location)
    {
        PackedScene scene = _scenes.Load(location);
        Node3D root = scene.Instantiate() as Node3D ?? throw location.Unreadable("a 3D model");
        return EvilModel.Wrap(root, location.Name, location.Full);
    }

    public void Close() => _scenes.Close();
}
