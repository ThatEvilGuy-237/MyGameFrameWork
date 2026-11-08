using EvilFont = Evil.Engine.Screen.Font;
using EvilModel = Evil.Engine.World.Model;
using EvilSound = Evil.Engine.Audio.Sound;
using EvilTexture = Evil.Engine.World.Texture;

namespace Evil.Engine.Hosting;

public sealed class ContentLibrary
{
    private readonly ModelLoader _models = new ModelLoader();
    private readonly TextureLoader _textures = new TextureLoader();
    private readonly FontLoader _fonts = new FontLoader();
    private readonly SoundLoader _sounds = new SoundLoader();

    internal ContentLibrary(string root) => Root = root;

    public string Root { get; }

    public EvilModel Model(string path) => _models.Load(Locate(path));

    public EvilTexture Texture(string path) => _textures.Load(Locate(path));

    public EvilFont Font(string path) => _fonts.Load(Locate(path));

    public EvilSound Sound(string path) => _sounds.Load(Locate(path));

    internal void Close()
    {
        _sounds.Close();
        _textures.Close();
        _fonts.Close();
        _models.Close();
    }

    private ContentLocation Locate(string path) => ContentLocation.Resolve(Root, path);
}
