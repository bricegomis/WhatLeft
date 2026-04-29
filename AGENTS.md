# WhatLeft — Agent Instructions

Task management app. Vue 3 frontend + ASP.NET Core 10 API (modular DDD) + PostgreSQL (Neon) + Auth0.

## Workspace layout

```
backend/      → ASP.NET Core 10 API (port 5000) ← active backend
clients/
  web/        → Vue 3 + Vite + Vuetify + Pinia frontend (port 5173)
  mobile/     → Futur client mobile (placeholder)
infra/        → Docker, CI/CD, scripts de déploiement (placeholder)
```

## Commands

### Frontend
```bash
cd clients/web
npm run dev       # http://localhost:5173
npm run build     # vue-tsc -b && vite build
```

### API
```bash
cd backend
dotnet run --project src/WhatLeft.Api    # http://localhost:5000
dotnet build
```

### EF Core migrations (from backend/)
```bash
# New migration after entity changes
dotnet ef migrations add <Name> --project src/Modules/Tasks/WhatLeft.Tasks.Infrastructure --startup-project src/WhatLeft.Api

# Apply to DB
dotnet ef database update --project src/Modules/Tasks/WhatLeft.Tasks.Infrastructure --startup-project src/WhatLeft.Api
```

Install tool once: `dotnet tool install --global dotnet-ef`

## Environment setup

**clients/web/.env** (copy from clients/web/.env.example if missing):
```
VITE_AUTH0_DOMAIN=dev-frvj0skig142mzhh.eu.auth0.com
VITE_AUTH0_CLIENT_ID=VudQOhonXNUNVMMEBwwVCOpTta8bZ2bC
VITE_AUTH0_AUDIENCE=https://whatleft-api
VITE_API_BASE_URL=http://localhost:5000
PORT=5173
```

**backend/src/WhatLeft.Api/appsettings.Development.json** (gitignored, create locally):
```json
{
  "Auth0": {
    "Domain": "dev-frvj0skig142mzhh.eu.auth0.com",
    "Audience": "https://whatleft-api",
    "ClientId": "VudQOhonXNUNVMMEBwwVCOpTta8bZ2bC"
  },
  "ConnectionStrings": { "Tasks": "<NEON_CONNECTION_STRING>" },
  "Cors": { "AllowedOrigin": "http://localhost:5173" }
}
```

## ASP.NET Core architecture (DDD modular monolith)

Each module has three projects + endpoints registration:

```
src/Modules/<Module>/
  <Module>.Domain/          → Aggregate roots, domain events, IRepository interfaces
  <Module>.Application/     → DTOs, TaskService (use cases), event handlers, Extensions.cs
  <Module>.Infrastructure/  → DbContext, Repository impl, Extensions.cs
src/WhatLeft.Api/
  Modules/<Module>/         → Minimal API endpoint groups
  OpenApi/                  → BearerSecuritySchemeTransformer
  Program.cs                → DI wiring: AddXxxApplication() + AddXxxInfrastructure() + MapXxxEndpoints()
```

**Adding a new module**: replicate the Tasks module pattern, add 3 lines in `Program.cs`.

### Key files
- [TaskItem.cs](backend/src/Modules/Tasks/WhatLeft.Tasks.Domain/Entities/TaskItem.cs) — aggregate root, all business rules here
- [TaskService.cs](backend/src/Modules/Tasks/WhatLeft.Tasks.Application/UseCases/TaskService.cs) — saves then dispatches domain events via MediatR
- [TasksDbContext.cs](backend/src/Modules/Tasks/WhatLeft.Tasks.Infrastructure/Persistence/TasksDbContext.cs) — schema `"tasks"`, Tags stored as comma-separated string
- [Program.cs](backend/src/WhatLeft.Api/Program.cs) — JWT Bearer (Auth0) + CORS + OpenAPI (Scalar) + `db.Database.Migrate()` on startup
- [TasksEndpoints.cs](backend/src/WhatLeft.Api/Modules/Tasks/TasksEndpoints.cs) — Minimal API, all routes require `.RequireAuthorization()`

## Frontend conventions

- State: Pinia store at [stores/tasks.ts](clients/web/src/stores/tasks.ts) — Task interface has `tags: string[]`
- Auth: [App.vue](clients/web/src/App.vue) fetches token silently → [tasksApi.ts](clients/web/src/services/tasksApi.ts) `setAuthToken()`
- Auth guard: all routes except `/login` use `authGuard` from `@auth0/auth0-vue`

### FullCalendar (critical)
Events **must** be inside the `calendarOptions` computed property — never as a separate `:events` prop. See [CalendarView.vue](clients/web/src/views/CalendarView.vue).

## Data conventions

- **Dates**: always `DateTimeOffset` UTC / ISO 8601 (`2026-04-29T14:00:00Z`). Never date-only strings.
- **Drag-drop**: use `.toISOString()`, never `.split('T')[0]`
- **Tags**: `string[]` in API/frontend → comma-separated `text` column in PostgreSQL
- **Domain events**: raised inside aggregates, dispatched by `TaskService` **after** `SaveChangesAsync()`, then cleared

## OpenAPI / Scalar

- Spec: `GET http://localhost:5000/openapi/v1.json`
- UI: `http://localhost:5000/scalar/v1` (Dev only)
- Auth in Scalar: OAuth2 Authorization Code + PKCE, ClientId pre-filled, scheme name `"Auth0"`
- Auth0 callback for Scalar: add `http://localhost:5000/scalar/v1` to Allowed Callback URLs in Auth0 Dashboard

## Known pitfalls

| Issue | Fix |
|---|---|
| `SQLite DateTimeOffset ORDER BY` error | Migrated to PostgreSQL — do not reintroduce SQLite |
| FullCalendar shows no events | Events must be in `calendarOptions` computed, not separate prop |
| `dotnet run` uses Production env | `launchSettings.json` sets `ASPNETCORE_ENVIRONMENT=Development` |
| Vite proxy `/api` → `:3000` | Use `VITE_API_BASE_URL=http://localhost:5000` for the dotnet API |
| Auth0 "not authorized to access resource server" | Auth0 Dashboard → APIs → WhatLeft API → Machine to Machine → authorize the SPA |
