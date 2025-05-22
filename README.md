# Git Tutorial

Ce tutoriel explique comment configurer et utiliser Git pour gérer un projet Unity avec Visual Studio Code.


## Préparation (à realiser une seule fois)

Vous devez d'abord configurer votre environnement de développement avant de commencer à utiliser Git. Voici les étapes à suivre :
 - Installez Unity 6000.0.42
 - Installez VS Code avec l’extension Git intégrée.
 - Installez Git sur votre machine.
 - Créer un compte GitHub avec votre adresse ESIEE.
 - Configurer votre compte GitHub dans VS Code.

Le projet est hébergé sur GitHub. Vous devez le cloner sur votre machine locale pour y travailler. Voici comment procéder :
 - Ouvrez VS Code et accédez à l’onglet *Source Control*.
 - Cliquez sur "Clone Repository" et selectionnez le dépôt GitHub que vous souhaitez cloner.
 - Choisissez un emplacement sur votre disque dur pour cloner le projet.
 - Ouvrez le projet cloné dans VS Code.
 - Ouvrez le projet dans Unity en sélectionnant le dossier du projet cloné.


## Utilisation de Git

La branche `origin/main` est toujours à jour avec les dernières fonctionnalités. Avant toute modification au projet, vous devez créer une nouvelle branche à partir de `origin/main`. Vous effectuez vos modifications sur cette nouvelle branche. Une fois que vous avez terminé, vous devez fusionner votre branche avec `origin/main`. La façon de fusionner une branche est de créer une PR (pull request) sur GitHub.

La durée de vie d'une branche est limitée à la durée de votre tâche. Il est important de faire des itérations courtes et de fusionner votre branche avec `origin/main` régulièrement. Une fois que votre branche est fusionnée, vous devez la supprimer. Cela permet de garder le dépôt propre et d'éviter l'accumulation de branches inutiles.


### Etape 1 : Créer une nouvelle branche

 - Dans VS Code, ouvrez l’onglet *Source Control*.
 - Assurez-vous que vous êtes sur la branche `origin/main` et que celle-ci est à jour.
 - Ouvrir le menu contextuel en cliquant sur les trois points `...` à droite du sous-onglet *CHANGES*.
 - Sélectionnez "Create New Branch" dans le menu contextuel.
 - Nommez la branche selon le format `dev/votre-nom/nom-de-la-fonctionnalité`.


### Etape 2 : Effectuer des modifications

 - Effectuez les modifications nécessaires dans le projet Unity.
 - Enregistrez vos modifications dans Unity et VS Code.
 - Pour certaines modifications, vous devrez cliquer sur *Save Project* dans le menu *File* de Unity ou peut-être même redémarrer Unity pour qu'elles prennent effet. 


### Etape 3 : Valider vos modifications (commit)

 - Dans l’onglet *Source Control*, les fichiers modifiés et les nouveaux fichiers apparaîtront sous la section *Changes*.
 - Cliquez sur le bouton "+" à côté de chaque fichier ou dossier que vous souhaitez valider. Pour ajouter tous les fichiers, cliquez sur le + de la section *Changes*.
 - Une fois que vous avez ajouté tous les fichiers, entrez un message de validation dans le champ de texte en haut du bouton *Commit*.
 - Cliquez sur le bouton *Commit* pour valider vos modifications. Cela enregistre vos modifications dans votre branche locale.

#### Modifier un commit existant

En cliquant sur la flèche à droite du bouton *Commit*, vous pouvez sélectionner **Commit (Amend)**. Cela vous permettra de modifier le dernier commit sans en créer un nouveau, afin de corriger un message ou d'ajouter des fichiers oubliés. Notez que cette action ouvre un fichier de commit dans l'éditeur de texte. Vous devez cliquer sur la coche en haut à droite pour valider les modifications. Vous pouvez également annuler cette action en cliquant sur la croix.


### Etape 4 : Mettre à jour votre branche (rebase)

