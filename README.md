🧭 WhatLeft
Decide what to do now, and what can wait.
A personal planner to understand what’s left to do.

🎯 Vision
WhatLeft est une application personnelle de planification et de gestion du quotidien.
Le nom exprime l’idée de :

« qu’est‑ce qu’il me reste à faire maintenant ? »
« qu’est‑ce qui peut attendre ? »

L’objectif est de mieux organiser son temps, son énergie et ses priorités, sans tomber dans une productivité rigide ou artificielle.

🧘 Philosophie
WhatLeft est volontairement développé dans une logique de vibe coding :

itérations rapides
décisions pragmatiques
amélioration continue
priorité au flow et à la clarté
équilibre entre rigueur technique et liberté créative

Le projet est à la fois :

un outil personnel
un laboratoire d’architecture
un support de montée en compétences long terme


🧠 Ce que permet WhatLeft

Gérer des tâches et activités avec des tags
Planifier des journées et des semaines via une vue calendrier
Définir des tâches récurrentes (sport, routines, habitudes)
Gérer des quotas hebdomadaires (loisirs, jeux, séries, sorties…)
Prendre en compte des contraintes réelles (ex : semaines A/B, télétravail)
Préparer l’analyse et le suivi dans le temps (dashboards à venir)

WhatLeft ne se limite pas à faire plus,
mais aide à mieux choisir ce qui mérite d’être fait maintenant.

🏗️ Principes d’architecture

Backend‑first
Un backend, plusieurs clients
Modular Monolith
Event‑driven (interne)
Stateless API
Technologies standards et transférables
Évolutivité sans sur‑ingénierie

Le backend est conçu comme un produit autonome, indépendant des interfaces clientes.

🗂️ Structure du repository
Plain Text.├── backend/            # API et logique métier│   ├── src/│   │   ├── Api/│   │   ├── Modules/│   │   │   ├── Tasks│   │   │   ├── Planning│   │   │   ├── Habits        (à venir)│   │   │   └── Points        (à venir)│   │   └── BuildingBlocks/│   └── backend.sln│├── clients/│   ├── web/             # Application web (Vue.js)│   └── mobile/          # Client mobile (prévu)│├── infra/               # Infrastructure, scripts, observabilité│└── README.mdShow more lines

🧱 Backend
Stack technique

.NET 10
ASP.NET Core
PostgreSQL 17
Hébergement PostgreSQL : Neon
EF Core
Authentification : JWT (Auth0)

Responsabilités

Gestion des tâches et activités
Règles de planification :

récurrence
quotas
semaine A / B


Génération et validation du planning
Exposition d’une API REST stateless
Émission d’événements métier internes

La base de données stocke l’état ;
le métier vit dans le code.

🌐 Clients
Client Web (actuel)

Application Vue.js
Vue calendrier hebdomadaire
Organisation visuelle des tâches
Drag & drop pour planifier les journées

Client Mobile (futur)

Usage rapide et contextuel :

widgets
consultation du planning
check‑in d’habitudes


Consomme exactement la même API


🗄️ Base de données

PostgreSQL
Une base pour le MVP
Séparation logique par domaines métier
Pas de logique métier dans la base


📊 Observabilité & dashboards (à venir)
WhatLeft est conçu pour intégrer plus tard :

une stack analytique (OpenSearch / Elasticsearch)
des dashboards type Kibana pour :

temps passé par activité
respect des quotas
équilibre semaine A / B
évolution dans le temps



Les dashboards seront alimentés par des événements métier, pas directement par la base transactionnelle.

🚀 Démarrage (dev)
Shell# Backendcd backenddotnet restoredotnet run# Client webcd clients/webnpm installnpm run devShow more lines
Configurer la connection string PostgreSQL via variables d’environnement ou appsettings.*.json.

🛣️ Roadmap (indicative)

 CRUD des tâches avec tags
 Base de la planification hebdomadaire
 Règles de récurrence avancées
 Quotas hebdomadaires
 Vue calendrier enrichie
 Client mobile
 Dashboards analytiques
 Automatisations & notifications


❤️ En résumé

WhatLeft is not about doing more.
It’s about understanding what’s left, and choosing wisely.