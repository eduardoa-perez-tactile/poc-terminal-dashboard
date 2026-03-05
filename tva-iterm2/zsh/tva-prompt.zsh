# TVA prompt theme for zsh.
# Source from ~/.zshrc after your framework/theme setup.

if [[ -n ${__TVA_PROMPT_LOADED:-} ]]; then
  return
fi
__TVA_PROMPT_LOADED=1

autoload -Uz colors vcs_info
colors
setopt prompt_subst

zstyle ':vcs_info:*' enable git
zstyle ':vcs_info:git:*' formats ' [BRANCH:%b]'
zstyle ':vcs_info:git:*' actionformats ' [BRANCH:%b|%a]'
typeset -g __TVA_BOOT_SHOWN=0

_tva_precmd() {
  local last_status=$?
  vcs_info

  TVA_TIME="$(date +%H:%M:%S)"
  TVA_STATE="OK"
  if [[ ${last_status} -ne 0 ]]; then
    TVA_STATE="ERR:${last_status}"
  fi

  local amber='%F{214}'
  local dim='%F{240}'
  local reset='%f'

  if [[ ${__TVA_BOOT_SHOWN} -eq 0 ]]; then
    print -P "${amber}LET'S SEE WHAT YA KNOW:${reset}"
    print -P "${dim}TVA  FILE  EDIT  VIEW  MODE  HELP${reset}"
    print -P "${dim}--------------------------------------------------${reset}"
    __TVA_BOOT_SHOWN=1
  fi

  PROMPT="${amber}TVA::${reset}%n@%m ${dim}%~${reset}${vcs_info_msg_0_} ${dim}[${TVA_TIME}|${TVA_STATE}]${reset}
${amber}> ${reset}"
}

precmd_functions+=(_tva_precmd)

# Keep right prompt empty for cleaner console framing.
RPROMPT=''
