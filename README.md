# Winzlee

**Winzlee** est une application Windows de conversion et de gestion de fichiers. Elle propose une roue d'actions accessible dans l'Explorateur avec **Ctrl + clic droit**, la conversion de formats compatibles, le renommage de masse et la gestion des archives.

> **État : v1.0.0 RC1 en préparation.** Les éditions installateur et portable ont été testées pendant le développement privé. Le code importé dans ce dépôt doit encore passer ses propres contrôles avant la publication. Aucune version stable n’est disponible actuellement.

## Fonctionnalités

- Conversion de fichiers et préréglages de conversion.
- Roue de commandes dans l'Explorateur Windows (Ctrl + clic droit, Échap pour fermer et Tab pour retourner la roue).
- Compression et extraction d'archives, dont le mode HatchZip avec avertissement lorsqu'une opération modifie ou consomme une archive source.
- Renommage de masse avec aperçu et options de destination.
- Suppression facultative des originaux après vérifications, via la Corbeille Windows.
- Paramètres, thème, démarrage silencieux et zone de notification.

## Installation et distribution

**Deux éditions Windows x64 sont prévues pour la v1.0.0 :**
- **Installateur EXE** : installation, raccourcis et désinstallation.
- **ZIP portable** : extraction dans un dossier, sans installation.

Les deux éditions ont été validées lors de la préparation privée de la RC1. Les futurs téléchargements officiels seront proposés dans les [Releases de Winzlee](https://github.com/KylianProvot/Winzlee-Public/releases) après leur validation ; aucun artefact temporaire ne constitue une publication officielle. Pour les essais, utiliser des copies de fichiers et conserver une sauvegarde des paramètres. Les mises à jour doivent préserver les préréglages et données utilisateur; leur validation réelle constitue un critère bloquant de la v1.0.0.

La publication restera privée tant que les derniers contrôles Windows et la confidentialité de l'historique Git ne seront pas validés.

**Suivi de la publication :** [liste de vérification](RELEASE-CHECKLIST.md) · [guide de création de la Release](RELEASING.md) · [feuille de route à jour](docs/ROADMAP.md). Le workflow RC1 peut préparer un brouillon de Release après ses tests ; il ne publie aucun téléchargement sans validation finale.

### Coexistence avec Filee

La RC1 est préparée pour fonctionner **indépendamment de Filee** : identité d'installation Windows, intégration au menu contextuel Windows 11, paramètres et téléchargements de moteurs séparés. Les moteurs de l'ancien dossier `%LOCALAPPDATA%\Filee\engines` peuvent être consultés en lecture pour maintenir les conversions d'une ancienne préversion Winzlee, mais Winzlee ne doit ni les mettre à jour ni les supprimer. Pour gérer les moteurs dans les paramètres de Winzlee, leur réinstallation dans son propre dossier pourra être nécessaire.

**Validation :** les tests automatisés Windows de la RC1 sont réussis ; le propriétaire a confirmé le fonctionnement de l'installation, des conversions, de la désinstallation et du ZIP portable sur son PC. La coexistence réelle avec une installation Filee et la mise à jour depuis une installation précédente nécessitent encore des essais ciblés.

## Création et crédits

**Winzlee — Kylian Provot ([KylianProvot](https://github.com/KylianProvot))**, étudiant en **2ᵉ année de BUT Métiers du Multimédia et de l'Internet (MMI)**.

Winzlee s'appuie largement sur les projets open source [Filee — KnifeLemon](https://github.com/KnifeLemon/Filee) et [HatchZip — xevrion](https://github.com/xevrion/hatchzip). Il utilise notamment des composants de [7-Zip](https://www.7-zip.org/). Développement et amélioration assistés par **ChatGPT (OpenAI)**.

L'attribution des auteurs d'origine, les licences et les autres composants sont détaillés dans [LICENSE](app/LICENSE) et [THIRD-PARTY-NOTICES.md](app/THIRD-PARTY-NOTICES.md). Winzlee n'est affilié ni à Filee, ni à HatchZip, ni à Microsoft PowerToys, ni à OpenAI.

## Code et contributions

Le code de l'application se trouve dans `app/`. Les fichiers de référence hérités de Filee (notamment les README multilingues) ne décrivent pas forcément le périmètre de distribution Windows de Winzlee. L'objectif de la RC1 est d'unifier la documentation, d'auditer les mises à jour et de conserver le comportement existant sans régression. Les commandes de packaging transmettent explicitement `-Version 1.0.0-rc1` ; la version de développement par défaut est également `1.0.0-rc1`.

Projet : [github.com/KylianProvot/Winzlee-Public](https://github.com/KylianProvot/Winzlee-Public).

Pour signaler un bug ou proposer une amélioration, utiliser les [issues Winzlee](https://github.com/KylianProvot/Winzlee-Public/issues/new/choose). Pour une faille de sécurité, consulter [SECURITY.md](SECURITY.md) et ne pas publier les détails sensibles dans une issue.

**Compilation depuis les sources :** installer le SDK .NET 10, puis lancer `dotnet restore app/Filee.slnx` et `dotnet build app/Filee.slnx -c Release`. La préparation des installeurs Windows est documentée par les scripts `app/build/`.
