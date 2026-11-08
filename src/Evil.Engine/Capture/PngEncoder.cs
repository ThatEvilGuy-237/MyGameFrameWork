using System.Buffers.Binary;
using System.IO.Compression;

namespace Evil.Engine.Capture;

public static class PngEncoder
{
    private static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];

    public static byte[] Encode(int width, int height, ReadOnlySpan<byte> rgba)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        if (rgba.Length != width * height * 4)
        {
            throw new ArgumentException($"A {width}x{height} image needs {width * height * 4} bytes of RGBA, got {rgba.Length}.", nameof(rgba));
        }

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header[4..], height);
        header[8] = 8;
        header[9] = 6;

        using var output = new MemoryStream();
        output.Write(Signature);
        WriteChunk(output, "IHDR"u8, header);
        WriteChunk(output, "IDAT"u8, Compress(width, height, rgba));
        WriteChunk(output, "IEND"u8, []);
        return output.ToArray();
    }

    public static void Save(string path, int width, int height, ReadOnlySpan<byte> rgba)
    {
        var folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllBytes(path, Encode(width, height, rgba));
    }

    private static byte[] Compress(int width, int height, ReadOnlySpan<byte> rgba)
    {
        var stride = width * 4;
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                zlib.WriteByte(0);
                zlib.Write(rgba.Slice(y * stride, stride));
            }
        }

        return compressed.ToArray();
    }

    private static void WriteChunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> number = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(number, data.Length);
        output.Write(number);
        output.Write(type);
        output.Write(data);
        BinaryPrimitives.WriteUInt32BigEndian(number, Crc32.Finish(Crc32.Update(Crc32.Update(Crc32.Start, type), data)));
        output.Write(number);
    }
}
