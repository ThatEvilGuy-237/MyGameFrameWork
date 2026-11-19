using Evil.Engine.Characters;
using Godot;

namespace Evil.Engine.Hosting;

internal sealed class CharacterFiles : ICharacterFiles
{
    private readonly ContentLocation _folder;
    private readonly SceneLoader _scenes;
    private readonly TextureLoader _textures;

    public CharacterFiles(ContentLocation folder, SceneLoader scenes, TextureLoader textures)
    {
        _folder = folder;
        _scenes = scenes;
        _textures = textures;
    }

    public string Source => _folder.Path;

    public Node3D Scene(string file)
    {
        ContentLocation location = _folder.Inside(file);
        return _scenes.Load(location).Instantiate() as Node3D ?? throw location.Unreadable("a 3D model");
    }

    public Texture2D Picture(string file) => _textures.Load(_folder.Inside(file)).Native;
}
