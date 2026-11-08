#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

shots="${EVIL_ENGINE_SHOTS:-documents/screenshots}"
mkdir -p "$shots"

echo "== build (warnings are errors)"
dotnet build EvilEngine.slnx -c Release -nologo -v q

echo "== tests"
dotnet tests/Evil.Engine.Tests/bin/Release/net10.0/Evil.Engine.Tests.dll | tail -n 1

sample="samples/Evil.Engine.Sample/bin/Release/net10.0/Evil.Engine.Sample.dll"
if ! command -v xvfb-run >/dev/null; then
  echo "== sample skipped: xvfb-run is not installed (run it on a desktop instead)"
  exit 0
fi

echo "== sample, still frame"
xvfb-run -a -s "-screen 0 1024x900x24" dotnet "$sample" --capture "$shots/sample.png" --frame 90
test -s "$shots/sample.png"
echo "saved $shots/sample.png"

if ! command -v xdotool >/dev/null || ! command -v import >/dev/null; then
  echo "== input drive skipped: needs xdotool and ImageMagick import"
  exit 0
fi

echo "== sample, driven with real keys and mouse"
xvfb-run -a -s "-screen 0 1024x900x24" bash scripts/drive.sh "$sample" "$shots/sample-driven.png"
test -s "$shots/sample-driven.png"
echo "saved $shots/sample-driven.png"
