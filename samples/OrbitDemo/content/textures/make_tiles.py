import struct
import sys
import zlib

size = 64
pixels = []
for y in range(size):
    row = []
    for x in range(size):
        grout = x % 32 in (0, 31) or y % 32 in (0, 31)
        tile = ((x // 32) + (y // 32)) % 2
        noise = ((x * 7 + y * 13) % 9) - 4
        base = (70, 92, 92) if tile == 0 else (82, 104, 100)
        colour = (40, 52, 52) if grout else tuple(max(0, min(255, c + noise)) for c in base)
        row.append(colour)
    pixels.append(row)

raw = b"".join(b"\x00" + bytes(c for p in row for c in p) for row in pixels)


def chunk(kind, data):
    return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF)


png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", size, size, 8, 2, 0, 0, 0)) + chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")
open(sys.argv[1], "wb").write(png)
