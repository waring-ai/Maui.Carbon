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
dotnet run --project tools/Maui.Carbon.Generator -p:UseSharedCompilation=false -- --check

step "library net10.0"
dotnet build src/Maui.Carbon -f net10.0 "${FLAGS[@]}"

if [[ $FAST == 0 ]]; then
  step "library net10.0-maccatalyst"
  dotnet build src/Maui.Carbon -f net10.0-maccatalyst "${FLAGS[@]}"

  step "library net10.0-ios"
  dotnet build src/Maui.Carbon -f net10.0-ios "${FLAGS[@]}"

  step "Apple platform code is compiled in"
  for tf in net10.0-maccatalyst net10.0-ios; do
    strings "src/Maui.Carbon/bin/Debug/$tf/Maui.Carbon.dll" | grep -c CdsAppleHitTargetView >/dev/null \
      || { echo "src/Maui.Carbon/Apple/ is missing from the $tf build"; exit 1; }
  done

  STUB_SDK="$PWD/out/stub-android-sdk"
  mkdir -p "$STUB_SDK/platform-tools" && touch "$STUB_SDK/platform-tools/adb" && chmod +x "$STUB_SDK/platform-tools/adb"
  step "library net10.0-android (compile only)"
  # The stub SDK makes the Android tooling warn; fail only on compiler/analyzer warnings or errors.
  ANDROID_LOG=$(dotnet build src/Maui.Carbon -t:Compile -f net10.0-android -p:CarbonIncludeAndroid=true \
    -p:_AndroidAllowMissingSdkTooling=true -p:AndroidSdkDirectory="$STUB_SDK" -p:TreatWarningsAsErrors=false "${FLAGS[@]}" 2>&1) \
    || { echo "$ANDROID_LOG" | grep -E 'error' ; echo "android compile failed"; exit 1; }
  if echo "$ANDROID_LOG" | grep -E ' warning (CS|CA|IL|SYSLIB)[0-9]+' ; then echo "android compile has warnings"; exit 1; fi

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
