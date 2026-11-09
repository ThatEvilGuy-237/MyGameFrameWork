#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

shots="${EVIL_ENGINE_SHOTS:-documents/screenshots}"
mkdir -p "$shots"
shots="$(cd "$shots" && pwd)"

echo "== code style (explicit types, constructor then statements)"
python3 -I scripts/style-check.py | tail -n 1

echo "== clean code (object oriented, no duplicates, may only get better)"
python3 -I scripts/clean-code-check.py

echo "== the Make a Game note shows the game template as it is"
python3 -I scripts/make-a-game.py

echo "== build (warnings are errors)"
dotnet build EvilEngine.slnx -nologo -v q

echo "== pure tests"
dotnet tests/Evil.Engine.Tests/bin/Debug/net10.0/Evil.Engine.Tests.dll | tail -n 1

echo "== Godot tests (inside Godot, headless)"
godot_tests="$(mktemp)"
if ! bash scripts/godot.sh --headless tests/Evil.Engine.GodotTests > "$godot_tests"; then
  grep -E "FAIL|passed" "$godot_tests" >&2 || cat "$godot_tests" >&2
  rm -f "$godot_tests"
  echo "Godot tests failed" >&2
  exit 1
fi
grep -E "FAIL|Godot tests passed" "$godot_tests"
if grep -q "leaked at exit" "$godot_tests"; then
  grep -E "leaked at exit|Pages in use" "$godot_tests" >&2
  rm -f "$godot_tests"
  echo "Godot reported leaked objects: a test or the engine does not free what it made" >&2
  exit 1
fi
rm -f "$godot_tests"

echo "== import OrbitDemo content (models, textures, fonts)"
bash scripts/godot.sh --headless samples/OrbitDemo --import > /dev/null

echo "== OrbitDemo in Godot, frame 90"
rm -f "$shots/orbit.png"
demo_log="$(mktemp)"
bash scripts/godot.sh samples/OrbitDemo -- --capture "$shots/orbit.png" --frame 90 | tee "$demo_log"
if grep -qE "leaked at exit|still in use at exit" "$demo_log"; then
  echo "the demo left Godot objects behind at exit" >&2
  rm -f "$demo_log"
  exit 1
fi
if ! grep -qE '^Evil Engine [0-9.]+: "Evil Engine orbit demo" 1280x720, first scene OrbitScene' "$demo_log"; then
  echo "the engine did not say it started (the line a game prints first)" >&2
  rm -f "$demo_log"
  exit 1
fi
rm -f "$demo_log"
test -s "$shots/orbit.png" || { echo "no frame was captured" >&2; exit 1; }
echo "saved $shots/orbit.png"

if command -v xdotool >/dev/null && command -v xvfb-run >/dev/null; then
  echo "== OrbitDemo driven with real keys and mouse"
  drive_log="$(mktemp)"
  drive_audio="$(mktemp --suffix .wav)"
  rm -f "$shots/orbit-driven.png" "$drive_audio"
  if ! xvfb-run -a -s "-screen 0 1600x900x24" bash scripts/drive-orbit.sh "$shots/orbit-driven.png" "$drive_log" "$drive_audio"; then
    cat "$drive_log" >&2
    rm -f "$drive_log"
    exit 1
  fi
  for seen in "spinning: False" "selected: pawn" "zoom:" "screen: 1024x640" "arrived: pawn" "action: Attack" "sound: hit" "done: attack" "music muted: True" "recorded:"; do
    if ! grep -q "$seen" "$drive_log"; then
      echo "the driven run never showed '$seen'" >&2
      cat "$drive_log" >&2
      rm -f "$drive_log"
      exit 1
    fi
  done
  grep -E "spinning: False|selected: pawn|zoom:|screen:|action:|arrived:|done:|sound:|muted:" "$drive_log" | head -n 14
  if grep -qE "leaked at exit|still in use at exit" "$drive_log"; then
    echo "the driven demo left Godot objects behind at exit" >&2
    rm -f "$drive_log" "$drive_audio"
    exit 1
  fi
  rm -f "$drive_log"
  test -s "$shots/orbit-driven.png" || { echo "no driven frame was captured" >&2; exit 1; }
  python3 -I scripts/audio-level.py "$drive_audio" || { rm -f "$drive_audio"; exit 1; }
  rm -f "$drive_audio"
  echo "saved $shots/orbit-driven.png"
fi
