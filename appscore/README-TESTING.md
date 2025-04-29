# Guide de Test de l'Application Kubernetes (appscore)

Ce document décrit les commandes et étapes nécessaires pour tester l'application Kubernetes contenue dans le dossier `appscore`.

---

## Prérequis

- Kubernetes configuré et accessible (kubectl configuré)
- Accès au cluster Kubernetes
- Bash shell disponible pour exécuter les scripts

---

## Étapes pour tester l'application

### 1. Appliquer les manifests Kubernetes

Déployer les ressources Kubernetes (pods, services, ingress, secrets, etc.) :

```bash
kubectl apply -f k8s-config/deployments/
```

Cette commande crée ou met à jour les déploiements, services et autres ressources nécessaires.

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

### 4. Accéder à l'application

Ouvrez votre navigateur et accédez à l'URL configurée, par exemple :

```
https://web.localhost
```

Si vous avez un message d'erreur lié au certificat, assurez-vous que le certificat a bien été régénéré et que le navigateur fait confiance au certificat auto-signé.

---

## Notes supplémentaires

- Si vous devez modifier les configurations, appliquez à nouveau les manifests avec la commande `kubectl apply`.
- Le script de régénération TLS doit être relancé chaque fois que vous souhaitez renouveler le certificat auto-signé.
- Assurez-vous que le fichier `hosts` de votre système contient une entrée pour `web.localhost` pointant vers l'adresse IP du cluster ou de la machine locale.

---

Ce guide permet à toute personne recevant le dossier `appscore` de déployer et tester rapidement l'application dans un environnement Kubernetes.
