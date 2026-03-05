#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

mkdir -p "${HOME}/.zsh" "${HOME}/.tmux"

cp "${ROOT_DIR}/zsh/tva-prompt.zsh" "${HOME}/.zsh/tva-prompt.zsh"
cp "${ROOT_DIR}/tmux/tva.tmux.conf" "${HOME}/.tmux/tva.tmux.conf"
cp "${ROOT_DIR}/tmux/tva-dashboard.sh" "${HOME}/.tmux/tva-dashboard.sh"
chmod +x "${HOME}/.tmux/tva-dashboard.sh"

if [[ ! -f "${HOME}/.zshrc" ]]; then
  touch "${HOME}/.zshrc"
fi

if ! grep -Fq 'source ~/.zsh/tva-prompt.zsh' "${HOME}/.zshrc"; then
  {
    echo
    echo '# TVA terminal prompt'
    echo 'source ~/.zsh/tva-prompt.zsh'
  } >> "${HOME}/.zshrc"
fi

if [[ ! -f "${HOME}/.tmux.conf" ]]; then
  touch "${HOME}/.tmux.conf"
fi

if ! grep -Fq 'source-file ~/.tmux/tva.tmux.conf' "${HOME}/.tmux.conf"; then
  {
    echo
    echo '# TVA tmux theme'
    echo 'source-file ~/.tmux/tva.tmux.conf'
  } >> "${HOME}/.tmux.conf"
fi

echo "Installed TVA zsh + tmux files."
echo "Next: import ${ROOT_DIR}/iterm2/TVA-Loki.itermcolors in iTerm2 and apply font settings from INSTALL.md"
