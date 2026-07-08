#!/usr/bin/env bash
set -euo pipefail

usage() {
  printf 'Usage: bash scripts/extract_frames.sh input.mp4 output_frames 12\n' >&2
}

if [ "$#" -ne 3 ]; then
  usage
  exit 2
fi

input_file="$1"
output_dir="$2"
fps="$3"

if [ ! -f "$input_file" ]; then
  printf 'Input video not found: %s\n' "$input_file" >&2
  exit 1
fi

if ! printf '%s' "$fps" | grep -Eq '^[0-9]+([.][0-9]+)?$'; then
  printf 'FPS must be a positive number: %s\n' "$fps" >&2
  exit 1
fi

find_windows_ffmpeg() {
  local candidate

  for candidate in \
    "/f/Tools/ffmpeg"/*/bin/ffmpeg.exe \
    "/mnt/f/Tools/ffmpeg"/*/bin/ffmpeg.exe; do
    if [ -x "$candidate" ]; then
      printf '%s\n' "$candidate"
      return 0
    fi
  done

  return 1
}

find_ffmpeg() {
  if [ -n "${FFMPEG_BIN:-}" ]; then
    printf '%s\n' "$FFMPEG_BIN"
    return 0
  fi

  if command -v ffmpeg >/dev/null 2>&1; then
    command -v ffmpeg
    return 0
  fi

  if command -v ffmpeg.exe >/dev/null 2>&1; then
    command -v ffmpeg.exe
    return 0
  fi

  find_windows_ffmpeg
}

ffmpeg_bin="$(find_ffmpeg || true)"
if [ -z "$ffmpeg_bin" ]; then
  printf 'ffmpeg not found. Run bash scripts/install_ffmpeg.sh first.\n' >&2
  exit 1
fi

mkdir -p "$output_dir"

"$ffmpeg_bin" \
  -hide_banner \
  -y \
  -i "$input_file" \
  -vf "fps=${fps}" \
  -start_number 1 \
  "$output_dir/frame_%04d.png"

printf 'Frames written to: %s/frame_0001.png ...\n' "$output_dir"
