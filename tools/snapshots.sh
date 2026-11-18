#!/usr/bin/env bash
# Screenshots every gallery page (one app launch per category so few pages stay alive), or with APP=relay every
# screen and open state of the Relay sample (one launch; it signs itself in).
# Must run outside any sandbox with window-server access. Usage: [APP=relay] tools/snapshots.sh [outdir] [themes] [Debug|Release]
# Release captures are faster and less timing-sensitive; build it first with -c Release.
set -uo pipefail
cd "$(dirname "$0")/.."
OUT="${1:-$PWD/out/shots}"; THEMES="${2:-white,g100}"; CONFIG="${3:-Debug}"
rm -rf "$OUT"; mkdir -p "$OUT"
if [[ "${APP:-gallery}" == "relay" ]]; then
  RELAY="samples/Maui.Carbon.Relay/bin/$CONFIG/net10.0-maccatalyst/maccatalyst-arm64/Maui.Carbon.Relay.app"
  open -n -W --env CARBON_SNAPSHOT_DIR="$OUT" --env CARBON_SNAPSHOT_THEMES="$THEMES" "$RELAY"
  echo "$(ls "$OUT"/*.png 2>/dev/null | wc -l | tr -d ' ') screenshots"
  grep -v ': ok' "$OUT/snapshot-log.txt" || echo "all captured"
  # Relay's Serilog file (relay-<date>.log) lands beside the shots in this mode.
  echo "log: $(cat "$OUT"/relay-*.log 2>/dev/null | grep -c '\[WRN\]') warnings"
  grep -hE '\[(ERR|FTL)\]' -A20 "$OUT"/relay-*.log 2>/dev/null || echo "log: no errors"
  exit 0
fi
APP="samples/Maui.Carbon.Gallery/bin/$CONFIG/net10.0-maccatalyst/maccatalyst-arm64/Maui.Carbon.Gallery.app"
for c in Foundations Actions Forms FluidForms Data Navigation Containers Overlays Feedback AI Layout Patterns; do
  before=$(ls "$OUT"/*.png 2>/dev/null | wc -l)
  open -n -W --env CARBON_SNAPSHOT_DIR="$OUT" --env CARBON_SNAPSHOT_CATEGORY="$c" --env CARBON_SNAPSHOT_THEMES="$THEMES" "$APP"
  after=$(ls "$OUT"/*.png 2>/dev/null | wc -l)
  echo "$c: $((after - before)) screenshots"
done
grep -vE ': ok|: overlays \[' "$OUT/snapshot-log.txt" || true
echo "overlays not fully shown at capture:"
grep ': overlays \[' "$OUT/snapshot-log.txt" | grep -E 'opacity=0|open=False|size=0x' || echo "none"
