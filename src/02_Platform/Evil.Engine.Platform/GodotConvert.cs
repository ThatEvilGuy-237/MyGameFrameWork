using Evil.Engine.Core;
using GodotColor = Godot.Color;
using GodotQuaternion = Godot.Quaternion;
using GodotVector2 = Godot.Vector2;
using GodotVector3 = Godot.Vector3;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Evil.Engine.Platform;

internal static class GodotConvert
{
    public static GodotVector3 ToGodot(this Vector3 value) => new GodotVector3(value.X, value.Y, value.Z);

    public static Vector3 ToEvil(this GodotVector3 value) => new Vector3(value.X, value.Y, value.Z);

    public static GodotVector2 ToGodot(this Vector2 value) => new GodotVector2(value.X, value.Y);

    public static Vector2 ToEvil(this GodotVector2 value) => new Vector2(value.X, value.Y);

    public static GodotQuaternion ToGodot(this Quaternion value) => new GodotQuaternion(value.X, value.Y, value.Z, value.W);

    public static Quaternion ToEvil(this GodotQuaternion value) => new Quaternion(value.X, value.Y, value.Z, value.W);

    public static GodotColor ToGodot(this Color value) => new GodotColor(value.R, value.G, value.B, value.A);

    public static Color ToEvil(this GodotColor value) => new Color(value.R, value.G, value.B, value.A);
}
