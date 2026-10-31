# Local Docker infrastructure

`docker-compose.local.yml` runs the local dependencies currently selected for WAREFLOW:

- PostgreSQL 18 with the named `wareflow-postgres-data` volume.
- Redis as disposable cache/coordination storage without a persistent volume.
- Mailpit for local SMTP capture and browser-based email inspection.

The Compose file is development-only. It must not be used as a production deployment configuration.

From the repository root:

```powershell
Copy-Item .env.example .env
docker compose -f infrastructure\docker\docker-compose.local.yml up -d
docker compose -f infrastructure\docker\docker-compose.local.yml ps
```

The committed `.env.example` contains placeholders only. Replace local values in `.env`; `.env` is ignored by Git.

## Local endpoints

| Service | Host endpoint |
|---|---|
| PostgreSQL | `localhost:5432` |
| Redis | `localhost:6379` |
| Mailpit SMTP | `localhost:1025` |
| Mailpit web UI | <http://localhost:8025> |

Reset only the PostgreSQL data when a clean local database is required:

```powershell
docker compose -f infrastructure\docker\docker-compose.local.yml down
docker volume rm wareflow-postgres-data
docker compose -f infrastructure\docker\docker-compose.local.yml up -d
```

The Redis container has no persistent volume by design. Removing and recreating it clears local cache state.
