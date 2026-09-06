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
  Atm.UnitTests/        domain + application tests
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) — `brew install --cask dotnet-sdk` on macOS
- Node 20+ — for the frontend (from PR 5)

## Run

```bash
dotnet run --project src/Atm.Api
```

Ports are defined in `src/Atm.Api/Properties/launchSettings.json`. In Development the
OpenAPI document is served at `/openapi/v1.json`; `GET /health` returns `{"status":"ok"}`.

## Test

```bash
dotnet test
```
