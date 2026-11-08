#!/usr/bin/env bash
set -uo pipefail
godot="${EVIL_GODOT:-/mnt/hdd/tools/godot-4.7.2/Godot_v4.7.2-stable_mono_linux.x86_64}"
if [ ! -x "$godot" ]; then
  echo "Godot 4.7.2 .NET not found at $godot (set EVIL_GODOT)" >&2
  exit 1
fi

headless=0
if [ "${1:-}" = "--headless" ]; then
  headless=1
  shift
fi
project="$1"
shift

if [ "$headless" = 1 ]; then
  run=("$godot" --headless --path "$project" --audio-driver Dummy "$@")
else
  run=("$godot" --path "$project" --rendering-method gl_compatibility --rendering-driver opengl3 --audio-driver Dummy "$@")
fi
run=(timeout 300 "${run[@]}")
if command -v systemd-run >/dev/null; then
  run=(systemd-run --user --scope -p MemoryMax=2G --quiet "${run[@]}")
fi
if [ "$headless" = 0 ] && [ -z "${DISPLAY:-}" ]; then
  run=(xvfb-run -a -s "-screen 0 1600x900x24" "${run[@]}")
fi

"${run[@]}" 2>&1 | grep -v -E "^ALSA|^$|audio_driver_alsa|All audio drivers failed|servers/audio/audio_server|Could not set V-Sync|gl_manager_x11.cpp"
exit "${PIPESTATUS[0]}"
