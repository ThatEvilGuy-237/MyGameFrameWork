using Godot;

namespace Evil.Engine.Hosting;

internal sealed record ContentLocation(string Path, string Full)
{
    public bool IsImported => Full.StartsWith("res://", StringComparison.Ordinal) && ResourceLoader.Exists(Full);

    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

    public string Extension => System.IO.Path.GetExtension(Full).ToLowerInvariant();

    public static bool IsGodotPath(string path) =>
        path.StartsWith("res://", StringComparison.Ordinal) || path.StartsWith("user://", StringComparison.Ordinal);

    public static ContentLocation Resolve(string root, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (IsGodotPath(path) || System.IO.Path.IsPathRooted(path))
        {
            return new ContentLocation(path, path);
        }

        string full = IsGodotPath(root) ? $"{root.TrimEnd('/')}/{path}" : System.IO.Path.Combine(root, path);
        return new ContentLocation(path, full);
    }

    public void RequireFile()
    {
        if (!Godot.FileAccess.FileExists(Full))
        {
            throw new FileNotFoundException($"Content: '{Path}' was not found (looked at {Full}).", Full);
        }
    }

    public InvalidDataException Unreadable(string what) =>
        new InvalidDataException($"Content: '{Path}' could not be loaded as {what}.");

    public InvalidDataException Unreadable(string what, Error error) =>
        new InvalidDataException($"Content: '{Path}' could not be read as {what} ({error}).");
}
