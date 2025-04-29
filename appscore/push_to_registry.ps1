#!/usr/bin/env pwsh
# Script pour construire les images Docker et les pousser vers le registre privé rioall77

# Configuration
$REGISTRY = "rioall77"
$SERVICES = @("webmvc", "applicants.api", "jobs.api", "identity.api")
$TAG = "latest"

# Informations pour l'utilisateur
Write-Host "Préparation des images pour le registre Docker privé $REGISTRY" -ForegroundColor Green
Write-Host "Avant de continuer, assurez-vous d'être connecté au registre avec:" -ForegroundColor Yellow
Write-Host "  docker login" -ForegroundColor Cyan

# Attendre confirmation
$confirmation = Read-Host "Êtes-vous connecté au registre Docker? (O/N)"
if ($confirmation -ne "O" -and $confirmation -ne "o") {
    Write-Host "Veuillez vous connecter au registre avant de continuer." -ForegroundColor Red
    exit 1
}

# Construction des images
Write-Host "Construction des images Docker..." -ForegroundColor Green
docker-compose build

# Vérification du résultat de la construction
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors de la construction des images. Veuillez vérifier les erreurs ci-dessus." -ForegroundColor Red
    exit 1
}

# Push des images vers le registre
Write-Host "Push des images vers le registre $REGISTRY..." -ForegroundColor Green

foreach ($service in $SERVICES) {
    $imageName = "$REGISTRY/$service`:$TAG"
    Write-Host "  Push de $imageName..." -ForegroundColor Cyan
    docker push $imageName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Erreur lors du push de $imageName. Veuillez vérifier votre connexion au registre." -ForegroundColor Red
    } else {
        Write-Host "  L'image $imageName a été poussée avec succès!" -ForegroundColor Green
    }
}

Write-Host "`nToutes les images ont été construites et poussées vers $REGISTRY." -ForegroundColor Green
Write-Host "Vous pouvez maintenant les utiliser dans vos déploiements Kubernetes avec les références:"

foreach ($service in $SERVICES) {
    Write-Host "  - $REGISTRY/$service`:$TAG" -ForegroundColor Cyan
}

# Notes sur la structure k8s-config
Write-Host "`nVotre secret d'authentification au registre a été configuré dans:" -ForegroundColor Yellow
Write-Host "  .\k8s-config\secrets\registry-secret.yaml" -ForegroundColor Cyan

Write-Host "`nPour utiliser ce secret dans vos déploiements Kubernetes, assurez-vous que vos fichiers de déploiement" -ForegroundColor Yellow
Write-Host "contiennent la référence suivante dans la section 'spec':" -ForegroundColor Yellow
Write-Host "  imagePullSecrets:" -ForegroundColor Cyan
Write-Host "  - name: docker-registry-secret" -ForegroundColor Cyan