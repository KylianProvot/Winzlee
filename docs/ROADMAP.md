# Feuille de route Winzlee

Dernière mise à jour : 9 novembre 2026.

## État actuel

Winzlee est en préparation de la **v1.0.0 RC1** pour Windows x64. Les anciens documents `WINZLEE_V*`, les plans de test v0.x et le suivi d'import Filee restent conservés comme **archives de développement**, pas comme liste de tâches à réaliser.

| Chantier | État |
| --- | --- |
| Adaptation de Filee à Winzlee et conservation des crédits | Réalisé pour la RC1 |
| Interface Winzlee et intégration Ctrl + clic droit | Testées sur Windows |
| Conversion, renommage de masse et opérations sur archives | Testés pendant le développement |
| Installateur Windows et édition portable | Construits et testés manuellement |
| Tests automatisés Windows RC1 (dépôt de développement) | Réussis le 1er novembre 2026 |
| Compilation et tests Windows complets depuis **Winzlee-Public** | Réussis le 2 novembre 2026, exécution `41417869823` (job Windows) |
| Mise à jour d'une version existante avec conservation des réglages | À vérifier sur un vrai PC |
| Coexistence avec Filee et extensions Explorateur | À vérifier sur un vrai PC |
| Vérification finale du contenu publiable, licences, données privées | Audit de préparation effectué le 9 novembre ; validation humaine finale encore nécessaire |
| Release GitHub RC1 et téléchargements publics | Non publiée ; packages validés sauvegardés localement, artefacts Actions nettoyés |
| Version stable v1.0.0 | Après validation de la RC1 |

## Publication

Le dépôt de développement `KylianProvot/Winzlee` doit rester privé. `Winzlee-Public` contient un instantané du code source avec un historique réinitialisé et doit rester privé jusqu'à la fin de l'audit de publication.

Les étapes de diffusion et les noms d'artefacts se trouvent dans [RELEASE-CHECKLIST.md](../RELEASE-CHECKLIST.md). Le workflow manuel de validation peut préparer une **Release brouillon**, mais ne la rend jamais publique automatiquement.

## Suite après la première publication

- Retours utilisateurs et corrections de la RC1.
- Stabilisation du parcours de mise à jour.
- Documentation des formats pris en charge et des dépendances externes.
- Améliorations de l'ergonomie et des performances fondées sur les retours réels.
