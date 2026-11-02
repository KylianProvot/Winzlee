# Winzlee — publication checklist (v1.0.0 RC1)

This repository is **not yet a published release**. Do not advertise a download until the installer and portable archive have been attached to a GitHub Release and tested.

## Before publishing

- [x] Import current source as a single clean initial commit (no temporary import history).
- [x] Run Windows automated RC1 build and test workflow (private development repository, 2026-11-01).
- [x] Manual Windows checks reported for install, conversions, uninstall and portable use.
- [ ] Run the **public repository's own** Windows workflow on its current commit, and review test logs.
- [ ] Check upgrade from an existing Winzlee installation with saved settings and presets.
- [ ] Validate RC-to-stable version comparison tests in the public repository's Windows workflow; older RC1 installs may still require a manual in-place upgrade.
- [ ] Check actual coexistence with Filee and Explorer integration.
- [ ] Review included source, assets, personal information, licenses and third-party attributions for public disclosure. Review Git history and workflow logs too.
- [ ] Produce matching installer and portable binaries from the final release commit using the public repository's own Windows workflow.
- [ ] Confirm filenames, version metadata, SHA-256 checksums, signatures if applicable, and test freshly downloaded release assets on Windows.
- [ ] Publish a **pre-release** tagged `v1.0.0-rc1` if RC1 is intended for testing; only publish `v1.0.0` as stable after outstanding checks succeed.
- [ ] Switch repository visibility to public only after the disclosure audit is complete.
- [ ] Test release links from a signed-out browser, then add the correct installer and portable links to the website.

## Expected RC1 assets

- `Winzlee-1.0.0-rc1-win-Setup.exe`
- `Winzlee-1.0.0-rc1-win-Portable.zip`
- `Winzlee-1.0.0-rc1-SHA256SUMS.txt`

The manual workflow [`.github/workflows/release-rc1.yml`](.github/workflows/release-rc1.yml) builds these packages as **Actions artifacts**. When its `prepare_draft_release` checkbox is enabled on `main`, it also stages checked packages as a **draft** GitHub pre-release. A draft is not a published, publicly downloadable Release. See [RELEASING.md](RELEASING.md) for the launch and verification procedure.

## Maintainer note

The GitHub connector used during preparation can edit repository files but does not expose direct workflow dispatch or visibility-changing actions. Release creation with its assets can now be performed by the explicitly triggered GitHub Actions workflow, in **draft mode only**. The final approval and public publication remain manual. Never claim publication is complete until the Release, assets and public download URLs have been verified.
