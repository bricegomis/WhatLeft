# infra/

Dossier réservé à l'infrastructure et aux outils de déploiement de WhatLeft.

## Ce qui s'y trouvera

### Cloud & Base de données
- **Neon** (PostgreSQL serverless) — scripts de provisioning, variables d'environnement de référence
- Procédures de migration EF Core pour les environnements staging/production

### Conteneurs
- `Dockerfile` pour le backend ASP.NET Core
- `docker-compose.yml` pour le développement local (PostgreSQL local en alternative à Neon)

### CI/CD
- Pipelines GitHub Actions :
  - Build + test backend (`dotnet build`, `dotnet test`)
  - Build + lint frontend (`npm run build`)
  - Déploiement automatique (ex. Railway, Fly.io, Azure App Service)

### Auth0
- Scripts de configuration Auth0 (via Management API ou Terraform)
- Documentation des Allowed Callback URLs par environnement

## Conventions

- Les secrets ne doivent jamais être commités — utiliser les variables d'environnement du CI
- Les fichiers `appsettings.Development.json` et `.env` sont gitignorés
