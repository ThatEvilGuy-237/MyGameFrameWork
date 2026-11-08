using Evil.Engine.Textures;

namespace Evil.Engine.Graphics;

internal interface ITriangleSink
{
    void Submit(Texture2D texture, ReadOnlySpan<Vertex> vertices);
}
