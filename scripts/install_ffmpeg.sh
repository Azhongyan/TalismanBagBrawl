#!/usr/bin/env bash
set -euo pipefail

WINDOWS_INSTALL_DIR="${FFMPEG_INSTALL_DIR:-F:\\Tools\\ffmpeg}"
WINDOWS_TEMP_DIR="${FFMPEG_TEMP_DIR:-F:\\Tools\\winget-temp}"

log() {
  printf '%s\n' "$*"
}

has_cmd() {
  command -v "$1" >/dev/null 2>&1
}

find_windows_tool() {
  local name="$1"
  local candidate

  for candidate in \
    "/f/Tools/ffmpeg"/*/bin/"${name}.exe" \
    "/mnt/f/Tools/ffmpeg"/*/bin/"${name}.exe"; do
    if [ -x "$candidate" ]; then
      printf '%s\n' "$candidate"
      return 0
    fi
  done

  return 1
}

find_tool() {
  local name="$1"

  if has_cmd "$name"; then
    command -v "$name"
    return 0
  fi

  if has_cmd "${name}.exe"; then
    command -v "${name}.exe"
    return 0
  fi

  find_windows_tool "$name"
}

print_versions_if_available() {
  local ffmpeg_bin
  local ffprobe_bin

  ffmpeg_bin="$(find_tool ffmpeg || true)"
  ffprobe_bin="$(find_tool ffprobe || true)"

  if [ -n "$ffmpeg_bin" ] && [ -n "$ffprobe_bin" ]; then
    log "ffmpeg found: $ffmpeg_bin"
    "$ffmpeg_bin" -version | head -n 1
    log "ffprobe found: $ffprobe_bin"
    "$ffprobe_bin" -version | head -n 1
    return 0
  fi

  return 1
}

detect_os() {
  case "$(uname -s)" in
    Linux*) printf 'linux\n' ;;
    Darwin*) printf 'macos\n' ;;
    MINGW*|MSYS*|CYGWIN*) printf 'windows\n' ;;
    *) printf 'unknown\n' ;;
  esac
}

install_linux() {
  if ! has_cmd apt-get; then
    log "ffmpeg is missing, and apt-get is not available. Install failed."
    return 1
  fi

  if [ "$(id -u)" -eq 0 ]; then
    apt-get update
    apt-get install -y ffmpeg
  elif has_cmd sudo; then
    sudo apt-get update
    sudo apt-get install -y ffmpeg
  else
    log "ffmpeg is missing, and sudo is not available for apt-get. Install failed."
    return 1
  fi
}

install_macos() {
  if ! has_cmd brew; then
    log "ffmpeg is missing, and Homebrew is not available. Install failed."
    return 1
  fi

  brew install ffmpeg
}

install_windows() {
  if has_cmd powershell.exe; then
    powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "\
      New-Item -ItemType Directory -Force -Path '${WINDOWS_INSTALL_DIR}','${WINDOWS_TEMP_DIR}' | Out-Null; \
      \$env:TEMP='${WINDOWS_TEMP_DIR}'; \
      \$env:TMP='${WINDOWS_TEMP_DIR}'; \
      if (Get-Command winget -ErrorAction SilentlyContinue) { \
        winget install --id Gyan.FFmpeg --exact --location '${WINDOWS_INSTALL_DIR}' --accept-source-agreements --accept-package-agreements --disable-interactivity; \
        exit \$LASTEXITCODE; \
      } \
      if (Get-Command choco -ErrorAction SilentlyContinue) { \
        choco install ffmpeg -y --install-directory='${WINDOWS_INSTALL_DIR}' --cache-location='${WINDOWS_TEMP_DIR}'; \
        exit \$LASTEXITCODE; \
      } \
      Write-Host 'ffmpeg is missing, and neither winget nor choco is available. Install failed.'; \
      exit 1"
    return $?
  fi

  log "ffmpeg is missing, and Windows package managers cannot be launched from this shell. Install failed."
  return 1
}

main() {
  local os_name
  os_name="$(detect_os)"
  log "Detected OS: $os_name"

  if print_versions_if_available; then
    log "ffmpeg is already available; skipping install."
    return 0
  fi

  case "$os_name" in
    linux) install_linux ;;
    macos) install_macos ;;
    windows) install_windows ;;
    *)
      log "Unsupported OS: $os_name. Install failed."
      return 1
      ;;
  esac

  print_versions_if_available
}

main "$@"
