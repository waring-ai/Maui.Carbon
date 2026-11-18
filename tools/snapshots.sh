#!/usr/bin/env bash
# Screenshots every gallery page (one app launch per category so few pages stay alive).
# Must run outside any sandbox with window-server access. Usage: tools/snapshots.sh [outdir] [themes] [Debug|Release]
# Release captures are faster and less timing-sensitive; build it first with -c Release.
set -uo pipefail
cd "$(dirname "$0")/.."
OUT="${1:-$PWD/out/shots}"; THEMES="${2:-white,g100}"; CONFIG="${3:-Debug}"
APP="samples/Maui.Carbon.Gallery/bin/$CONFIG/net10.0-maccatalyst/maccatalyst-arm64/Carbon Gallery.app"
rm -rf "$OUT"; mkdir -p "$OUT"
for c in Foundations Actions Forms FluidForms Data Navigation Containers Overlays Feedback AI Layout Patterns; do
  before=$(ls "$OUT"/*.png 2>/dev/null | wc -l)
  open -n -W --env CARBON_SNAPSHOT_DIR="$OUT" --env CARBON_SNAPSHOT_CATEGORY="$c" --env CARBON_SNAPSHOT_THEMES="$THEMES" "$APP"
  after=$(ls "$OUT"/*.png 2>/dev/null | wc -l)
  echo "$c: $((after - before)) screenshots"
done
grep -vE ': ok|: overlays \[' "$OUT/snapshot-log.txt" || true
echo "overlays not fully shown at capture:"
grep ': overlays \[' "$OUT/snapshot-log.txt" | grep -E 'opacity=0|open=False|size=0x' || echo "none"
