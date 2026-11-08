import math
import struct
import sys
import wave

with wave.open(sys.argv[1]) as recording:
    channels = recording.getnchannels()
    width = recording.getsampwidth()
    rate = recording.getframerate()
    frames = recording.readframes(recording.getnframes())

if width != 2:
    sys.exit(f"expected 16-bit audio, got {width * 8}-bit")

samples = struct.unpack(f"<{len(frames) // 2}h", frames)
seconds = len(samples) / channels / rate
rms = math.sqrt(sum(s * s for s in samples) / max(1, len(samples))) / 32768
loudest = max((abs(s) for s in samples), default=0) / 32768
print(f"recorded {seconds:.1f} s of game audio: level {rms:.4f}, loudest {loudest:.3f}")
if seconds < 1 or rms < 0.002:
    sys.exit("the game audio was silent")
