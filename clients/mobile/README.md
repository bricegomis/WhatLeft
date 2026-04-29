# clients/mobile

Dossier réservé au futur client mobile WhatLeft.

## Technologie envisagée

- **React Native** (Expo) ou **Flutter** selon les contraintes d'équipe
- Partage des types/contrats API via OpenAPI spec : `GET http://localhost:5000/openapi/v1.json`

## Ce qui s'y trouvera

- Application mobile iOS / Android
- Authentification via Auth0 (même tenant que le client web)
- Consommation de la même API REST que `clients/web`

## Démarrage

Ce dossier sera initialisé lors du sprint mobile. En attendant, référencez :
- L'API : [backend/](../backend/)
- Le client web comme référence d'implémentation Auth0 : [clients/web/](../web/)
