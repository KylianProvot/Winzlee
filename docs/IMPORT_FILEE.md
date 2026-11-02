# Import de Filee v2.1.0

## Source figée

- Projet : [KnifeLemon/Filee](https://github.com/KnifeLemon/Filee)
- Version : [v2.1.0](https://github.com/KnifeLemon/Filee/releases/tag/v2.1.0)
- Commit attendu : `e830916e7f02526bff6b7576edc22e51ecf9c126`
- Licence : MIT (à conserver avec l'avis d'auteur initial)
- Destination : `app/` (copie du code original avant renommage)

## Pourquoi `app/` ?

Le projet Winzlee possède déjà sa propre documentation et son README. En plaçant la copie de Filee dans `app/`, nous évitons d'écraser ces fichiers, et la solution C# reste entière dans `app/Filee.slnx` pendant la préparation du rebranding.

## Import reproductible

Le workflow GitHub Actions `.github/workflows/import-filee.yml` télécharge le tag v2.1.0, vérifie le commit et copie les fichiers dans `app/`. **Il ne compile ni n'exécute Filee.** Il écrit uniquement sur la branche `feature/import-filee-v2.1.0`.

Après import : inspecter les points d'entrée, les noms des binaires et les fichiers de configuration, puis préparer le renommage progressif sur une nouvelle branche.

> Ne pas supprimer `app/LICENSE` ou les notices tierces de Filee. Respecter également les licences des moteurs tiers avant distribution.
