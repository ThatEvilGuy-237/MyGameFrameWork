using Godot;
using EvilTexture = Evil.Engine.World.Texture;

namespace Evil.Engine.Hosting;

internal sealed class TextureLoader : ContentLoader<EvilTexture, Texture2D>
{
    protected override string Kind => "a texture";

    protected override Texture2D ReadFile(ContentLocation location)
    {
        Image image = Image.LoadFromFile(location.Full) ?? throw location.Unreadable("an image");
        return ImageTexture.CreateFromImage(image);
    }

    protected override EvilTexture Wrap(Texture2D native, ContentLocation location) => new EvilTexture(native, location.Path);

    protected override Texture2D NativeOf(EvilTexture content) => content.Native;
}
