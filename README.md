# Senran Kagura Aspect Ratio MOD Tool (v2)

A Windows desktop utility for applying widescreen and ultrawide aspect-ratio fixes to supported SENRAN KAGURA PC titles.

## Supported Games

- **SENRAN KAGURA Estival Versus (EV)**
- **SENRAN KAGURA Burst Re:Newal (BrN)**
- **SENRAN KAGURA Peach Beach Splash (PBS)**
- **SENRAN KAGURA Reflexions**
- **SENRAN KAGURA Peach Ball (PB)**

> Note: Patching support for **PBS**, **Reflexions**, and **Peach Ball** is currently fairly basic. Additional issue fixes and broader patch coverage are being investigated.

## Download

- Latest releases: https://github.com/JakLuminth/SenranKaguraAspectMOD/releases

## Quick Start

1. Place the tool executable in the **root folder of a supported game**.
2. Launch the tool.
3. Select a game (auto-selected when exactly one supported executable is detected).
4. Select an aspect ratio preset.
5. Click **Apply** and confirm.
6. Wait for completion.
7. (Optional) Use **Show Log** to review detailed patch output.

## Features

- Multi-game support with a single UI
- Auto-detection when exactly one supported game executable exists in the folder
- Aspect ratio presets:
  - 16:10
  - 15:9
  - 16:9
  - 21:9 (2560 by 1080)
  - 21:9 (3440 by 1440)
  - 21:9 (3840 by 1600)
  - 32:9
- Patches executable offsets for the selected game
- Patches additional binary data files based on game layout:
  - Character camera files (`GameData\\Motion\\Player\\*cam.cat`)
  - EV beach files (`GameData\\Motion\\Beach\\*.cat`)
  - EV creative finish files (`GameData\\Placement\\plbg\\*.cat`)
  - BrN UI files (`GameData\\Ui\\*data.cat`, including subfolders)
- Fast binary scanning and parallel file patching for data-file operations
- Timestamped in-app logging with expandable/collapsible log panel
- One-click revert using created backups

## Backup and Revert Behavior

- The first time a file is patched, a backup is created and preserved for reuse.
- Executable backups are created as `*.backup` beside the game executable.
- Data-file backups are stored under a `Backup` subfolder in each patched data directory.
- Revert restores the executable and any available backed-up data files for the selected game.

By default, **Delete backups automatically** is enabled. After a successful revert, backup files/folders are removed unless that option is disabled first.

> Important: Revert before applying a different ratio. Applying multiple ratios in sequence without revert can leave mixed values.

## Credits

This project is based on the original work and guide by **Loggey**:

- Steam guide: https://steamcommunity.com/sharedfiles/filedetails/?id=1815701855

Thanks to Loggey for the original offsets, process, and tool concept.

## Disclaimer

- This tool edits local game files only.
- Use at your own risk.
- Keep your own independent backups if needed.
