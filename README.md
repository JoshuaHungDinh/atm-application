# ATM Application

A web-based ATM for a single user managing two bank accounts: deposit, withdraw, and
transfer between accounts, with full transaction history.

Backend: ASP.NET Core (.NET 10). Frontend: React + TypeScript.

## Status

Delivered as a sequence of pull requests:

| PR | Scope | State |
|----|-------|-------|
| 1 | Solution scaffold, layered projects, CI | done |
| 2 | Domain model — accounts, transactions, invariants | planned |
| 3 | Application services + EF Core / SQLite persistence | planned |
| 4 | Web API — endpoints, error handling | planned |
| 5 | React frontend | planned |

## Architecture

Layered, with dependencies pointing inward:

```
Atm.Api  ──►  Atm.Application  ──►  Atm.Domain
   │                                    ▲
   └──►  Atm.Infrastructure  ───────────┘
              (implements interfaces defined in Application)
```

- **Domain** — entities (`Account`, `Transaction`) and business rules (no overdraft,
  positive amounts). No framework dependencies.
- **Application** — use-case services (deposit / withdraw / transfer) and the repository
  interface they depend on.
- **Infrastructure** — EF Core `DbContext` and the repository implementation.
- **Api** — controllers, request/response models, error-to-HTTP mapping.

## Project layout

```
src/
  Atm.Domain/          entities, business rules
  Atm.Application/      use-case services, repository interface
  Atm.Infrastructure/  EF Core DbContext + repository
  Atm.Api/             controllers, HTTP concerns
tests/
  Atm.UnitTests/        domain, persistence, and API tests
client/                 React + TypeScript SPA
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) — `brew install --cask dotnet-sdk` on macOS
- Node 20+ — for the `client/` frontend

## Run

```bash
dotnet run --project src/Atm.Api
```

On first start the app creates a SQLite database at `src/Atm.Api/atm.db`, applies the
migrations, and seeds the two accounts (`Checking`, `Savings`). The file persists between
runs — delete it to start over. The connection string lives in
`src/Atm.Api/appsettings.json`.

Ports are defined in `src/Atm.Api/Properties/launchSettings.json`. In Development the
OpenAPI document is served at `/openapi/v1.json`; `GET /health` returns `{"status":"ok"}`.

### Adding a migration

The EF Core CLI is pinned in `.config/dotnet-tools.json`:

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project src/Atm.Infrastructure --startup-project src/Atm.Infrastructure
```

A design-time factory (`AtmDbContextFactory`) builds the context, so the API host does not
need to start. Applying migrations needs nothing extra — the app does it on startup.

## API

All endpoints return JSON. Failures are RFC 7807 `application/problem+json` with a `title`
and `detail`.

| Method | Route | Body | Success | Errors |
|--------|-------|------|---------|--------|
| `GET` | `/accounts` | — | `200` — every account with balance and history | — |
| `POST` | `/accounts/{id}/deposit` | `{ "amount": 50.00 }` | `200` — the updated account | `404` unknown account · `422` invalid amount |
| `POST` | `/accounts/{id}/withdraw` | `{ "amount": 50.00 }` | `200` — the updated account | `404` · `409` insufficient funds · `422` invalid amount |
| `POST` | `/transfers` | `{ "fromAccountId": "…", "toAccountId": "…", "amount": 50.00 }` | `200` — both updated accounts | `404` · `409` insufficient funds · `422` invalid / same-account transfer |

Amounts must be positive with at most two decimal places — the domain enforces this and a
violation maps to `422`; malformed JSON returns `400`.

```bash
curl -s localhost:5078/accounts
curl -s -X POST localhost:5078/accounts/<id>/deposit \
  -H 'Content-Type: application/json' -d '{"amount":25.00}'
```

## Frontend

A React + TypeScript SPA in `client/`. Run it alongside the API:

```bash
cd client
npm install
npm run dev
```

The dev server runs on `http://localhost:5173` and proxies `/accounts` and `/transfers`
to the API on `:5078`, so start `dotnet run --project src/Atm.Api` first. `npm run build`
produces a production bundle in `client/dist/`.

## Test

```bash
dotnet test
```

```bash
dotnet test
```
