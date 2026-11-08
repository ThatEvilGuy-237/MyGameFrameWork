#!/usr/bin/env bash
set -euo pipefail
sample="$1"
out="$2"

dotnet "$sample" --capture "$out" --frame 420 &
game=$!
trap 'kill "$game" 2>/dev/null || true' EXIT

window=""
for _ in $(seq 1 100); do
  window="$(xdotool search --name "Evil Engine sample" 2>/dev/null | head -n 1 || true)"
  [ -n "$window" ] && break
  sleep 0.1
done
[ -n "$window" ] || { echo "the sample window never opened" >&2; exit 1; }

sleep 2
xdotool windowfocus "$window"
xdotool mousemove 400 400
xdotool key space
sleep 0.5

star="$(import -window root -depth 8 ppm:- | python3 -I "$(dirname "$0")/find-color.py" 255 255 0)"
echo "yellow star at $star"
xdotool mousemove $star
sleep 0.5
xdotool key p
wait "$game"
trap - EXIT
