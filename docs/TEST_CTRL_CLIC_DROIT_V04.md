# Test Winzlee v0.4 — Ctrl + clic droit dans l'Explorateur

Cette fonctionnalité **n'existait pas** dans Winzlee v0.1 à v0.3. La v0.4 ajoute un déclencheur de **clic immédiat**, distinct du glisser-déposer et du maintien. Le raccourci utilise le système de sélection de fichiers et la roue hérités de Filee.

## Pour tester

1. Depuis l'icône dans la zone de notification, **Quitter entièrement Winzlee v0.3**. S'assurer qu'aucun `Winzlee.exe` ne tourne dans le Gestionnaire des tâches.
2. Décompresser l'archive GitHub `Winzlee-v0.4-win-x64-ctrl-right-preview` dans **un nouveau dossier** et lancer `Winzlee.exe`. La version affichée doit être **0.4.0**.
3. Dans l'Explorateur Windows (sans exécuter Winzlee en administrateur), sélectionner **un ou plusieurs fichiers de test**.
4. Garder la touche **Ctrl** enfoncée, faire **un clic droit rapide** sur les fichiers sélectionnés, sans maintenir le bouton ni déplacer la souris.
5. Résultat attendu : le menu circulaire de Winzlee apparaît avec ces fichiers. Le menu contextuel habituel de Windows **ne doit pas** apparaître en même temps.
6. Essayer le **clic droit sans Ctrl** : le menu contextuel normal doit toujours s'ouvrir.
7. Essayer la combinaison dans une autre application (navigateur, bureau de texte) : les clics droits normaux doivent rester disponibles.
8. Dans la page Raccourcis, repérer le déclencheur de type « Modificateur + clic (fichiers sélectionnés) », avec Ctrl / bouton Droit / Explorateur. Sa désactivation ou sa suppression doit être respectée.

## Limitations et précautions

- Ce comportement utilise un **hook d'entrée global natif synchrone** sur Windows. Il n'installe pas d'extension de menu Explorer et ne modifie pas les clés de registre Filee.
- Il intercepte Ctrl + clic droit lorsque la souris est dans une fenêtre de l'Explorateur, même si le dossier est vide. Dans ce cas, aucune roue ne s'ouvrira car la sélection est vide. Ce cas sera affiné après tests sur Windows.
- L'Explorateur doit normalement être la **fenêtre active** pour que Winzlee puisse lire la sélection : mieux vaut choisir les fichiers avant de tester le raccourci.
- Ne pas lancer Winzlee en administrateur : les différences de privilèges peuvent empêcher la récupération des fichiers sélectionnés par Explorer.
- Si Winzlee signale que les raccourcis sont suspendus, les réactiver dans la zone de notification.
- L'événement est volontairement limité au **processus Explorer**, pas aux navigateurs ni aux gestionnaires de fichiers tiers.
- La combinaison exacte **Ctrl** est requise ; Ctrl+Maj+clic droit n'est pas intercepté. Un clic droit standard reste un clic droit standard.
- Cette première implémentation a une validation de compilation et des tests de logique, mais seule une **vérification manuelle sur Windows 11 réel** permettra de confirmer l'interaction avec l'Explorateur.

## Si cela échoue

Relever si (a) le menu normal Windows apparaît, (b) aucun menu n'apparaît, ou (c) deux menus apparaissent ensemble. Vérifier le nom de la version v0.4 dans Winzlee, la présence éventuelle d'une autre instance Winzlee, et le texte d'état sur la page **Raccourcis**.

Les journaux sont dans `%APPDATA%\\Winzlee\\logs`. Ne transmettre que les lignes liées au déclencheur, en masquant les chemins privés.

## Prochaine étape : meilleure ressemblance avec PowerToys

La v0.3 est une vraie refonte, mais sa bannière colorée et ses cartes promotionnelles sont **plus démonstratives que l'interface de Microsoft PowerToys**. La prochaine étape visuelle doit adopter une présentation de paramètres Windows Fluent plus sobre :

- Navigation latérale neutre, séparations légères et catégories de modules ;
- Tableau de bord composé de cartes **compactes de fonctionnalités**, non de grands encarts marketing ;
- Affichage explicite des touches/gestes, de l'état actif et de **vrais** interrupteurs lorsque la fonctionnalité est activable ;
- Raccourcis clairs vers les paramètres avancés et des explications « i » pour les débutants ;
- Accent Windows discret, prise en charge des thèmes clair et sombre ;
- Préservation intégrale des moteurs de conversion et de la roue configurable.

La référence PowerToys sert d'inspiration au design, pas de nom, icône ou identité visuelle à reproduire à l'identique.
