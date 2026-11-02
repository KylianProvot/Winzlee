# Interface Winzlee — orientation UX/UI

## Direction artistique
- Inspiration **PowerToys Windows** : application utilitaire discrète, panneaux nets, navigation latérale, espacements constants, typographie lisible.
- Respect du thème clair/sombre et des conventions Windows ; éviter l'aspect « dashboard marketing », les grosses animations et les effets décoratifs omniprésents.
- Ne pas reproduire à l'identique le logo ou les assets Microsoft.
- Priorité à la densité informative maîtrisée, aux états explicites et aux raccourcis clavier.

## Navigation proposée
- **Accueil** : glisser-déposer, actions rapides, récents.
- **Convertir** : source, sélection d'un format, options, résumé d'action.
- **Archives** : Extraire / Compresser, mode classique ou progressif, différence expliquée.
- **Historique** : opérations, succès, échecs, accès aux résultats.
- **Préréglages** : profils de conversion existants, avec libellés clairs.
- **Paramètres** : Général, Conversion, Archives, Intégration Windows.
- **À propos** : versions, licences et crédits de Filee/HatchZip.

## Composant « information format »
**PNG** — Image sans perte. Recommandé pour captures d'écran, logos et éléments transparents. Pour les photographies, JPG ou WEBP produisent souvent des fichiers plus légers. Un bouton « i » ouvre un panneau d'aide sans faire quitter le flux de conversion.

Chaque fiche de format : « À quoi ça sert ? », « Quand l'utiliser ? », « Avantages », « Limites », « Compatibilité ».

## Roue contextuelle
Ne pas refaire sa personnalisation. Conserver les profils et l'ordre des formats ; améliorer exclusivement le moyen d'activation et ajouter des actions archives pertinentes selon les types de fichiers.

## Microcopie du mode progressif
**Extraire sans conserver l'archive** — « L'archive est réduite au fur et à mesure de l'extraction. Cette option économise l'espace disque. Une fois terminée, l'archive d'origine n'est plus disponible. Ce n'est pas un effacement sécurisé. »

Ne pas présenter « sans trace » comme une garantie de confidentialité.

## Prochain livrable
Maquette écran principal, puis adaptation aux composants Avalonia réellement présents dans Filee v2.1.0.
