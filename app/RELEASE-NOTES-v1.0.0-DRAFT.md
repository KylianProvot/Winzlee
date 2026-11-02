# Winzlee v1.0.0 — notes de publication (BROUILLON)

> **Ne pas publier sans validation explicite.** Document préparatoire à vérifier avec la version finale des exécutables et des sommes SHA-256.

## Présentation

Winzlee est une application Windows de conversion et de gestion de fichiers, avec roue d'actions accessible depuis l'Explorateur par **Ctrl + clic droit**.

## Principales fonctionnalités

- Conversion de fichiers avec préréglages et options de format et de qualité.
- Renommage de masse avec aperçu.
- Création et extraction d'archives (notamment ZIP/7z et extraction via HatchZip).
- Gestion des originaux avec suppression facultative et précautions avant mise à la Corbeille.
- Options de langue, de thème, de démarrage en arrière-plan et d'intégration Windows.

## Éditions Windows x64

- **Installateur EXE** : `Winzlee-1.0.0-win-Setup.exe` (nom final à confirmer lors du packaging stable).
- **ZIP portable** : `Winzlee-1.0.0-win-Portable.zip`.
- Empreintes : `Winzlee-1.0.0-SHA256SUMS.txt`.

La RC1 validée utilise les noms `Winzlee-1.0.0-rc1-win-Setup.exe` et `Winzlee-1.0.0-rc1-win-Portable.zip`.

## Validation disponible au 1er novembre 2026

- [Exécution GitHub Actions RC1 réussie](https://github.com/KylianProvot/Winzlee/actions/runs/41290679277) : compilation complète, six suites de tests, essais d'installation/mise à jour/désinstallation en sandbox et fabrication des packages.
- Essais utilisateur sur Windows : installation, exécution, conversions, désinstallation et utilisation de la version portable sans anomalie rapportée.
- **Reste à vérifier avant stable** : mise à jour à partir d'une ancienne installation avec paramètres, coexistence réelle avec Filee, contrôle final des métadonnées/licences et éventuels scénarios d'échec.

## Crédits

Winzlee : **Kylian Provot** ([GitHub](https://github.com/KylianProvot)), étudiant en 2ᵉ année de BUT MMI. Basé sur les contributions open source de [Filee](https://github.com/KnifeLemon/Filee) et intégrant [HatchZip](https://github.com/xevrion/hatchzip). Développement assisté par ChatGPT (OpenAI). Voir `app/LICENSE` et `app/THIRD-PARTY-NOTICES.md`.

Winzlee n'est officiellement affilié à aucun de ces projets ou organisations.

## Retour arrière

Conserver une copie des paramètres et de l'ancienne version avant toute mise à jour. Tester d'abord le ZIP portable dans un dossier séparé. En cas de régression, arrêter Winzlee, désinstaller la nouvelle édition si nécessaire, puis restaurer la version et les paramètres sauvegardés. **Ne pas supposer qu'une désinstallation restaure automatiquement une version antérieure.**

## Publication

- [ ] Finaliser les tests Windows ciblés et les éventuelles corrections.
- [ ] Choisir la version définitive et recompiler explicitement avec `-Version 1.0.0`.
- [ ] Vérifier les deux exécutables, métadonnées, licences et sommes SHA-256 finales.
- [ ] Obtenir l'accord explicite du propriétaire.
- [ ] Seulement ensuite : créer la GitHub Release et joindre les fichiers.
