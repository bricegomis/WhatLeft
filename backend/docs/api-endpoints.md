# Référence des endpoints REST

## Tâches — `/tasks`

| Méthode | Route | Description |
|---|---|---|
| `GET` | `/tasks` | Tâches en cours (status `Pending`) |
| `GET` | `/tasks/history` | Tâches terminées ou annulées |
| `POST` | `/tasks` | Créer une tâche `{title, duration, tags}` |
| `PUT` | `/tasks/:id` | Modifier une tâche |
| `DELETE` | `/tasks/:id` | Supprimer définitivement |
| `POST` | `/tasks/:id/reactivate` | Remettre en `Pending` depuis l'historique |

## Récurrences — `/recurring-templates`

| Méthode | Route | Description |
|---|---|---|
| `GET` | `/recurring-templates` | Tous les templates de l'utilisateur |
| `POST` | `/recurring-templates` | Créer un template `{title, recurrenceType, …}` |
| `PUT` | `/recurring-templates/:id` | Modifier un template |
| `DELETE` | `/recurring-templates/:id` | Désactivation douce (`IsActive = false`) |
| `POST` | `/recurring-templates/:id/activate` | Réactiver un template |
| `POST` | `/recurring-templates/:id/advance` | Passer manuellement au cycle suivant |
| `POST` | `/recurring-templates/advance-all?type=` | Avancer tous les templates d'un type (`Daily` / `Weekly` / `Monthly`) |
| `POST` | `/recurring-templates/:id/process` | Forcer la création des tâches pour la période courante |

Tous les endpoints requièrent `Authorization: Bearer <access_token>`.

```mermaid
graph LR
    subgraph "Tasks /tasks"
        G1["GET /\ntâches en cours"]
        G2["GET /history\ntâches terminées"]
        P1["POST /\ncréer"]
        P2["POST /:id/reactivate\nréactiver"]
        U1["PUT /:id\nmodifier"]
        D1["DELETE /:id\nsupprimer"]
    end

    subgraph "Recurring /recurring-templates"
        R1["GET /"]
        R2["POST /\ncréer"]
        R3["PUT /:id"]
        R4["DELETE /:id\nsoft deactivate"]
        R5["POST /:id/activate"]
        R6["POST /:id/advance\nun template"]
        R7["POST /advance-all?type=\ntous d'un type"]
        R8["POST /:id/process\nforce création"]
    end
```
