# Guide de Test de l'Application Kubernetes (appscore) avec Helm

Ce document décrit les commandes et étapes nécessaires pour tester l'application Kubernetes contenue dans le dossier `appscore` en utilisant Helm.

---

## Prérequis

Avant de commencer, assurez-vous d'avoir installé et configuré les outils suivants :

- [kubectl](https://kubernetes.io/docs/tasks/tools/) : outil en ligne de commande pour interagir avec Kubernetes.
- [Helm](https://helm.sh/docs/intro/install/) : gestionnaire de packages Kubernetes.
- [Bash shell](https://git-scm.com/downloads) : pour exécuter les scripts shell.
- Un cluster Kubernetes fonctionnel (local comme Minikube, Kind, ou distant).
- (Optionnel) Un éditeur de texte pour modifier les fichiers de configuration si nécessaire.
- (Optionnel) Navigateur web pour accéder à l'application.
- (Optionnel) Accès administrateur pour modifier le fichier `hosts` de votre système afin d'ajouter une entrée pour `web.localhost`.

---

## Installation des outils nécessaires

### kubectl

- Suivez la documentation officielle pour installer kubectl :  
  https://kubernetes.io/docs/tasks/tools/

### Helm

- Suivez la documentation officielle pour installer Helm :  
  https://helm.sh/docs/intro/install/

### Bash shell (pour Windows)

- Installez Git Bash qui fournit un shell Bash sous Windows :  
  https://git-scm.com/downloads

---

## Étapes pour tester l'application avec Helm

### 1. Installer ou mettre à jour le chart Helm

Le projet contient un chart Helm dans le dossier `helm-charts/dotnetgigs`. Pour déployer l'application, utilisez la commande suivante :

```bash
helm upgrade --install dotnetgigs helm-charts/dotnetgigs
```

Cette commande installe ou met à jour la release Helm nommée `dotnetgigs` avec les manifests Kubernetes générés par le chart.

---

### 2. Régénérer le certificat TLS avec SAN

Pour éviter les erreurs de certificat dans le navigateur, il faut générer un certificat TLS auto-signé avec l'extension Subject Alternative Names (SAN) pour `web.localhost`.

Exécutez le script suivant :

```bash
bash scripts/regenerate-tls-and-restart-ingress.sh
```

Ce script :

- Génère une nouvelle clé privée et un certificat auto-signé avec SAN
- Supprime l'ancien secret TLS Kubernetes
- Crée un nouveau secret TLS avec le certificat généré
- Redémarre le déploiement de l'ingress controller pour appliquer le nouveau certificat

---

### 3. Vérifier l'état des pods et ingress

Assurez-vous que tous les pods sont en état `Running` et que l'ingress est configuré :

```bash
kubectl get pods
kubectl get ingress
```

---

### 4. Modifier le fichier hosts (si nécessaire)

Pour que `web.localhost` pointe vers votre cluster Kubernetes local ou distant, ajoutez une entrée dans le fichier `hosts` de votre système : dans ce dossiers C:\Windows\System32\drivers\etc

```
127.0.0.1 web.localhost
```

Par exemple, pour un cluster local Minikube, vous pouvez obtenir l'IP avec :

```bash
minikube ip
```

---

### 5. Accéder à l'application

Ouvrez votre navigateur et accédez à l'URL configurée, par exemple :

```
https://web.localhost
```

Si vous avez un message d'erreur lié au certificat, assurez-vous que le certificat a bien été régénéré et que le navigateur fait confiance au certificat auto-signé.

---

## Notes supplémentaires

- Si vous devez modifier les configurations, mettez à jour les valeurs Helm ou les templates, puis relancez la commande `helm upgrade`.
- Le script de régénération TLS doit être relancé chaque fois que vous souhaitez renouveler le certificat auto-signé.
- Assurez-vous que les outils `kubectl` et `helm` sont bien installés et configurés dans votre environnement.

---

Ce guide permet à toute personne recevant le dossier `appscore` de déployer et tester rapidement l'application dans un environnement Kubernetes avec Helm.
