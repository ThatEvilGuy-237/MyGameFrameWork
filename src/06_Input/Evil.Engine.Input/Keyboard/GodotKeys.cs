using GodotKey = Godot.Key;

namespace Evil.Engine.Input;

internal static class GodotKeys
{
    private const string KeyPadPrefix = "KeyPad";

    private static readonly Dictionary<GodotKey, Key> Map = Build();

    public static IReadOnlyDictionary<GodotKey, Key> All => Map;

    public static bool TryMap(GodotKey key, out Key mapped) => Map.TryGetValue(key, out mapped);

    private static Dictionary<GodotKey, Key> Build()
    {
        Dictionary<GodotKey, Key> map = new Dictionary<GodotKey, Key>();
        foreach (Key key in Enum.GetValues<Key>())
        {
            map.Add(Enum.Parse<GodotKey>(GodotName(key), ignoreCase: true), key);
        }

        return map;
    }

    private static string GodotName(Key key) => key switch
    {
        Key.Control => "Ctrl",
        Key.LeftBracket => "Bracketleft",
        Key.RightBracket => "Bracketright",
        Key.GraveAccent => "Quoteleft",
        Key.KeyPadDecimal => "KpPeriod",
        _ => SameNameInGodot(key.ToString()),
    };

    private static string SameNameInGodot(string name)
    {
        if (name.StartsWith(KeyPadPrefix, StringComparison.Ordinal))
        {
            return "Kp" + name[KeyPadPrefix.Length..];
        }

        bool isDigitRow = name.Length == 2 && name[0] == 'D' && char.IsAsciiDigit(name[1]);
        return isDigitRow ? "Key" + name[1] : name;
    }
}
