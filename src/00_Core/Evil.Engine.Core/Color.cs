using System.Globalization;

namespace Evil.Engine.Core;

public readonly record struct Color(float R, float G, float B, float A = 1f)
{
    public static Color White => new(1f, 1f, 1f);
    public static Color Black => new(0f, 0f, 0f);
    public static Color Transparent => new(0f, 0f, 0f, 0f);
    public static Color Red => new(1f, 0f, 0f);
    public static Color Green => new(0f, 1f, 0f);
    public static Color Blue => new(0f, 0f, 1f);
    public static Color Yellow => new(1f, 1f, 0f);
    public static Color Gray => new(0.5f, 0.5f, 0.5f);

    public static Color FromBytes(byte r, byte g, byte b, byte a = 255) =>
        new(r / 255f, g / 255f, b / 255f, a / 255f);

    public static Color FromHex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);
        var digits = hex.StartsWith('#') ? hex[1..] : hex;
        if ((digits.Length != 6 && digits.Length != 8)
            || !uint.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"Color.FromHex needs #RRGGBB or #RRGGBBAA, got '{hex}'.");
        }

        if (digits.Length == 6)
        {
            value = (value << 8) | 0xFF;
        }

        return FromBytes((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    public Color WithAlpha(float alpha) => this with { A = alpha };

    public Color Multiply(Color other) => new(R * other.R, G * other.G, B * other.B, A * other.A);

    public static Color Lerp(Color from, Color to, float amount) =>
        new(
            from.R + (to.R - from.R) * amount,
            from.G + (to.G - from.G) * amount,
            from.B + (to.B - from.B) * amount,
            from.A + (to.A - from.A) * amount);

    public string ToHex() =>
        $"#{ToByte(R):X2}{ToByte(G):X2}{ToByte(B):X2}{ToByte(A):X2}";

    private static byte ToByte(float channel) => (byte)MathF.Round(Math.Clamp(channel, 0f, 1f) * 255f);
}
