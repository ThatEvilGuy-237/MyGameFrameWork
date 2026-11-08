import sys

target = tuple(int(value) for value in sys.argv[1:4])
data = sys.stdin.buffer.read()

fields = []
position = 0
while len(fields) < 4:
    while data[position:position + 1].isspace():
        position += 1
    if data[position:position + 1] == b"#":
        position = data.index(b"\n", position) + 1
        continue
    end = position
    while not data[end:end + 1].isspace():
        end += 1
    fields.append(data[position:end])
    position = end
position += 1

if fields[0] != b"P6" or int(fields[3]) != 255:
    sys.exit("expected an 8-bit binary PPM")

width = int(fields[1])
pixels = data[position:]
xs, ys = 0, 0
count = 0
for index in range(0, len(pixels) - 2, 3):
    if all(abs(pixels[index + channel] - target[channel]) <= 8 for channel in range(3)):
        pixel = index // 3
        xs += pixel % width
        ys += pixel // width
        count += 1

if count == 0:
    sys.exit(f"no pixel of colour {target} on screen")

print(f"{xs // count} {ys // count}")
