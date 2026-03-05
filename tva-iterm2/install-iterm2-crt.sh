#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OVERLAY_SRC="${ROOT_DIR}/shader/tva-scanline-overlay.svg"
OVERLAY_DIR="${HOME}/.iterm2"
OVERLAY_PNG="${OVERLAY_DIR}/tva-scanline-overlay.png"
DYN_DIR="${HOME}/Library/Application Support/iTerm2/DynamicProfiles"
DYN_FILE="${DYN_DIR}/TVA-Loki-CRT.json"
JSON_SRC="${ROOT_DIR}/iterm2/TVA-Loki-CRT-Dynamic.json"

mkdir -p "${OVERLAY_DIR}" "${DYN_DIR}" /tmp

qlmanage -t -s 1920 -o /tmp "${OVERLAY_SRC}" >/tmp/ql_tva_crt.log 2>&1
if [[ -f "/tmp/tva-scanline-overlay.svg.png" ]]; then
  cp "/tmp/tva-scanline-overlay.svg.png" "${OVERLAY_PNG}"
elif [[ -f "/tmp/tva-scanline-overlay.png" ]]; then
  cp "/tmp/tva-scanline-overlay.png" "${OVERLAY_PNG}"
else
  echo "Failed to render scanline PNG. Check /tmp/ql_tva_crt.log"
  exit 1
fi

cp "${JSON_SRC}" "${DYN_FILE}"

cat <<MSG
Installed iTerm2 dynamic profile:
  ${DYN_FILE}
Overlay image:
  ${OVERLAY_PNG}

In iTerm2: Settings -> Profiles -> Other Actions... -> Reload Profiles
Then select profile: TVA Loki CRT
MSG
