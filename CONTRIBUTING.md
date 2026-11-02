# Contribuer à Winzlee

Merci de votre intérêt pour Winzlee, une application Windows de conversion et de gestion de fichiers.

## Signaler un problème

Utilisez les [issues GitHub](https://github.com/KylianProvot/Winzlee-Public/issues/new/choose) et indiquez la version de Winzlee, Windows, l'édition installateur ou portable et les étapes permettant de reproduire le problème.

Évitez d'envoyer des documents confidentiels, des chemins personnels et des informations d'identification. Pour les vulnérabilités, consultez [SECURITY.md](SECURITY.md) et privilégiez un signalement privé.

## Proposer une modification

Ouvrez d'abord une issue pour les modifications importantes afin de discuter de leur intérêt et de leur périmètre. Les corrections petites et ciblées peuvent être proposées par pull request. Décrivez la modification, les scénarios testés et ses éventuelles implications sur les moteurs de conversion.

Le code principal se trouve dans `app/` et utilise .NET 10. La solution de développement est `app/Filee.slnx` : certains noms de projets et espaces de noms portent encore le nom Filee pour préserver la compatibilité avec le code amont.

## Respect du travail d'origine

Winzlee dérive de [Filee](https://github.com/KnifeLemon/Filee) et utilise notamment [HatchZip](https://github.com/xevrion/hatchzip). Conservez les licences, notices et crédits existants. Consultez `app/LICENSE` et `app/THIRD-PARTY-NOTICES.md` avant d'ajouter ou de redistribuer un composant.

Les changements affectant l'installation, la désinstallation, les paramètres utilisateur et l'intégration Windows doivent faire l'objet de tests de non-régression, car la coexistence avec Filee est une contrainte importante.
