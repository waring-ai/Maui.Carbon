#!/usr/bin/env bash
# Walks every gallery page in sidebar order (Mac Catalyst) under a JetBrains profiler and reports per-page timings.
# Usage: tools/profile-gallery.sh [options]
#   --profiler timeline|sampling|memory|none
#                      timeline (default) and sampling use dotTrace; memory uses dotMemory
#                      (dotnet tool install -g JetBrains.dotMemory.Console); none only measures.
#                      As of dotTrace 2026.1, sampling loads into the Mac Catalyst app but collects nothing
#                      ("No snapshots have been collected"); timeline works.
#   --config Release|Debug   default Release (Debug is much slower and not representative)
#   --out DIR          default out/profile/<timestamp>
#   --category NAME    one sidebar section, e.g. Charts (a GalleryCategory name)
#   --pages a,b        page titles without spaces, or routes (charts-areachart)
#   --passes N         walk N times; pass 2+ revisits the pages Shell has cached (what revisits cost or leak)
#   --scroll           also scroll each page to the bottom
#   --timeout 30m      stop profiling after this long
#   --no-build         use the existing build
# Writes to DIR: walk-report.md (slowest pages), walk.csv (every visit), app.log, and the profiler's snapshot
# (trace/*.dtt for timeline, trace/*.dtp for sampling, memory/*.dmw for dotMemory; open them in Rider or dotTrace/dotMemory).
# dotTrace records the whole run (startup too) and saves when the app quits; walk-report.md gives each page's start in
# seconds since launch, the snapshot's time axis. dotMemory snapshots are taken by the walk itself through
# JetBrains.Profiler.Api: one before the walk and one after each pass (see WalkRunner.cs).
# Needs window-server access: run it from a logged-in terminal, not over SSH or in a sandbox.
set -euo pipefail
cd "$(dirname "$0")/.."

PROFILER=timeline; CONFIG=Release; OUT=""; CATEGORY=""; PAGES=""; PASSES=1; SCROLL=0; TIMEOUT=30m; BUILD=1
while [[ $# -gt 0 ]]; do
  case "$1" in
    --profiler) PROFILER="$2"; shift 2 ;;
    --config) CONFIG="$2"; shift 2 ;;
    --out) OUT="$2"; shift 2 ;;
    --category) CATEGORY="$2"; shift 2 ;;
    --pages) PAGES="$2"; shift 2 ;;
    --passes) PASSES="$2"; shift 2 ;;
    --scroll) SCROLL=1; shift ;;
    --timeout) TIMEOUT="$2"; shift 2 ;;
    --no-build) BUILD=0; shift ;;
    -h|--help) sed -n '2,22p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "unknown option $1 (see --help)" >&2; exit 2 ;;
  esac
done

case "$PROFILER" in
  sampling|timeline) TOOL=dottrace; INSTALL="dotnet tool install -g JetBrains.dotTrace.GlobalTools" ;;
  memory) TOOL=dotmemory; INSTALL="dotnet tool install -g JetBrains.dotMemory.Console" ;;
  none) TOOL="" ;;
  *) echo "--profiler must be sampling, timeline, memory or none" >&2; exit 2 ;;
esac
if [[ -n "$TOOL" ]] && ! command -v "$TOOL" >/dev/null; then
  echo "$TOOL is not on PATH. Install it with: $INSTALL" >&2
  exit 2
fi

OUT="${OUT:-$PWD/out/profile/$(date +%Y%m%d-%H%M%S)}"
mkdir -p "$OUT"
OUT="$(cd "$OUT" && pwd)"
APP="samples/Maui.Carbon.Gallery/bin/$CONFIG/net11.0-maccatalyst/maccatalyst-arm64/Maui.Carbon.Gallery.app"
EXE="$PWD/$APP/Contents/MacOS/Maui.Carbon.Gallery"

if [[ $BUILD == 1 ]]; then
  echo "== building the gallery ($CONFIG)"
  dotnet build samples/Maui.Carbon.Gallery -c "$CONFIG" -f net11.0-maccatalyst -r maccatalyst-arm64 -v:minimal -clp:NoSummary
fi
[[ -x "$EXE" ]] || { echo "no build at $APP (drop --no-build)" >&2; exit 1; }
if pgrep -f "$APP/Contents/MacOS/" >/dev/null; then
  echo "note: another copy of this gallery build is running; close it for cleaner numbers" >&2
fi

export CARBON_WALK_DIR="$OUT" CARBON_WALK_PASSES="$PASSES" CARBON_WALK_SCROLL="$SCROLL" CARBON_WALK_PROFILER="$PROFILER"
[[ -n "$CATEGORY" ]] && export CARBON_WALK_CATEGORY="$CATEGORY"
[[ -n "$PAGES" ]] && export CARBON_WALK_PAGES="$PAGES"
rm -f "$OUT/walk-done" "$OUT/walk-error.txt"

# The executable is started directly (not with open) so the profiler launches it as its own child process.
echo "== walking the gallery (profiler: $PROFILER) -> $OUT"
case "$PROFILER" in
  none)
    "$EXE" >"$OUT/app.log" 2>&1 || true ;;
  sampling|timeline)
    TYPE=Timeline; EXT=dtt
    if [[ "$PROFILER" == sampling ]]; then
      TYPE=Sampling; EXT=dtp
      echo "warning: dotTrace sampling has not collected data from the Mac Catalyst app (dotTrace 2026.1); use timeline if this run saves nothing" >&2
    fi
    mkdir -p "$OUT/trace"
    # No --use-api: in timeline mode the API's save call never completes and dotTrace waits until --timeout.
    dottrace start --framework=NetCore --profiling-type="$TYPE" --save-to="$OUT/trace/gallery-walk.$EXT" --overwrite \
      --timeout="$TIMEOUT" --no-check-for-updates "$EXE" >"$OUT/app.log" 2>&1 || true ;;
  memory)
    mkdir -p "$OUT/memory"
    dotmemory start --use-api --save-to-dir="$OUT/memory" --overwrite --timeout="$TIMEOUT" "$EXE" >"$OUT/app.log" 2>&1 || true ;;
esac

if [[ -f "$OUT/walk-error.txt" ]]; then echo "the walk failed:"; cat "$OUT/walk-error.txt"; fi
if [[ ! -f "$OUT/walk-done" ]]; then
  echo "the walk did not finish; see $OUT/app.log" >&2
  tail -20 "$OUT/app.log" >&2
  exit 1
fi
echo
sed -n '1,/^## Most time stalled/p' "$OUT/walk-report.md" | sed '$d'
echo "Full report: $OUT/walk-report.md"
echo "Every visit: $OUT/walk.csv"
if [[ -d "$OUT/trace" ]]; then echo "dotTrace snapshots:"; find "$OUT/trace" -maxdepth 1 \( -name '*.dtt' -o -name '*.dtp' \) | sort | sed 's/^/  /' | grep . || echo "  none saved; see $OUT/app.log"; fi
if [[ -d "$OUT/memory" ]]; then echo "dotMemory snapshots:"; find "$OUT/memory" -maxdepth 2 -name '*.dmw' | sed 's/^/  /' | grep . || echo "  none saved; see $OUT/app.log"; fi
