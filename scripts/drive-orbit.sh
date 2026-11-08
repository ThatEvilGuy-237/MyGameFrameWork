#!/usr/bin/env bash
set -uo pipefail
out="$1"
log="$2"
cd "$(dirname "$0")/.."

bash scripts/godot.sh samples/OrbitDemo -- --capture "$out" --drive > "$log" 2>&1 &
game=$!
trap 'kill "$game" 2>/dev/null || true' EXIT

window=""
for _ in $(seq 1 150); do
  window="$(xdotool search --name "Evil Engine orbit demo" 2>/dev/null | head -n 1 || true)"
  [ -n "$window" ] && break
  sleep 0.2
done
[ -n "$window" ] || { echo "the demo window never opened" >&2; exit 1; }

sleep 2
xdotool windowfocus "$window"
xdotool key space
sleep 1

for y in $(seq 250 40 650); do
  for x in $(seq 150 50 1150); do
    xdotool mousemove --window "$window" "$x" "$y"
    sleep 0.03
  done
done
sleep 0.5

spot="$(grep -m 1 -oE "hover: pawn-[a-z]+ at [0-9]+,[0-9]+" "$log" | sed -E 's/.* at ([0-9]+),([0-9]+)/\1 \2/')"
[ -n "$spot" ] || { echo "the mouse never found a pawn" >&2; exit 1; }
xdotool mousemove --window "$window" $spot
sleep 0.4
xdotool click 1
sleep 0.5
for _ in 1 2 3; do
  xdotool click 4
  sleep 0.15
done
sleep 1
xdotool key F12
wait "$game"
trap - EXIT
