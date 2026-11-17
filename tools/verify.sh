#!/usr/bin/env bash
# Builds and tests Maui.Carbon the way CI does, on a Mac without Android SDK / Windows.
#   tools/verify.sh               library (net10.0 + maccatalyst) + compile-only android/windows + tests
#   tools/verify.sh --fast        library net10.0 + tests only (inner loop)
#   tools/verify.sh --gallery     also build the gallery for Mac Catalyst
set -euo pipefail
cd "$(dirname "$0")/.."

export MSBUILDDISABLENODEREUSE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
FLAGS=(-nodeReuse:false -p:UseSharedCompilation=false -v:minimal -clp:NoSummary)
FAST=0; GALLERY=0
for a in "$@"; do
  case "$a" in
    --fast) FAST=1 ;;
    --gallery) GALLERY=1 ;;
    *) echo "unknown option $a" >&2; exit 2 ;;
  esac
done

trap 'dotnet build-server shutdown >/dev/null 2>&1 || true' EXIT

step() { printf '\n== %s\n' "$*"; }

step "generator drift check"
dotnet run --project tools/Maui.Carbon.Generator "${FLAGS[@]}" -- --check

step "library net10.0"
dotnet build src/Maui.Carbon -f net10.0 "${FLAGS[@]}"

if [[ $FAST == 0 ]]; then
  step "library net10.0-maccatalyst"
  dotnet build src/Maui.Carbon -f net10.0-maccatalyst "${FLAGS[@]}"

  step "library net10.0-ios"
  dotnet build src/Maui.Carbon -f net10.0-ios "${FLAGS[@]}"

  STUB_SDK="$PWD/out/stub-android-sdk"
  mkdir -p "$STUB_SDK/platform-tools" && touch "$STUB_SDK/platform-tools/adb" && chmod +x "$STUB_SDK/platform-tools/adb"
  step "library net10.0-android (compile only)"
  dotnet build src/Maui.Carbon -t:Compile -f net10.0-android -p:CarbonIncludeAndroid=true \
    -p:_AndroidAllowMissingSdkTooling=true -p:AndroidSdkDirectory="$STUB_SDK" -p:TreatWarningsAsErrors=false \
    -p:NoWarn='XA0034%3BXARAT7003%3BMSB4181' "${FLAGS[@]}" 2>&1 | grep -E 'error|Warn.*CS' || true
  dotnet build src/Maui.Carbon -t:Compile -f net10.0-android -p:CarbonIncludeAndroid=true \
    -p:_AndroidAllowMissingSdkTooling=true -p:AndroidSdkDirectory="$STUB_SDK" -p:TreatWarningsAsErrors=false "${FLAGS[@]}" >/dev/null 2>&1 \
    || { echo "android compile failed"; exit 1; }

  step "library net10.0-windows (compile only)"
  dotnet build src/Maui.Carbon -t:Compile -f net10.0-windows10.0.19041.0 -p:CarbonIsWindows=true -p:EnableWindowsTargeting=true "${FLAGS[@]}"
fi

step "tests"
dotnet test tests/Maui.Carbon.Tests "${FLAGS[@]}"

if [[ $GALLERY == 1 ]]; then
  step "gallery net10.0-maccatalyst"
  dotnet build samples/Maui.Carbon.Gallery -f net10.0-maccatalyst -r maccatalyst-arm64 "${FLAGS[@]}"
fi

printf '\nverify: OK\n'
