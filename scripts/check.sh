#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

shots="${EVIL_ENGINE_SHOTS:-documents/screenshots}"
mkdir -p "$shots"
shots="$(cd "$shots" && pwd)"

echo "== build (warnings are errors)"
dotnet build EvilEngine.slnx -nologo -v q

echo "== tests"
dotnet tests/Evil.Engine.Tests/bin/Debug/net10.0/Evil.Engine.Tests.dll | tail -n 1

echo "== OrbitDemo in Godot, frame 90"
rm -f "$shots/orbit.png"
bash scripts/godot.sh samples/OrbitDemo -- --capture "$shots/orbit.png" --frame 90
test -s "$shots/orbit.png" || { echo "no frame was captured" >&2; exit 1; }
echo "saved $shots/orbit.png"
