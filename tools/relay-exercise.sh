#!/usr/bin/env bash
# Drives the Relay sample through every screen, open state, interaction, theme and window size, audits each state
# (layout overflow, contrast, collapsed text, unsettled layout), checks for leaks and UI hangs, and attributes every
# logged warning and error to the step that caused it, and totals Maui.Carbon's metrics (CdsMetrics) per step and
# component. Writes exercise-report.md (+ .json, the Serilog log and captures of each visual finding) to the output
# directory and prints the report.
# Must run outside any sandbox with window-server access. Build first: dotnet build samples/Maui.Carbon.Relay -c Release
#   -f net11.0-maccatalyst -r maccatalyst-arm64
# Usage: [QUICK=1] [THEMES=white,g100] [SIZES=480,1440] [PHASES=login,screens,interactions,leaks,stress,live-resize] [VISIT=<route> with PHASES=login,visit]
#        tools/relay-exercise.sh [outdir] [Debug|Release]
set -uo pipefail
cd "$(dirname "$0")/.."
OUT="${1:-$PWD/out/exercise}"; CONFIG="${2:-Release}"
rm -rf "$OUT"; mkdir -p "$OUT"
# The newest .NET the app was built for (the TFM follows CarbonNetVersion in Directory.Build.props).
RELAY=$(ls -d samples/Maui.Carbon.Relay/bin/"$CONFIG"/net*-maccatalyst/maccatalyst-arm64/Maui.Carbon.Relay.app 2>/dev/null | sort -V | tail -1)
[[ -n "$RELAY" ]] || { echo "no $CONFIG Mac Catalyst build of Relay: build it first (see above)"; exit 2; }
ARGS=(--env RELAY_EXERCISE_DIR="$OUT")
[[ -n "${QUICK:-}" ]] && ARGS+=(--env RELAY_EXERCISE_QUICK=1)
[[ -n "${THEMES:-}" ]] && ARGS+=(--env RELAY_EXERCISE_THEMES="$THEMES")
[[ -n "${SIZES:-}" ]] && ARGS+=(--env RELAY_EXERCISE_SIZES="$SIZES")
[[ -n "${PHASES:-}" ]] && ARGS+=(--env RELAY_EXERCISE_PHASES="$PHASES")
[[ -n "${VISIT:-}" ]] && ARGS+=(--env RELAY_EXERCISE_VISIT="$VISIT")
STAMP="$OUT/.started"; touch "$STAMP"
open -n -W "${ARGS[@]}" "$RELAY"
if [[ ! -f "$OUT/exercise-report.md" ]]; then
  echo "no report: the app exited before the first phase finished. Log tail:"; tail -40 "$OUT"/relay-*.log 2>/dev/null
fi
[[ -f "$OUT/exercise-report.md" ]] && cat "$OUT/exercise-report.md"
# A crash: name the step it happened in and the top of the crashing thread's stack (from the macOS crash report).
# The crash reporter writes the .ips a few seconds after the process dies.
CRASH=""
if ! grep -q survived "$OUT/exercise-live-resize.txt" 2>/dev/null; then
  for _ in $(seq 20); do
    CRASH=$(find "$HOME/Library/Logs/DiagnosticReports" -name 'Maui.Carbon.Relay-*.ips' -newer "$STAMP" 2>/dev/null | head -1)
    [[ -n "$CRASH" ]] && break
    python3 -c 'import time; time.sleep(1)'
  done
fi
if [[ -n "$CRASH" ]]; then
  echo; echo "## CRASH during: $(tail -1 "$OUT/exercise-steps.txt" 2>/dev/null)"; echo
  python3 - "$CRASH" <<'PY'
import json, sys
head, body = open(sys.argv[1]).read().split('\n', 1)
d = json.loads(body)
e = d.get('exception', {})
print(f"{e.get('type')} {e.get('signal')}: {e.get('message', e.get('subtype', ''))}")
images = d['usedImages']
frames = d['threads'][d['faultingThread']]['frames']
seen = []
for f in frames[:60]:
    s = f.get('symbol', '?')
    if 'Maui_Carbon' in s or not seen or len(seen) < 6:
        seen.append(s)
print('\n'.join('  ' + s for s in dict.fromkeys(seen)))
print(f"({len(frames)} frames; report {sys.argv[1]})")
PY
  exit 2
fi
# Non-zero when anything was logged at error level, so this can gate a test run.
! grep -qE '\[(ERR|FTL)\]' "$OUT"/relay-*.log
