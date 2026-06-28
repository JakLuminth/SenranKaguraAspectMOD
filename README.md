# Senran Kagura EV/BrN Aspect Ratio MOD Tool (v1.1)

A Windows desktop utility for applying widescreen and ultrawide aspect-ratio fixes to:

- **SENRAN KAGURA Estival Versus (EV)**
- **SENRAN KAGURA Burst Re:Newal (BrN)**

The app automates hex edits that would otherwise need to be done manually across multiple game files.

## Credit

This project is based on the original work and guide by **Loggey**:

- Steam guide: https://steamcommunity.com/sharedfiles/filedetails/?id=1815701855

Thank you to Loggey for the original offsets, process, and tool concept.

## What the application does

- Lets you choose a target game (EV or BrN)
- Lets you choose an aspect ratio preset (16:10, 15:9, 16:9, multiple 21:9 options, 32:9)
- Applies the selected ratio to:
  - Game executable offsets
  - Character camera files
  - EV beach/menu and creative-finish related files
  - BrN room/menu related files
- Creates backup files before patching (first run)
- Can revert modified files from backup

## Safety / backup behavior

Before writing changes, the app creates backups in the game folder and related subfolders. Revert uses those backups to restore original files.

> Important: Revert before applying a different ratio, otherwise the new ratio may not be fully reflected.

## How to use

1. Place the executable in the root game folder.
2. Launch the tool.
3. Select the game.
4. Select the desired aspect ratio.
5. Click **Apply** and confirm.
6. Wait for completion (status + log window show progress).

To undo changes:

1. Select the game.
2. Click **Revert** and confirm.

## Logging

The bottom log panel records timestamped status and file-operation details, including:

- Which executable is being patched/restored
- Which backup files are copied during revert
- File group counts being processed during apply

## Requirements

- Windows
- Correct game directory layout
- Write permission to game files/folders

## Notes

- This tool edits local game files only.
- Use at your own risk.
- Keep your own independent backups if desired.
