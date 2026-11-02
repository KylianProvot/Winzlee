# Plan de tests Windows — Winzlee v0.1

> Document de recette pour les premières archives portables. Les tests de l'interface restent **à réaliser sur un PC Windows** : une compilation CI ne prouve pas la bonne exécution graphique.

## Avant de commencer

1. Conserver la version portable de Filee dans son dossier existant.
2. Télécharger le paquet Winzlee généré par GitHub Actions et l'extraire dans un **nouveau** dossier, par exemple `C:\Outils\Winzlee-Test\`.
3. Faire les essais sur des copies de fichiers sans valeur. Éviter les dossiers sensibles et les fichiers de travail.
4. À ce stade, ne pas activer une installation système ni l'intégration Explorateur ; elles restent désactivées dans notre build de développement.

## Tests prioritaires

| Test | Résultat attendu |
| --- | --- |
| Démarrage | `Winzlee.exe` démarre sans crash et ouvre son interface depuis la zone de notification. |
| Identité | La fenêtre, son titre et la page À propos affichent « Winzlee ». Certains éléments de traduction/icônes sont encore à refaire. |
| Instances distinctes | Filee portable et Winzlee peuvent fonctionner simultanément ; aucun ne ferme ni ne réutilise la fenêtre de l'autre. |
| Dossiers distincts | Winzlee crée ses réglages dans `%APPDATA%\Winzlee`, Filee conserve `%APPDATA%\Filee`. |
| Paramètres | Un changement de paramètre Winzlee n'est **pas** répercuté dans Filee. |
| Historique | Les historiques Winzlee et Filee ne se mélangent pas. |
| Configuration de la roue | La roue Filee reste intacte après réglage de celle de Winzlee. |
| Explorateur Windows | Aucune ancienne entrée Filee n'est remplacée par Winzlee. |
| Conversions simples | Convertir une image PNG de test en JPG dans un dossier de test ; contrôler la sortie. |
| Arrêt / redémarrage | Quitter via la zone de notification puis relancer Winzlee. |

## Non disponible dans v0.1

- Le déclencheur Ctrl + clic droit de la roue.
- L'extraction progressive HatchZip.
- Le mode compression « supprimer les originaux après vérification ».
- La nouvelle interface PowerToys complète.
- L'installation intégrée aux menus contextuels Windows.
- Une icône Winzlee définitive ou un paquet signé numériquement.

## Diagnostic en cas d'erreur

Relever la version de Windows, le message affiché, et les dernières lignes du journal dans `%APPDATA%\Winzlee\logs`. Éviter de transmettre des données personnelles contenues dans les logs, comme des chemins de fichiers privés.

## Considérations de sécurité

Le paquet de prévisualisation est un **build de développement non signé**. Ne pas ignorer automatiquement les avertissements de sécurité Windows. Un téléchargement d'archives n'est pas une preuve d'authenticité : utiliser uniquement les artefacts du workflow du dépôt privé Winzlee. Ne pas activer les fonctionnalités de suppression ou d'extraction destructives avant leurs tests spécifiques.
