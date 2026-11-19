using Evil.Engine.World;
using Godot;

namespace Evil.Engine.Characters;

public sealed class CharacterModel : WorldObject
{
    private const float FacingForward = 180f;

    private readonly CharacterDescription _told;
    private readonly CharacterLook _look;
    private readonly Wardrobe _wardrobe;
    private readonly FaceTicker _faces;

    internal CharacterModel(CharacterDescription told, ICharacterFiles files, SurfaceShader shader)
        : base(new Node3D(), told.Name)
    {
        _told = told;
        Source = files.Source;
        Node.Name = told.Name;
        LoadedScene body = new LoadedScene(files.Scene(told.Body.File), files.Source);
        Node3D pivot = new Node3D();
        pivot.Name = "Pivot";
        pivot.RotationDegrees = new Vector3(0f, FacingForward, 0f);
        pivot.Scale = new Vector3(told.Scale, told.Scale, told.Scale);
        Node.AddChild(pivot);
        pivot.AddChild(body.Root);
        _look = new CharacterLook(shader, new PictureBook(told.Pictures, files));
        foreach (PartDescription part in told.Body.Parts)
        {
            _look.Paint(body.Mesh(part.Name), part);
        }

        _wardrobe = new Wardrobe(told, files, body, _look);
        Clips = new CharacterClips(body.Player, told.Clips, files.Source);
        _faces = new FaceTicker(Clips, _look, told.Expressions);
        _faces.Name = "Faces";
        Node.AddChild(_faces);
    }

    public string Source { get; }

    public float Height => _told.Height * _told.Scale;

    public IReadOnlyList<string> Clothing => [.. _told.Clothing.Keys];

    public IReadOnlyList<string> Outfits => [.. _told.Outfits.Keys];

    public IReadOnlyList<string> Worn => _wardrobe.Worn;

    public CharacterClips Clips { get; }

    internal int SkinTriangles => _wardrobe.SkinTriangles;

    internal void ShowFaceNow() => _faces.Tick();

    public void Wear(string piece)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(piece);
        RequireAlive();
        _wardrobe.Wear(piece);
    }

    public void WearOutfit(string outfit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outfit);
        if (!_told.Outfits.TryGetValue(outfit, out IReadOnlyList<string>? pieces))
        {
            throw new ArgumentException($"{_told.Name} has no outfit named '{outfit}'. It has: {string.Join(", ", _told.Outfits.Keys)}.", nameof(outfit));
        }

        foreach (string piece in pieces)
        {
            Wear(piece);
        }
    }

    public void TakeOff(string piece)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(piece);
        RequireAlive();
        _wardrobe.TakeOff(piece);
    }
}
