# Winzlee v0.12.0 — installation and update checks

This is a preview branch, based on v0.11.0. Do not merge, tag, or publish a release before Windows user validation.

## Optional engine size reference (Windows x64)

The **download** values below are computed from the pinned component archive sizes in `src/Filee.Engines/Infrastructure/engines.json` (decimal MB). The **installed** values are engineering estimates, not measurements on every PC. They exclude the Winzlee app itself and temporary download/unpacking space.

| Optional engine | Download | Estimated installed disk |
| --- | ---: | ---: |
| LibreOffice + H2Orestart + Java runtime | 422.9 MB | 1,265 MB |
| Pandoc | 42.1 MB | 236 MB |
| Ghostscript + runtime | 20.0 MB | 31 MB |
| FFmpeg | 99.9 MB | 272 MB |
| Calibre | 226.5 MB | 663 MB |
| Tesseract | 50.2 MB | 168 MB |

All six together represent about **861.6 MB download** and **2.64 GB installed disk**, before temporary files; none is preselected. When the component catalog changes, recompute values before release.

## Windows manual gates

- [ ] Portable preview on a PC with no prior Winzlee data: opens without an engine download prompt.
- [ ] General > Engines continues to install, remove, and verify optional packages.
- [ ] New Windows Setup Options sheet: no optional package selected at initial render, including Pandoc.
- [ ] Each item explains the extra formats; LibreOffice does not imply ordinary Word/Excel/PDF need it.
- [ ] Per-package download and installed figures display, and the selected totals change after ticking/unticking.
- [ ] Installer can install/update/uninstall from a sandbox without touching real Windows registry or user files.
- [ ] On a repository without a public Release, Check Now says no public Release (not "up to date").
- [ ] On network failure, Check Now says verification is unavailable.
- [ ] On a newer signed stable Release, installed builds validate SHA-256 and Authenticode before launching Setup.
- [ ] Portable update opens Winzlee's own GitHub Releases page; it does not invoke an automatic installer.
- [ ] Verify an actual Winzlee-branded installer package and update **before** public v1.0.0; the legacy Filee Setup packaging/names still require a separate compatibility audit.

The GitHub Actions test of Setup uses a **stub app** in sandbox mode. It validates installer mechanics, **not** a complete user-installable Winzlee setup package. Treat this distinction as a release gate.