Arrivé à ce stade, vous avez effectué des modifications sur votre branche locale et vous souhaitez les pousser vers le dépôt distant. Cependant, avant de le faire, il est important de s'assurer que votre branche est à jour avec `origin/main`. Cela permet de garantir que vous travaillez sur la version la plus récente du code.

#### 4.1 Récupérer les dernières modifications

Tout d'abord, vous devez récupérer les dernières modifications de `origin/main`. Cela vous permet de voir si d'autres développeurs ont apporté des modifications au code depuis votre dernière mise à jour.

- Dans l’onglet *Source Control*, cliquez sur le menu contextuel `...` à droite du sous-onglet *CHANGES*.
- Sélectionnez "Pull, Push > Pull from..." et choisissez `origin/main`. Cela mettra à jour votre copie locale de la branche `origin/main` avec les dernières modifications.

#### 4.2 Effectuer un rebase

Si votre branche de developpement est devenue obsolète par rapport à `origin/main`, vous devez effectuer un rebase. Cela permet de réappliquer vos modifications sur la dernière version de `origin/main`.

- Dans l’onglet *Source Control*, cliquez sur le menu contextuel `...` à droite du sous-onglet *CHANGES*.
- Sélectionnez "Branch > Rebase Branch..."
- Choisissez `origin/main` comme branche cible.

#### 4.3 Résoudre les conflits (si nécessaire)

Un rebase peut entraîner des conflits si d'autres développeurs ont apporté des modifications aux mêmes fichiers que vous. Si cela se produit, VS Code mettra en évidence les fichiers en conflit dans l'onglet *Source Control*. Vous devrez résoudre ces conflits manuellement avant de pouvoit pousser vos modifications.

- Si un conflit survient, ouvrez les fichiers dans VS Code. Les sections en conflit seront marquées (par exemple, `<<<<<<< HEAD`).
- Modifiez le fichier pour conserver les bonnes modifications, puis ajoutez-le à l’index (+) et continuez le rebase.


### Etape 5 : Pousser vos modifications (push)

Une fois que vous avez validé vos modifications et mis à jour votre branche, vous pouvez les pousser vers le dépôt distant.
- Dans l’onglet *Source Control*, si vous n'avez pas encore publié votre branche, le bouton **Publish Branch** sera visible. Cliquez dessus pour publier votre branche sur le dépôt distant.
- Si vous avez déjà publié votre branche, vous verrez le bouton **Push** après avoir créé un commit. Cliquez dessus pour pousser vos modifications vers le dépôt distant.

#### Forcer le push si nécessaire
Si vous effectuez un amend ou un rebase sur une branche déjà publiée, le push peut être rejeté. Dans ce cas, vous devez forcer le push pour mettre à jour la branche distante avec vos modifications locales. Pour ce faire, ouvrez le terminal intégré de VS Code et exécutez la commande suivante.

```bash
git push --force-with-lease
```

Soyez prudent, car cela remplace l’historique distant. Assurez-vous que vous êtes le seul à travailler sur cette branche avant de forcer le push.


### Etape 6 : Créer une Pull Request (PR)

Une fois que vous avez poussé vos modifications vers le dépôt distant, vous pouvez créer une Pull Request (PR) pour demander la fusion de votre branche avec `origin/main`. Voici comment procéder. 

- Accédez à votre dépôt sur GitHub.
- Cliquez sur l'onglet "Pull Requests" puis sur le bouton "New Pull Request".
- Sélectionnez votre branche comme source et `origin/main` comme destination.
- Ajoutez un titre et une description pour expliquer les modifications apportées. Puis, cliquez sur le bouton "Create Pull Request".
- Une fois la PR créée, vous pouvez demander à un autre développeur de la réviser. Il peut laisser des commentaires ou approuver la PR.
- Une fois que la PR est approuvée, vous pouvez la fusionner avec `origin/main` en cliquant sur le bouton **Rebase and Merge**.
- Supprimer votre branche après la fusion.