# Winzlee v0.7 — intégration de Filee 2.2.0

Branche : `feature/filee-2.2-upgrade-v0.7` depuis `feature/fluent-visual-v0.6`.

Cette branche intègre **sélectivement et par fusion à trois versions** les modifications de Filee 2.2.0 issues du tag officiel, en utilisant Filee 2.1.0 comme ancêtre commun.

Les fichiers de marque, la fenêtre principale, le thème Fluent et surtout les déclencheurs Windows `Ctrl + clic droit` sont explicitement protégés. Les conflits dans les fichiers partagés favorisent les personnalisations Winzlee, et les tests Windows sont obligatoires avant de considérer l'archive portable comme validée.

Le workflow `.github/workflows/winzlee-v07-filee220.yml` est un outil de migration ponctuel : il copie la source officielle sur la branche v0.7, publie le commit résultant, puis compile et teste le résultat. Les statistiques et exceptions de fusion sont écrites dans `docs/UPSTREAM_FILEE_220_MERGE.json`.

Tests de recette indispensables : `Ctrl + clic droit` dans l'Explorateur Windows 11, choix des fichiers, roue, préréglages, historique, conversions longues, nouveaux formats, nouveaux outils image, thème clair/sombre, logo Winzlee, paramètres et isolation vis-à-vis de Filee installé.

**Attention :** la compilation automatique ne prouve pas que toutes les nouvelles commandes et vues sont raccordées correctement ; valider les nouveaux outils d'image sur Windows avant fusion dans une branche stable.
