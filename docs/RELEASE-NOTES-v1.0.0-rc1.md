# Winzlee v1.0.0 RC1 — préversion Windows x64

**Cette version est une candidate (RC1), et non la version stable.**

Les préversions ne sont pas proposées par le canal de mise à jour automatique actuel : cette RC1 se télécharge depuis la page Releases une fois publiée.

Winzlee permet de convertir et gérer des fichiers sous Windows. La roue d'actions s'ouvre dans l'Explorateur avec **Ctrl + clic droit**.

## Téléchargements

- **Winzlee-1.0.0-rc1-win-Setup.exe** : version avec installateur et désinstalleur.
- **Winzlee-1.0.0-rc1-win-Portable.zip** : version portable, à extraire dans un dossier distinct.
- **Winzlee-1.0.0-rc1-SHA256SUMS.txt** : empreintes SHA-256 pour vérifier les téléchargements.

## Fonctionnalités principales

- Conversion de fichiers et préréglages, avec choix de format et de qualité selon les conversions.
- Roue d'actions accessible depuis l'Explorateur Windows.
- Renommage de masse avec aperçu.
- Création et extraction d'archives, avec prise en charge de HatchZip.
- Options de thème, de langue et de démarrage silencieux.

## Points restant à vérifier

Les tests automatisés Windows et les essais d'installation, de conversion, de désinstallation et de version portable ont été concluants pendant le développement privé. **Avant de considérer Winzlee comme stable**, il reste à tester la mise à jour avec conservation de paramètres/préréglages, la coexistence avec Filee et les cas d'échec de l'installation.

Il est conseillé de conserver une copie de vos données et de tester cette préversion sur des fichiers non critiques. Certaines opérations sur les archives ou les originaux peuvent être destructrices : lisez les confirmations affichées.

## Crédits et licence

Winzlee est développé par Kylian Provot à partir du projet open source [Filee](https://github.com/KnifeLemon/Filee), avec notamment [HatchZip](https://github.com/xevrion/hatchzip). Voir [la licence](https://github.com/KylianProvot/Winzlee-Public/blob/v1.0.0-rc1/app/LICENSE) et [les mentions tierces](https://github.com/KylianProvot/Winzlee-Public/blob/v1.0.0-rc1/app/THIRD-PARTY-NOTICES.md).

## Signaler un problème

Utilisez [les issues GitHub](https://github.com/KylianProvot/Winzlee-Public/issues/new/choose) pour les bugs et suggestions. Ne communiquez pas de fichiers personnels ni de données sensibles dans une issue publique.
