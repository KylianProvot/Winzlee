# Cahier des charges Winzlee — v0.1

## Positionnement
Utilitaire Windows discret, rapide et accessible instantanément, avec une interface semblable à PowerToys en termes de clarté et d'organisation. Le public visé est mixte : débutants informés et utilisateurs avancés. Le style doit être équilibré et ne pas surcharger les écrans.

## Portée fonctionnelle
1. **Identité** : renommer Filee en Winzlee (titre, métadonnées, icônes, commandes, noms de fichiers lorsque raisonnable). Conserver la provenance et la licence.
2. **Explications des formats** : bouton « i » sur chaque format proposant usage, exemples, atouts, limites et compatibilité. Exemple PNG : image sans perte, transparence, idéal captures/logos, taille parfois supérieure au JPG pour les photos.
3. **Roue** : préserver la personnalisation existante. Ajouter si possible un déclenchement dans l'Explorateur par sélection puis Ctrl + clic droit. Vérifier compatibilité Windows 10/11 et conflits avec le menu natif ; prévoir un raccourci alternatif si nécessaire.
4. **Archives** : pour ZIP/RAR/7z/TAR et autres formats validés par les tests, extraction normale ou extraction progressive HatchZip. Afficher clairement que le mode progressif consomme l'archive et **n'est pas un effacement sécurisé des traces**. Informer avant action destructive.
5. **Compression** : format et niveau au choix. Mode « supprimer les originaux » seulement **après** création, test d'intégrité et validation du résultat. Ne pas prétendre que HatchZip fournit ce mode.
6. **Explorateur Windows** : options contextuelles pour extraire, extraire progressivement, compresser et ouvrir Winzlee ; aucune association invasive par défaut.

## Sécurité et robustesse
- Avant toute extraction destructive : contrôle du disque, chemins, collisions, autorisations et état de l'archive ; confirmation compréhensible.
- Préserver les mécanismes de reprise de HatchZip ; ne jamais supprimer ses fichiers de progression avant opération terminée.
- Journal des erreurs, indicateur de progression, actions de reprise et d'annulation si prises en charge.
- Valider le fonctionnement et les contraintes de licence des moteurs inclus avant publication.
- Ne jamais écraser sans avertissement une installation Filee existante ; données et réglages isolés.

## Contraintes
- Base cible : tag immuable Filee v2.1.0 ; langage C#/.NET, UI Avalonia.
- Priorité Windows, build portable au départ.
- Intégration HatchZip par exécutable auxiliaire dans un premier temps, à vérifier selon son interface CLI.
- Dépôt privé de travail ; aucune licence propre ni copie de code source amont n'est encore importée.

## Critères d'acceptation initiaux
- La future application s'affiche sous « Winzlee », sans perturber Filee portable.
- Le panneau des formats explique PNG, JPG, WEBP et PDF en français.
- Le raccourci et les menus ne déclenchent d'action que sur des sélections éligibles.
- En mode « sans trace », une interruption permet une reprise sûre conformément aux garanties de HatchZip, vérifiées par tests.
