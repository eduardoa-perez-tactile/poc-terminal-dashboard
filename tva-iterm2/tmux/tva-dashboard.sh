#!/usr/bin/env bash
set -euo pipefail

SESSION_NAME="${1:-tva}"
SHELL_BIN="${SHELL:-/bin/zsh}"
LOG_CMD="${TVA_LOG_CMD:-if [ -f ./logs/dev.log ]; then tail -n 120 -F ./logs/dev.log; else printf 'No ./logs/dev.log found in %s\\n' \"$PWD\"; while sleep 4; do printf '[%s] waiting for logs...\\n' \"$(date +%H:%M:%S)\"; done; fi}"
SYS_CMD="${TVA_SYS_CMD:-if command -v btop >/dev/null 2>&1; then btop; elif command -v htop >/dev/null 2>&1; then htop; else top -o cpu; fi}"

if tmux has-session -t "${SESSION_NAME}" 2>/dev/null; then
  exec tmux attach-session -t "${SESSION_NAME}"
fi

tmux new-session -d -s "${SESSION_NAME}" -n DASH "${SHELL_BIN}"
tmux split-window -h -t "${SESSION_NAME}:DASH.0" "${SHELL_BIN}"
tmux split-window -v -t "${SESSION_NAME}:DASH.1" "${SHELL_BIN}"
tmux split-window -v -t "${SESSION_NAME}:DASH.0" "${SHELL_BIN}"
tmux select-layout -t "${SESSION_NAME}:DASH" tiled

tmux send-keys -t "${SESSION_NAME}:DASH.2" "${LOG_CMD}" C-m
tmux send-keys -t "${SESSION_NAME}:DASH.3" "${SYS_CMD}" C-m

tmux select-pane -t "${SESSION_NAME}:DASH.0"
exec tmux attach-session -t "${SESSION_NAME}"
