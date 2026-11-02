# Politique de sécurité — Winzlee (préversion)

## Versions prises en charge

Winzlee v0.11 est en cours de stabilisation. **Aucune version stable v1.0 n'est encore publiée.**
Les versions d'essai sont destinées à la validation sur des copies de fichiers ; la prise en charge de sécurité d'une version stable sera précisée lors de sa publication.

## Signaler une faille

N'exposez pas publiquement un exploit, un fichier malveillant ou des données personnelles.
Utilisez de préférence l'option de [signalement privé GitHub de ce dépôt](https://github.com/KylianProvot/Winzlee/security/advisories/new), **si elle est activée et accessible**.
Si elle n'est pas accessible, contactez les mainteneurs par un canal privé avant de communiquer des détails techniques sensibles.

Indiquez la version de Winzlee, la version de Windows, les étapes pour reproduire le problème, et si l'opération peut modifier ou supprimer des fichiers. Ne joignez pas d'originaux personnels.

## Périmètre prioritaire

- Lecture et conversion de fichiers non fiables ; archives endommagées ou malveillantes.
- Refus d'écraser un fichier existant, contrôles d'emplacements, et Corbeille Windows.
- Extraction classique/HatchZip, annulation, renommage et dossiers surveillés.
- Intégration à l'Explorateur et démarrage automatique.
- Provenance, contrôles SHA-256 et licences des dépendances fournies.

Les logiciels tiers embarqués restent régis par leurs propres politiques de sécurité. Un risque dans une version de moteur intégrée à Winzlee doit également être porté à l'attention du projet.

## Distribution

Ne pas supposer qu'un ZIP de préversion est signé ou qu'il bénéficie d'une mise à jour automatique.
La checklist et les résultats de validation font autorité avant toute release officielle.
