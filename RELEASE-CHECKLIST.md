# Winzlee — publication checklist (v1.0.0 RC1)

This repository is **not yet a published release**. Do not advertise a download until the installer and portable archive have been attached to a GitHub Release and tested.

## Before publishing

- [x] Import current source as a single clean initial commit (no temporary import history).
- [x] Run Windows automated RC1 build and test workflow (private development repository, 2026-11-01).
- [x] Manual Windows checks reported for install, conversions, uninstall and portable use.
- [x] Run the public distribution repository's full Windows RC1 workflow (2 November 2026, run `41417869823`): Windows job succeeded; the subsequent draft-release job failed with HTTP 403 permissions.
- [ ] Check upgrade from an existing Winzlee installation with saved settings and presets.
- [x] Validate RC-to-stable version comparison regression tests in the public repository's Windows workflow; older installed RC1 binaries can still require a manual in-place upgrade.
- [ ] Check actual coexistence with Filee and Explorer integration.
- [ ] Review included source, assets, personal information, licenses and third-party attributions for public disclosure. Review Git history and workflow logs too.
- [x] Produce matching installer and portable RC1 binaries from tested source commit `14151a4497a12a0dfcc9519a9603a4a67210aa38` using the public repository's Windows workflow; subsequent commits modify publication tooling and documentation, not the RC1 app binaries.
- [ ] Confirm the saved RC1 packages have the exact expected names, hashes and version metadata; test freshly downloaded **Release** assets on Windows after creating the Release. Hashes of the original Actions artifact were verified in CI.
- [ ] Publish a **pre-release** tagged `v1.0.0-rc1` if RC1 is intended for testing; only publish `v1.0.0` as stable after outstanding checks succeed.
- [ ] Switch repository visibility to public only after the disclosure audit is complete.
- [ ] Test release links from a signed-out browser, then add the correct installer and portable links to the website.

## Expected RC1 assets

- `Winzlee-1.0.0-rc1-win-Setup.exe`
- `Winzlee-1.0.0-rc1-win-Portable.zip`
- `Winzlee-1.0.0-rc1-SHA256SUMS.txt`

The manual workflow [`.github/workflows/release-rc1.yml`](.github/workflows/release-rc1.yml) builds these packages as **temporary Actions artifacts**. Its automated draft-creation step encountered HTTP 403 in November 2026. The tested package artifacts were intentionally deleted from GitHub Actions to free storage **after the maintainer saved a local copy**. To create the RC1 Release without a new build, use that saved package archive and follow [RELEASING.md](RELEASING.md); do not attempt to promote the deleted artifacts. A draft is not a published, publicly downloadable Release. See [RELEASING.md](RELEASING.md) for the launch and verification procedure.

## Maintainer note

The GitHub connector used during preparation can edit repository files but does not expose direct workflow dispatch or visibility-changing actions. Automatic draft creation requires GitHub Actions write permission and previously failed with HTTP 403; manual draft creation from the saved and verified packages is the fallback. The final approval and public publication remain manual. Never claim publication is complete until the Release, assets and public download URLs have been verified.


## Publication de la RC1 — 9 novembre 2026

- Dépôt `KylianProvot/Winzlee` devenu public (anciennement `Winzlee-Public`).
- GitHub Release [`v1.0.0-rc1`](https://github.com/KylianProvot/Winzlee/releases/tag/v1.0.0-rc1) publiée avec statut préversion.
- Installateur, archive portable et SHA256SUMS joints ; SHA-256 des deux binaires conformes aux digests GitHub.
- Site statique de téléchargement dans `docs/`; son hébergement via GitHub Pages doit être activé dans les réglages du dépôt.
- La version stable v1.0.0 nécessite encore ses propres validations.
