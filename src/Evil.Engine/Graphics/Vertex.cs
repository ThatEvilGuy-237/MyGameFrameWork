using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Evil.Engine.Graphics;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct Vertex(Vector2 Position, Vector2 TexCoord, Color Color)
{
    public static readonly int SizeInBytes = Unsafe.SizeOf<Vertex>();
    public const int PositionOffset = 0;
    public const int TexCoordOffset = 8;
    public const int ColorOffset = 16;
}
