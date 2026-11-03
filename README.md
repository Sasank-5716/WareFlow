# WAREFLOW

WAREFLOW is a multi-tenant Inventory and Warehouse Management SaaS platform.

This repository currently contains the approved monorepo foundation and foundational tenant persistence. It does not yet include sign-in, inventory, or business feature pages.

## Repository layout

- `backend/` — ASP.NET Core 10 Clean Architecture solution.
- `frontend/wareflow-web/` — Angular 22 standalone application.
- `infrastructure/` — Docker, Nginx, and local infrastructure scripts.
- `docs/` — architecture and database decisions.

## Prerequisites

- .NET SDK 10.0 or later
- Node.js compatible with Angular 22
- npm
- Angular CLI 22

## Local development commands

### Backend

```powershell
dotnet restore backend\Wareflow.sln
dotnet build backend\Wareflow.sln
dotnet run --project backend\src\Wareflow.Api
```

### Frontend

```powershell
Set-Location frontend\wareflow-web
npm install
npm start
```

Start local PostgreSQL, Redis, and Mailpit with the commands in [docs/LOCAL-DEVELOPMENT.md](docs/LOCAL-DEVELOPMENT.md). Apply the initial tenant migration explicitly using the documented EF command.

### Tests

```powershell
dotnet test backend\Wareflow.sln
```

Frontend test commands are provided by the generated Angular workspace and will be extended with feature tests as features are implemented.

## Security

Copy `.env.example` to a local environment file and replace placeholders locally. Never commit secrets.

Authorization roles, permissions, tenant isolation, delegation rules, and feature registration are documented in [docs/AUTHORIZATION.md](docs/AUTHORIZATION.md).
