#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

shots="${EVIL_ENGINE_SHOTS:-documents/screenshots}"
mkdir -p "$shots"
shots="$(cd "$shots" && pwd)"

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

echo "== OrbitDemo in Godot, frame 90"
rm -f "$shots/orbit.png"
bash scripts/godot.sh samples/OrbitDemo -- --capture "$shots/orbit.png" --frame 90
test -s "$shots/orbit.png" || { echo "no frame was captured" >&2; exit 1; }
echo "saved $shots/orbit.png"

if command -v xdotool >/dev/null && command -v xvfb-run >/dev/null; then
  echo "== OrbitDemo driven with real keys and mouse"
  drive_log="$(mktemp)"
  rm -f "$shots/orbit-driven.png"
  if ! xvfb-run -a -s "-screen 0 1600x900x24" bash scripts/drive-orbit.sh "$shots/orbit-driven.png" "$drive_log"; then
    cat "$drive_log" >&2
    rm -f "$drive_log"
    exit 1
  fi
  for seen in "spinning: False" "selected: pawn" "zoom:" "screen: 1024x640" "action: Attack"; do
    if ! grep -q "$seen" "$drive_log"; then
      echo "the driven run never showed '$seen'" >&2
      cat "$drive_log" >&2
      rm -f "$drive_log"
      exit 1
    fi
  done
  grep -E "spinning: False|selected: pawn|zoom:|screen:|action:" "$drive_log" | head -n 8
  rm -f "$drive_log"
  test -s "$shots/orbit-driven.png" || { echo "no driven frame was captured" >&2; exit 1; }
  echo "saved $shots/orbit-driven.png"
fi
