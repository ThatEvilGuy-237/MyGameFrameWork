using Evil.Engine.Characters;

namespace Evil.Engine.Hosting;

internal sealed class CharacterLoader
{
    private const string DescriptionFile = "character.json";

    private readonly Dictionary<string, CharacterDescription> _told = new Dictionary<string, CharacterDescription>(StringComparer.Ordinal);
    private readonly SceneLoader _scenes = new SceneLoader();
    private readonly TextureLoader _textures;
    private SurfaceShader? _shader;

    public CharacterLoader(TextureLoader textures) => _textures = textures;

    public CharacterModel Load(ContentLocation folder)
    {
        _shader ??= new SurfaceShader();
        return new CharacterModel(Describe(folder), new CharacterFiles(folder, _scenes, _textures), _shader);
    }

    public void Close()
    {
        _told.Clear();
        _scenes.Close();
        _shader?.Dispose();
        _shader = null;
    }

    private CharacterDescription Describe(ContentLocation folder)
    {
        if (_told.TryGetValue(folder.Full, out CharacterDescription? known))
        {
            return known;
        }

        ContentLocation file = folder.Inside(DescriptionFile);
        file.RequireFile();
        CharacterDescription told = new DescriptionReader(file.Path).Read(Godot.FileAccess.GetFileAsString(file.Full));
        _told[folder.Full] = told;
        return told;
    }
}
