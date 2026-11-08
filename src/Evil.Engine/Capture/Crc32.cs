namespace Evil.Engine.Capture;

internal static class Crc32
{
    private static readonly uint[] Table = BuildTable();

    public static uint Compute(ReadOnlySpan<byte> data) => Finish(Update(Start, data));

    public const uint Start = 0xFFFFFFFFu;

    public static uint Update(uint crc, ReadOnlySpan<byte> data)
    {
        foreach (var value in data)
        {
            crc = Table[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    public static uint Finish(uint crc) => crc ^ 0xFFFFFFFFu;

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < table.Length; n++)
        {
            var value = n;
            for (var bit = 0; bit < 8; bit++)
            {
                value = (value & 1) != 0 ? 0xEDB88320u ^ (value >> 1) : value >> 1;
            }

            table[n] = value;
        }

        return table;
    }
}
