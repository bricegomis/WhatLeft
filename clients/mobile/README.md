# WhatLeft — Mobile (React Native / Expo)

Application iOS & Android. TypeScript + Expo Router + Zustand. Même API que le client web.

## Structure

```
clients/mobile/
├── app/
│   ├── _layout.tsx          — Auth0 gate, navigation root
│   ├── (auth)/login.tsx     — Écran de connexion
│   └── (tabs)/
│       ├── _layout.tsx      — Tab bar (Tâches, Récurrences, Historique, Profil)
│       ├── index.tsx        — Liste des tâches + swipe actions + filtre tags
│       ├── recurring.tsx    — Récurrences (à compléter)
│       ├── history.tsx      — Historique + réactivation
│       └── settings.tsx     — Profil + déconnexion
├── src/
│   ├── types/               — task.ts, recurring.ts
│   ├── services/
│   │   ├── apiService.ts    — Toutes les routes REST (tasks + recurring)
│   │   └── tokenStorage.ts  — SecureStore (Keychain / Keystore)
│   ├── stores/
│   │   ├── tasksStore.ts    — Zustand
│   │   └── recurringStore.ts
│   └── components/
│       ├── TagChip.tsx
│       └── AddTaskModal.tsx
├── app.json                 — Config Expo (bundle ID, permissions, Auth0)
├── eas.json                 — EAS Build config
└── package.json
```

## Démarrage rapide

### Prérequis

```bash
node --version   # >= 18
npm install -g expo-cli eas-cli
```

### Installation

```bash
cd clients/mobile
npm install
```

### Lancer en développement

```bash
# Simulateur iOS (macOS requis) ou Android (Android Studio requis)
npm start         # Expo Go via QR code (pas d'accès natif complet)
npm run android   # Android emulator
npm run ios       # iOS simulator (macOS seulement)
```

> Pour tester sur un vrai device Android : activer le débogage USB, puis `npm run android`.

### URL de l'API

Dans `src/services/apiService.ts`, ligne `BASE_URL` :

- **Simulateur iOS** : `http://localhost:5000`
- **Émulateur Android** : `http://10.0.2.2:5000` (alias localhost Android)
- **Vrai device** : `http://192.168.x.x:5000` (IP locale de votre machine)

### Auth0

Créer une application **Native** dans Auth0 Dashboard, puis renseigner les variables dans `app.json` (plugin `react-native-auth0`) et `src/services/apiService.ts` :

| Variable | Où | Valeur |
|---|---|---|
| `domain` | `app.json` → plugin react-native-auth0 | `<VOTRE_DOMAIN>.auth0.com` |
| `clientId` | `app/_layout.tsx` → `Auth0Provider` | ID de votre application Native |
| `BASE_URL` | `src/services/apiService.ts` | URL de votre API |

Dans Auth0 Dashboard → votre app Native → onglet **Settings** :
- **Allowed Callback URLs** : `com.whatleft.app://<VOTRE_DOMAIN>/android/com.whatleft.app/callback, com.whatleft.app://<VOTRE_DOMAIN>/ios/com.whatleft.app/callback`
- **Allowed Logout URLs** : mêmes valeurs

## Build production (EAS)

```bash
eas login
eas build --platform android   # APK/AAB
eas build --platform ios       # IPA (nécessite compte Apple Developer)
```

## Widgets iOS (futur)

Les widgets home screen nécessitent Swift (WidgetKit). À créer séparément dans `clients/ios-widgets/` — partage de données avec l'app React Native via **App Group**. Nécessite un Mac + Xcode.
- Partage des types/contrats API via OpenAPI spec : `GET http://localhost:5000/openapi/v1.json`

## Ce qui s'y trouvera

- Application mobile iOS / Android
- Authentification via Auth0 (même tenant que le client web)
- Consommation de la même API REST que `clients/web`

## Démarrage

Ce dossier sera initialisé lors du sprint mobile. En attendant, référencez :
- L'API : [backend/](../backend/)
- Le client web comme référence d'implémentation Auth0 : [clients/web/](../web/)
