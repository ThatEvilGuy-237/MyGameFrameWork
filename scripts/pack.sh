#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
feed="${EVIL_FEED:-/mnt/hdd/projects/MyGameFrameWork-data/feed}"
version="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' Directory.Build.props)"
packages=(Evil.Engine.Mathematics Evil.Engine)
mkdir -p "$feed"
for id in "${packages[@]}"; do
  if [ -e "$feed/$id.$version.nupkg" ]; then
    echo "refused: $id $version is already in the feed; a version never changes, bump <Version> in Directory.Build.props" >&2
    exit 1
  fi
done
commit="$(git rev-parse --short HEAD)"
if [ -z "${EVIL_FEED:-}" ] && ! git diff --quiet HEAD; then
  echo "refused: the working tree has changes; the feed holds committed code only" >&2
  exit 1
fi
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
for id in "${packages[@]}"; do
  dotnet pack "src/$id/$id.csproj" -c Release -o "$out" -nologo -v q -p:ContinuousIntegrationBuild=true -p:RepositoryCommit="$commit" >/dev/null
done
for id in "${packages[@]}"; do
  install -m 644 "$out/$id.$version.nupkg" "$feed/"
  if [ -z "${EVIL_FEED:-}" ]; then
    "$HOME/bin/evil-packages" one nuget "$id" "$version" --save >/dev/null || { echo "refused: evil-packages does not count $id $version as our own package" >&2; exit 1; }
  fi
done
echo "packed ${packages[*]} $version ($commit) into $feed"
