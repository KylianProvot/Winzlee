# Publier Winzlee v1.0.0 RC1

> Guide du mainteneur. La Release **n'est pas publiée automatiquement**.

## 1. Préparer le brouillon

**État au 9 novembre 2026 :** le job Windows RC1 du dépôt de distribution a réussi (exécution `41417869823`) et les fichiers ont été sauvegardés localement par le mainteneur. Les artefacts Actions ont ensuite été supprimés pour libérer le quota. **Ne pas relancer une compilation seulement pour créer le brouillon.** La création automatique a échoué avec HTTP 403 ; utiliser en priorité la méthode manuelle ci-dessous.

### Méthode manuelle à partir des fichiers RC1 déjà vérifiés

1. Extraire sur le PC le ZIP `Winzlee-1.0.0-RC1-installable-packages` sauvegardé depuis GitHub Actions.
2. Vérifier la présence des trois fichiers attendus ci-dessous et comparer leurs empreintes à `SHA256SUMS.txt` avec PowerShell (`Get-FileHash -Algorithm SHA256`).
3. Dans [Nouvelle Release](https://github.com/KylianProvot/Winzlee-Public/releases/new), créer le tag `v1.0.0-rc1`, idéalement en ciblant le commit testé `14151a4497a12a0dfcc9519a9603a4a67210aa38`.
4. Donner le titre **Winzlee v1.0.0 RC1**, reprendre le contenu de `docs/RELEASE-NOTES-v1.0.0-rc1.md`, joindre les trois fichiers, marquer **préversion** et cliquer **Save draft** (pas Publish).
5. Examiner les fichiers du brouillon et effectuer les tests de la section 2.

### Pour une nouvelle compilation future (non nécessaire à la création du brouillon RC1)

Dans [Actions](https://github.com/KylianProvot/Winzlee-Public/actions/workflows/release-rc1.yml) :

1. Ouvrir **Winzlee RC1 - Windows validation, packages and draft release**.
2. Cliquer **Run workflow**.
3. Choisir la branche `main` et cocher **prepare_draft_release**.
4. Démarrer le workflow et attendre la réussite du job Windows, puis du job de création de brouillon.

Sans la case cochée, le workflow effectue seulement les tests et produit des **artefacts Actions temporaires**.

Le job Windows restaure et compile la solution, exécute les six projets de tests, contrôle l'installateur en sandbox, teste les dépendances d'archives et construit les deux distributions RC1. Le second job récupère les fichiers de **cette même exécution**, vérifie leurs noms, l'intégrité SHA-256 et le contenu essentiel du ZIP, puis crée une **GitHub Release en brouillon**, marquée préversion. Il n'écrase pas une Release du même tag déjà existante.

## 2. Vérifier le brouillon

Dans [Releases](https://github.com/KylianProvot/Winzlee-Public/releases), ouvrir le brouillon `v1.0.0-rc1`.

Fichiers attendus :

- `Winzlee-1.0.0-rc1-win-Setup.exe`
- `Winzlee-1.0.0-rc1-win-Portable.zip`
- `Winzlee-1.0.0-rc1-SHA256SUMS.txt`

Vérifier les SHA-256 sur un téléchargement **depuis la Release** (pas seulement depuis les artefacts Actions). Sous PowerShell, la commande `Get-FileHash -Algorithm SHA256 .\Winzlee-1.0.0-rc1-win-Setup.exe` permet de comparer l'empreinte à celle publiée.

Tester ensuite les deux téléchargements sous Windows :

- installation propre, lancement et conversions sur des fichiers de test ;
- installation par-dessus une ancienne version : préservation des paramètres et préréglages ;
- cohabitation avec Filee installé, y compris son intégration Explorateur ;
- désinstallation propre sans supprimer des données utilisateur ;
- lancement de la version portable dans un dossier séparé ;
- affichage de la version, identité Winzlee et absence de messages inattendus ;
- documentation et licences embarquées, signatures et éventuels avertissements Windows.

Le workflow automatise des contrôles, **mais ne remplace pas ces essais sur un vrai poste**.

## 3. Contrôler l'ouverture du dépôt

- Vérifier que le dépôt ne contient ni secret, fichier personnel, paramètre local ni information qui ne doit pas devenir publique.
- Examiner aussi les anciennes versions et l'historique (le dépôt a été réinitialisé avec un commit racine propre).
- Vérifier les licences et crédits de Filee, HatchZip et des composants distribués.
- Maintenir `KylianProvot/Winzlee` **privé**.

Ne changer la visibilité de **Winzlee-Public** qu'après ces vérifications.

## 4. Publication finale

Une fois les contrôles validés, passer **Winzlee-Public** en public depuis **Settings → General → Danger Zone → Change repository visibility** (opération manuelle avec les droits administrateur). Ouvrir le brouillon de Release et cliquer **Publish release** en conservant le statut de **préversion**.

Tester les téléchargements dans un navigateur déconnecté de GitHub. Les liens directs vers l'installateur et le ZIP portable ne sont considérés comme valides qu'après la publication et ce test.

**Ne pas appeler cette préversion « stable ».** La v1.0.0 stable nécessitera ses propres binaires recompilés depuis son commit final, tests et empreintes.

## En cas d'échec

- Si le job Windows échoue, corriger le problème avant toute création de Release.
- Si le job de brouillon échoue après la réussite des tests, consulter son journal ; ne pas recréer les tags ou écraser les fichiers à l'aveugle.
- Si une Release du même tag existe déjà, le workflow refuse son remplacement : examiner d'abord son statut et ses fichiers.
- Si une préversion échoue lors des tests Windows, la laisser privée ou en brouillon et corriger avant publication.

## 5. Test de détection des mises à jour sur un PC déjà équipé

**Ne pas désinstaller l'ancienne version pour ce test.** Noter au préalable le numéro affiché dans **À propos** et sauvegarder les paramètres et préréglages.

Winzlee vérifie automatiquement les mises à jour lorsqu'elles sont activées, puis environ toutes les six heures ; il propose aussi **Vérifier maintenant** dans l'interface. La vérification actuelle utilise l'API GitHub `/releases/latest`, qui renvoie la **dernière Release stable** et **ignore les préversions** : publier `v1.0.0-rc1` en tant que préversion ne fera pas apparaître cette mise à jour dans une ancienne application.

**Point de compatibilité important :** les anciennes compilations utilisaient une comparaison qui ignorait les suffixes `-rc1`. Si l'exécutable actuellement installé indique `1.0.0-rc1`, il peut considérer `1.0.0` comme identique et **ne pas détecter** la stable. Le code source a été corrigé pour les futures compilations, mais cette correction ne peut pas agir rétroactivement sur l'exécutable déjà installé. Dans ce cas, installer manuellement la version stable par-dessus la RC1, **sans désinstaller ni effacer les réglages**, pour vérifier leur conservation ; les futures mises à jour seront alors testables normalement.

Pour un test réel de détection automatique, il faut réunir ces conditions :

- la Release stable est publiée dans un dépôt **public** ; un brouillon ou une préversion ne suffit pas ;
- la version publiée est strictement plus récente selon le comparateur **du binaire actuellement installé** ;
- le PC a accès à GitHub et l'option de vérification des mises à jour est activée ;
- le bon EXE et `SHA256SUMS.txt` sont attachés à la Release ;
- le nouveau programme est installé sans supprimer les données utilisateur.

Vérifier ensuite les favoris, préréglages, raccourcis, commandes de conversion, intégration Explorateur et redémarrage automatique. La version portable ne s'installe pas automatiquement : elle ouvre la page des téléchargements.
