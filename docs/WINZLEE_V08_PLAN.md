# Winzlee v0.8 — development branch

Base: v0.7 validated with adapted Filee 2.2.0.

## First preview
- Explicit Windows startup option, default off; a Winzlee-only HKCU Run entry with --background.
- A forwarded --background does not open the main window.
- Filee and its shell registration are never changed.
- Credits in all languages updated to Filee 2.2.0.

## Later
- Conversion-wheel animation/accessibility refinements while protecting Ctrl+right-click.
- Consistent new image-editing windows.
- Batch rename: Photo 1, Photo 2, etc. with preview and Format choice (retain or *actually convert*, never just rename extension).
- HatchZip space-saving extraction, regular compression/extraction and compression-level controls.

## Manual Windows acceptance
1. Without opting in, no startup entry.
2. Enable from General, relaunch: state retained.
3. Re-login to Windows: tray active, main UI hidden, Ctrl+right-click works.
4. Run Winzlee.exe --background twice: one instance, no visible UI.
5. Disable: Winzlee Run value removed and Filee intact.
6. Portable moved: re-enable the switch to update its absolute path.

Do not merge to stable before manual validation.
