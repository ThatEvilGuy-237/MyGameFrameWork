namespace Evil.Engine.Characters;

internal sealed class PictureBook
{
    private readonly IReadOnlyDictionary<string, PictureDescription> _pictures;
    private readonly ICharacterFiles _files;

    public PictureBook(IReadOnlyDictionary<string, PictureDescription> pictures, ICharacterFiles files)
    {
        _pictures = pictures;
        _files = files;
    }

    public LookPicture Of(string name)
    {
        if (!_pictures.TryGetValue(name, out PictureDescription? picture))
        {
            throw new InvalidDataException($"Character: {_files.Source} names the picture '{name}' but does not list it.");
        }

        return new LookPicture(_files.Picture(picture.File), picture.Region);
    }
}
