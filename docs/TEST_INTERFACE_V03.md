# Test Winzlee 0.3 — la différence doit être visible

Cette version corrige le défaut de la v0.2 : la nouvelle interface était trop proche de Filee pour se distinguer au premier regard.

## Procédure indispensable

**Important : Winzlee v0.1, v0.2 et v0.3 partagent la même instance unique.**
Si Winzlee v0.2 est déjà ouvert ou reste en zone de notification, ouvrir v0.3 réactive v0.2 au lieu de démarrer la nouvelle version.

1. Enregistrer son travail et **Quitter** Winzlee v0.2 depuis l'icône de la zone de notification (ne pas seulement fermer la fenêtre).
2. Dans le Gestionnaire des tâches Windows, vérifier qu'il n'existe plus de processus `Winzlee.exe`.
3. Télécharger l'artifact `Winzlee-v0.3-windows-x64-preview` du workflow GitHub Actions le plus récent.
4. L'extraire dans un **nouveau dossier**, distinct de celui de Winzlee v0.2 et de Filee.
5. Exécuter `Winzlee.exe`. Aucune installation ni droit administrateur requis.
6. Vérifier dans la navigation que la version indiquée est **0.3.0**.

## Repères visuels non ambigus

- **Rail de navigation bleu nuit** à gauche, avec une vignette W bleue et des libellés blancs.
- **En-tête d'espace de travail** en haut de la zone principale, séparé de la navigation.
- Sur **Accueil**, grande **bannière bleue « Tes fichiers, convertis simplement. »** et bouton « Choisir des fichiers ».
- Trois **cartes d'accès rapide** pour Convertir, Roue de conversion et Raccourcis.
- La zone de glisser-déposer est plus basse et moins envahissante.
- Sous **Général**, les paramètres sont regroupés en **Application**, **Fichiers et conversion** et **Historique et données**.
- L'application devrait rester utilisable en thème clair comme en thème sombre.

## Vérifications fonctionnelles sur des fichiers sans importance

- « Choisir des fichiers » ouvre le dialogue Windows et la roue existante après sélection.
- La carte « Roue de conversion » ouvre l'éditeur de la roue.
- La carte « Raccourcis » ouvre la page des déclencheurs.
- La sélection manuelle des préréglages et la conversion simple PNG vers JPG fonctionnent encore.
- Filee original fonctionne séparément, sans copier ni perdre ses données.
- L'application quitte correctement via la zone de notification.

## Limites

- Ce build n'est **pas signé numériquement**.
- L'icône Windows `filee.ico` reste une ressource provisoire ; une icône Winzlee définitive est prévue.
- Les intégrations Explorateur Winzlee sont encore désactivées pour protéger Filee.
- L'intégration HatchZip et le Ctrl + clic droit ne sont pas encore ajoutés.
- Les traductions des nouvelles chaînes sont complètes en français et en anglais ; les autres langues utilisent un texte de secours en anglais.

Ne pas utiliser cette version de développement pour des conversions destructives ou des fichiers irremplaçables.
