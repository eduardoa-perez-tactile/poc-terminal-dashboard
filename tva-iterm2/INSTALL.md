# TVA iTerm2 Setup (Loki-Inspired)

## Files in this bundle
- `iterm2/TVA-Loki.itermcolors` (color preset)
- `iterm2/TVA-Loki-CRT-Dynamic.json` (auto-load iTerm2 dynamic profile with CRT-like overlay)
- `shader/tva-crt.frag` (CRT fragment shader)
- `zsh/tva-prompt.zsh` (TVA prompt)
- `tmux/tva.tmux.conf` (tmux visual theme)
- `tmux/tva-dashboard.sh` (optional multi-pane dashboard launcher)
- `install.sh` (installs zsh + tmux files)
- `install-iterm2-crt.sh` (installs dynamic iTerm2 CRT-like profile)

## 1) Install the font
```bash
brew tap homebrew/cask-fonts
brew install --cask font-ibm-plex-mono
```

## 2) Install shell + tmux configs
```bash
cd /Users/eduardoleale/workspace/terminal/tva-iterm2
./install.sh
```
Reload:
```bash
exec zsh
```

For tmux, either start a new session or reload manually:
```bash
tmux source-file ~/.tmux/tva.tmux.conf
```

## 3) Import iTerm2 color preset
1. Open iTerm2 -> Settings -> Profiles -> Colors.
2. Click `Color Presets...` -> `Import...`.
3. Select `iterm2/TVA-Loki.itermcolors`.
4. Re-open `Color Presets...` and choose `TVA-Loki`.

## 3.5) Install auto-load CRT-like iTerm2 profile (recommended)
```bash
cd /Users/eduardoleale/workspace/terminal/tva-iterm2
./install-iterm2-crt.sh
```
Then in iTerm2:
1. `Settings -> Profiles -> Other Actions... -> Reload Profiles`
2. Select profile `TVA Loki CRT`

## 4) Apply font and text rendering settings in iTerm2
Open iTerm2 -> Settings -> Profiles -> Text:
1. Font: `IBM Plex Mono`
2. Size: `15`
3. Horizontal Spacing: `105` (slight letter spacing increase)
4. Disable ligatures: uncheck `Use ligatures`.

## 5) CRT effect setup
`shader/tva-crt.frag` includes:
- Horizontal scanlines
- Subtle phosphor glow
- Gentle brightness variance
- Optional curvature distortion (uniform: `curvature`)

Important: iTerm2 does not expose a documented custom fragment-shader loader in stable profile settings. The included dynamic profile reproduces a CRT-like look (scanlines + light bloom feel) using supported iTerm profile attributes.

## 6) Optional TVA dashboard (tmux)
Launch:
```bash
~/.tmux/tva-dashboard.sh
```

Layout:
- Pane 1: shell/editor (vim/nvim friendly)
- Pane 2: extra shell
- Pane 3: logs (`./logs/dev.log` if available)
- Pane 4: system stats (`btop`/`htop`/`top` fallback)

Optional command overrides:
```bash
TVA_LOG_CMD='tail -F /path/to/your.log' ~/.tmux/tva-dashboard.sh
TVA_SYS_CMD='vm_stat 1' ~/.tmux/tva-dashboard.sh
```

## 7) Functional behavior retained
The setup keeps normal development usage intact:
- iTerm2 tabs/splits/sessions unchanged
- SSH unchanged
- git unchanged
- vim/nvim unchanged
- node/python tooling unchanged
