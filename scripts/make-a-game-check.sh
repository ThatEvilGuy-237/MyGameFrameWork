#!/usr/bin/env bash
set -uo pipefail
cd "$(dirname "$0")/.."
repo="$(pwd)"

if [ "${1:-}" = "--run" ]; then
  game="$2"
  work="$3"
  bash "$repo/scripts/godot.sh" "$game" > "$work/run.log" 2>&1 &
  pid=$!
  window=""
  for _ in $(seq 1 200); do
    window="$(xdotool search --name "My Game" 2>/dev/null | head -n 1 || true)"
    [ -n "$window" ] && break
    sleep 0.2
  done
  [ -n "$window" ] || { kill "$pid"; echo "the game window never opened" >&2; exit 1; }
  sleep 8
  import -window "$window" -depth 8 "$work/screen.png"
  xdotool windowfocus "$window"
  sleep 0.5
  xdotool key Escape
  for _ in $(seq 1 100); do
    kill -0 "$pid" 2>/dev/null || break
    sleep 0.2
  done
  if kill -0 "$pid" 2>/dev/null; then kill "$pid"; echo "Escape did not close the game" >&2; exit 1; fi
  wait "$pid"
  exit $?
fi

feed="$(realpath "${1:-/mnt/hdd/projects/MyGameFrameWork-data/feed}")"
shot="${2:-$repo/documents/screenshots/make-a-game.png}"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT
game="$work/mygame"
mkdir -p "$game"

echo "== a new game from 'Make a Game', Evil.Engine from $feed"
python3 -I scripts/make-a-game.py "documents/Getting Started/Make a Game.md" "$game" "$feed" || exit 1
(cd "$game" && NUGET_PACKAGES="$work/packages" dotnet build -nologo -v q -warnaserror) > "$work/build.log" 2>&1 || { tail -20 "$work/build.log"; echo "the game from the note does not build" >&2; exit 1; }
copied="$(ls "$game/.godot/mono/temp/bin/Debug" | grep -c '^Evil\.Engine.*\.dll$')"
[ "$copied" = 11 ] || { echo "expected the 11 Evil.Engine libraries next to the game, found $copied" >&2; exit 1; }
bash scripts/godot.sh --headless "$game" --import > "$work/import.log" 2>&1 || { tail -20 "$work/import.log"; exit 1; }

xvfb-run -a -s "-screen 0 1600x900x24" bash "$repo/scripts/make-a-game-check.sh" --run "$game" "$work" || { tail -20 "$work/run.log"; exit 1; }
if grep -qE "ERROR|Exception|leaked at exit|still in use at exit" "$work/run.log"; then
  grep -E "ERROR|Exception|leaked at exit|still in use at exit" "$work/run.log" >&2
  echo "the game reported errors or leaks" >&2
  exit 1
fi

cp "$work/screen.png" "$shot"
floor="$(convert "$shot" -crop 1x1+640+620 -depth 8 txt:- | tail -n 1 | grep -oE '#[0-9A-F]{6}')"
text="$(convert "$shot" -crop 200x40+10+20 -depth 8 txt:- | grep -cE '#F[0-9A-F]F[0-9A-F]F[0-9A-F]')"
python3 -I - "$floor" "$text" <<'PY'
import colorsys
import sys

floor, text = sys.argv[1], int(sys.argv[2])
red, green, blue = (int(floor[i:i + 2], 16) / 255 for i in (1, 3, 5))
hue = colorsys.rgb_to_hls(red, green, blue)[0] * 360
print(f"floor {floor} (hue {hue:.0f}), {text} white text pixels")
if not 100 <= hue <= 165:
    sys.exit("the floor is not green")
if text < 20:
    sys.exit("'My Game' is not in the corner")
PY
[ $? = 0 ] || exit 1
echo "made a game from the note: builds, runs, green floor, text, Escape quits, no leaks; picture $shot"
