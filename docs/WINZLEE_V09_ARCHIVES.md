# Winzlee v0.9 — Archives + HatchZip

Base : branche v0.8 (démarrage silencieux validé), sans modifier v0.8.

## Choix demandé par l'utilisateur
Pour chaque extraction **depuis la roue Winzlee**, une fenêtre permet de choisir :
- **Extraction classique**, par défaut, avec le moteur Winzlee/Filee et conservation de l'archive.
- **Économie d'espace (HatchZip)** : option active seulement si le moteur embarqué est disponible et que le type de fichier est pris en charge. Une case d'approbation supplémentaire est exigée avant de lancer le mode destructif.

L'archive d'origine est réduite puis supprimée au fur et à mesure ; une interruption laisse un état reprenable via HatchZip, et doit être reprise ou restaurée avec HatchZip avant emploi dans d'autres outils. Le programme effectue un check non destructif avant d'agir, puis `extract --verify-first --yes` **uniquement** après le choix explicite. Si plusieurs archives ont été sélectionnées, elles sont vérifiées puis traitées l'une après l'autre. Une panne après le début peut avoir modifié certaines archives : ne pas supprimer les fichiers `.hatchzip` de reprise.

## Aucun 7-Zip / WinRAR à installer séparément

La préversion portable inclut également la console **7-Zip 26.03**, obtenue via le script de Filee avec version et SHA-256 fixés. Les moteurs Winzlee pourront ainsi extraire les formats pris en charge et compresser en 7z dès le téléchargement du ZIP. C'est un composant interne et non une installation système de 7-Zip.

## Compression existante réutilisée
Quand une action ZIP, 7z, TAR ou TAR.GZ est choisie dans la roue, la fenêtre propose le format et les niveaux Store/Fast/Normal/Maximum de Filee 2.2.0. Le moteur Winzlee/Filee réalise la compression ; les presets enregistrés ne sont pas modifiés.

## Empaquetage / attribution
Le workflow Windows télécharge la **version CLI v0.1.7 de HatchZip, Windows x64** depuis le dépôt de l'auteur, vérifie son SHA-256 fixé, puis copie `hatchzip.exe` sous `engines/hatchzip/` dans le ZIP portable. Aucun exécutable HatchZip n'est téléchargé à l'exécution ; aucune installation séparée n'est demandée. Les licences MIT, Apache-2.0 et UnRAR sont incluses dans le dossier du moteur et les crédits sont ajoutés aux notices.

Les conversions lancées par les dossiers surveillés / navigateur restent automatiques et ne reçoivent pas de fenêtre modale. Pas de gestion native des mots de passe HatchZip dans cette préversion : un fichier chiffré peut échouer avec un message, son archive étant préservée si le pré-contrôle refuse.

## Contrôles sur Windows
1. Sélectionner archive.zip dans l'Explorateur → Ctrl + clic droit → Extraire : choix Classique/HatchZip.
2. Choisir Classique : les fichiers sont extraits ; archive.zip est conservée.
3. Choisir HatchZip sans cocher la confirmation : le bouton Extraire est désactivé.
4. Cocher, extraire une **archive de test** ; vérifier les données et la disparition attendue de l'archive.
5. Annuler au milieu d'une grande archive de test ; confirmer qu'il reste les fichiers de reprise, puis lancer à nouveau avec les mêmes options.
6. Compresser des fichiers en ZIP puis 7z avec des niveaux différents.
7. Vérifier Ctrl + clic droit et l'ouverture silencieuse au démarrage Windows.
8. Vérifier un dossier dont le chemin contient des espaces ; tester l'absence de HatchZip (mode classique utilisable).

La branche et la PR restent en brouillon jusqu'à ces essais.
