# Senran Kagura EV/BrN Aspect Ratio MOD Tool (v1.2)

A Windows desktop utility for applying widescreen and ultrawide aspect-ratio fixes to:

- **SENRAN KAGURA Estival Versus (EV)**
- **SENRAN KAGURA Burst Re:Newal (BrN)**

This tool automates hex edits that would otherwise be done manually across multiple game files.

## Download

- Latest releases: https://github.com/JakLuminth/SenranKaguraAspectMOD/releases

## Quick Start

1. Put the tool executable in the **root folder of the target game**.
2. Launch the tool.
3. Select game (auto-detected when possible).
4. Select an aspect ratio preset.
5. Click **Apply** and confirm.
6. Wait for completion and review the log panel.

## Features

- Game selection for EV/BrN
- Auto-game detection when only one supported executable is found
- Aspect ratio presets including:
  - 15:9
  - 16:10
  - 16:9
  - Multiple 21:9 variants
  - 32:9
- Applies patch data to:
  - Game executable offsets
  - Character camera files
  - EV beach/menu and creative-finish related files
  - BrN room/menu related files
- Automatic backup creation (first patch)
- One-click revert from backup
- Timestamped in-app logging (Show Log / Hide Log)

## Backup and Safety

Before any write operation, backups are created in the game folder and related subfolders. Revert restores from those backups.

By default, **Delete backups automatically** is enabled. After a successful revert, backup files/folders are removed unless this option is disabled first.

> Important: Revert first before applying a different ratio. Stacking applies may leave mixed values.

## Revert Changes

1. Select the game.
2. (Optional) Uncheck **Delete backups automatically** if you want to keep backup files after revert.
3. Click **Revert** and confirm.

## Logging

The bottom panel records timestamped status and file-operation details, such as:

- Which executable is patched/restored
- Which backup files are copied during revert
- File group counts processed during apply

## Requirements

- Windows
- Correct game directory layout
- Write permission for game files/folders

## Troubleshooting

- **Tool does not detect game:** Verify the executable is in the expected game root directory.
- **Apply/Revert fails:** Run as administrator if file permissions are restricted.
- **Ratio did not fully change:** Revert first, then apply the desired ratio again.
- **Missing backup files:** If backups were auto-deleted after revert, re-run apply to regenerate backups.

## Credits

This project is based on the original work and guide by **Loggey**:

- Steam guide: https://steamcommunity.com/sharedfiles/filedetails/?id=1815701855

Thanks to Loggey for the original offsets, process, and tool concept.

## Disclaimer

- This tool edits local game files only.
- Use at your own risk.
- Keep your own independent backups if desired.
