using GodotKey = Godot.Key;

namespace Evil.Engine.Input;

internal static class GodotKeys
{
    private static readonly Dictionary<GodotKey, Key> Map = Build();

    public static IReadOnlyDictionary<GodotKey, Key> All => Map;

    public static bool TryMap(GodotKey key, out Key mapped) => Map.TryGetValue(key, out mapped);

    private static Dictionary<GodotKey, Key> Build()
    {
        (GodotKey Godot, Key Ours)[] pairs =
        [
            (GodotKey.A, Key.A),
            (GodotKey.B, Key.B),
            (GodotKey.C, Key.C),
            (GodotKey.D, Key.D),
            (GodotKey.E, Key.E),
            (GodotKey.F, Key.F),
            (GodotKey.G, Key.G),
            (GodotKey.H, Key.H),
            (GodotKey.I, Key.I),
            (GodotKey.J, Key.J),
            (GodotKey.K, Key.K),
            (GodotKey.L, Key.L),
            (GodotKey.M, Key.M),
            (GodotKey.N, Key.N),
            (GodotKey.O, Key.O),
            (GodotKey.P, Key.P),
            (GodotKey.Q, Key.Q),
            (GodotKey.R, Key.R),
            (GodotKey.S, Key.S),
            (GodotKey.T, Key.T),
            (GodotKey.U, Key.U),
            (GodotKey.V, Key.V),
            (GodotKey.W, Key.W),
            (GodotKey.X, Key.X),
            (GodotKey.Y, Key.Y),
            (GodotKey.Z, Key.Z),
            (GodotKey.Key0, Key.D0),
            (GodotKey.Key1, Key.D1),
            (GodotKey.Key2, Key.D2),
            (GodotKey.Key3, Key.D3),
            (GodotKey.Key4, Key.D4),
            (GodotKey.Key5, Key.D5),
            (GodotKey.Key6, Key.D6),
            (GodotKey.Key7, Key.D7),
            (GodotKey.Key8, Key.D8),
            (GodotKey.Key9, Key.D9),
            (GodotKey.F1, Key.F1),
            (GodotKey.F2, Key.F2),
            (GodotKey.F3, Key.F3),
            (GodotKey.F4, Key.F4),
            (GodotKey.F5, Key.F5),
            (GodotKey.F6, Key.F6),
            (GodotKey.F7, Key.F7),
            (GodotKey.F8, Key.F8),
            (GodotKey.F9, Key.F9),
            (GodotKey.F10, Key.F10),
            (GodotKey.F11, Key.F11),
            (GodotKey.F12, Key.F12),
            (GodotKey.Up, Key.Up),
            (GodotKey.Down, Key.Down),
            (GodotKey.Left, Key.Left),
            (GodotKey.Right, Key.Right),
            (GodotKey.Space, Key.Space),
            (GodotKey.Enter, Key.Enter),
            (GodotKey.Escape, Key.Escape),
            (GodotKey.Tab, Key.Tab),
            (GodotKey.Backspace, Key.Backspace),
            (GodotKey.Delete, Key.Delete),
            (GodotKey.Insert, Key.Insert),
            (GodotKey.Home, Key.Home),
            (GodotKey.End, Key.End),
            (GodotKey.Pageup, Key.PageUp),
            (GodotKey.Pagedown, Key.PageDown),
            (GodotKey.Shift, Key.Shift),
            (GodotKey.Ctrl, Key.Control),
            (GodotKey.Alt, Key.Alt),
            (GodotKey.Minus, Key.Minus),
            (GodotKey.Equal, Key.Equal),
            (GodotKey.Comma, Key.Comma),
            (GodotKey.Period, Key.Period),
            (GodotKey.Slash, Key.Slash),
            (GodotKey.Semicolon, Key.Semicolon),
            (GodotKey.Apostrophe, Key.Apostrophe),
            (GodotKey.Bracketleft, Key.LeftBracket),
            (GodotKey.Bracketright, Key.RightBracket),
            (GodotKey.Backslash, Key.Backslash),
            (GodotKey.Quoteleft, Key.GraveAccent),
            (GodotKey.Kp0, Key.KeyPad0),
            (GodotKey.Kp1, Key.KeyPad1),
            (GodotKey.Kp2, Key.KeyPad2),
            (GodotKey.Kp3, Key.KeyPad3),
            (GodotKey.Kp4, Key.KeyPad4),
            (GodotKey.Kp5, Key.KeyPad5),
            (GodotKey.Kp6, Key.KeyPad6),
            (GodotKey.Kp7, Key.KeyPad7),
            (GodotKey.Kp8, Key.KeyPad8),
            (GodotKey.Kp9, Key.KeyPad9),
            (GodotKey.KpEnter, Key.KeyPadEnter),
            (GodotKey.KpAdd, Key.KeyPadAdd),
            (GodotKey.KpSubtract, Key.KeyPadSubtract),
            (GodotKey.KpMultiply, Key.KeyPadMultiply),
            (GodotKey.KpDivide, Key.KeyPadDivide),
            (GodotKey.KpPeriod, Key.KeyPadDecimal),
        ];
        Dictionary<GodotKey, Key> map = new Dictionary<GodotKey, Key>();
        foreach ((GodotKey godot, Key ours) in pairs)
        {
            map.Add(godot, ours);
        }

        return map;
    }
}
