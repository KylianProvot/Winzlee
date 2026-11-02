# Winzlee v0.6 — recette visuelle

Base : `feature/powertoys-navigation-v0.5` (commit `99b88d8`).
Branche v0.6 : `feature/fluent-visual-v0.6`.

## Périmètre strict
- Reprendre **police (Segoe UI Variable / Segoe UI)**, **couleurs des surfaces** et **rayon des rectangles** inspirés de PowerToys, en thème clair et sombre.
- Garder toutes les pages, l'ordre, la navigation, les contenus et les commandes de Winzlee v0.5.
- Préserver la roue de conversion personnalisable hors des modifications de thème.
- Mettre la fleur Winzlee fournie (SVG de référence, PNG et ICO) à la place de l'icône Filee aux emplacements visibles.
- En-têtes des menus déroulants : éviter la surbrillance violette persistante ; les sélections réelles restent visibles et fonctionnelles.

## Recette Windows
1. Quitter entièrement Winzlee v0.5 depuis la zone de notification avant de lancer la v0.6 portable dans un dossier neuf.
2. Vérifier la police, les arrière-plans, les cartes et les rectangles sur toutes les pages, en clair et en sombre.
3. Ouvrir/fermer les sections déroulantes Général, Roue et Préréglages : l'en-tête doit rester neutre.
4. Essayer les vraies listes `ComboBox`, notamment le dossier de sortie et les déclencheurs : le choix doit changer et être lisible.
5. Vérifier le nouveau logo dans la fenêtre, la page À propos, l'icône de tâche, la zone de notification et l'installateur.
6. Refaire les tests Ctrl + clic droit Explorer, roue, conversions, préréglages, historique, thèmes et persistance, sans données importantes.
7. Contrôler la séparation des données Winzlee / Filee.

## Contraintes
Aucune modification du moteur de conversion, du code des raccourcis ou du traitement des archives. La fleur n'est pas redessinée ; les icônes Windows placent le dessin original sur un fond foncé discret afin de rester lisibles dans les deux thèmes. L'intégration HatchZip demeure hors périmètre.
