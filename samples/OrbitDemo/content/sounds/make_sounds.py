import math
import random
import struct
import sys
import wave

RATE = 22050
folder = sys.argv[1]
random.seed(7)


def save(name, samples):
    with wave.open(f"{folder}/{name}.wav", "wb") as out:
        out.setnchannels(1)
        out.setsampwidth(2)
        out.setframerate(RATE)
        clipped = (max(-1.0, min(1.0, s)) for s in samples)
        out.writeframes(b"".join(struct.pack("<h", int(s * 32000)) for s in clipped))


def click():
    count = int(RATE * 0.05)
    return [0.6 * math.sin(2 * math.pi * 1400 * i / RATE) * math.exp(-i / (RATE * 0.008)) for i in range(count)]


def hop():
    count = int(RATE * 0.18)
    phase, out = 0.0, []
    for i in range(count):
        t = i / count
        phase += 2 * math.pi * (260 + 360 * t) / RATE
        out.append(0.5 * math.sin(phase) * (1 - t) ** 1.5)
    return out


def hit():
    count = int(RATE * 0.22)
    out = []
    for i in range(count):
        t = i / RATE
        decay = math.exp(-t / 0.05)
        out.append(0.45 * random.uniform(-1, 1) * decay + 0.6 * math.sin(2 * math.pi * 90 * t) * math.exp(-t / 0.08))
    return out


def music():
    chords = [(261.63, 329.63, 392.00), (220.00, 261.63, 329.63), (174.61, 220.00, 261.63), (196.00, 246.94, 293.66)]
    beat = RATE // 4
    out = [0.0] * (len(chords) * 4 * beat)
    for c, chord in enumerate(chords):
        for step in range(4):
            note = chord[step % 3] * (2 if step == 3 else 1)
            start = (c * 4 + step) * beat
            for i in range(beat * 2):
                if start + i >= len(out):
                    break
                t = i / RATE
                envelope = math.exp(-t / 0.35)
                tone = math.sin(2 * math.pi * note * t) + 0.3 * math.sin(4 * math.pi * note * t)
                out[start + i] += 0.18 * tone * envelope
            for i in range(len(out) // len(chords) if step == 0 else 0):
                t = i / RATE
                out[start + i] += 0.07 * math.sin(2 * math.pi * chord[0] / 2 * t)
    edge = int(RATE * 0.02)
    for i in range(edge):
        out[i] *= i / edge
        out[-1 - i] *= i / edge
    return out


save("click", click())
save("hop", hop())
save("hit", hit())
save("music", music())
