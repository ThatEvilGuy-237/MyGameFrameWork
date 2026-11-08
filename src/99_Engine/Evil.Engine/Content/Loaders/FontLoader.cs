using Godot;
using EvilFont = Evil.Engine.Screen.Font;

namespace Evil.Engine.Hosting;

internal sealed class FontLoader : ContentLoader<EvilFont, Godot.Font>
{
    protected override string Kind => "a font";

    protected override Godot.Font ReadFile(ContentLocation location)
    {
        FontFile file = new FontFile();
        Error error = file.LoadDynamicFont(location.Full);
        if (error != Error.Ok)
        {
            file.Dispose();
            throw location.Unreadable(Kind, error);
        }

        return file;
    }

    protected override EvilFont Wrap(Godot.Font native, ContentLocation location) => new EvilFont(native, location.Path);

    protected override Godot.Font NativeOf(EvilFont content) => content.Native;
}
