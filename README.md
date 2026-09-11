# Boundless Enterprises — Backend

The .NET API for the Boundless Enterprises central platform (the holding
company's digital operating layer). Built as a **modular monolith**: one app
project with clear internal boundaries kept as folders, following the platform
architecture document.

> Frontend lives in a separate repository: **Boundlessinc** (Next.js).

## Solution layout

```
backend/
├── BoundlessEnterprises.sln
├── docker-compose.yml           # local SQL Server 2022
├── src/
│   └── BoundlessEnterprises/     # the app (net8.0 web) — one project
│       ├── Domain/               # business concepts, no infrastructure
│       ├── Application/          # use cases (CQRS + MediatR), validation
│       ├── Infrastructure/       # EF Core, SQL Server, persistence, seed
│       ├── Api/                  # thin HTTP surface (minimal-API endpoint modules)
│       └── Program.cs            # web host + DI wiring
└── tests/
    └── BoundlessEnterprises.Tests/   # unit tests (xUnit)
```

Two projects: the app and its tests. The layer folders keep the same internal
boundaries — Domain holds no EF Core / HTTP / SDK concerns — just without the
overhead of separate `.csproj` projects.

## Layer responsibilities

| Folder | Holds | Never holds |
|--------|-------|-------------|
| Domain | Entities, enums, domain events, invariants | HTTP, EF Core, Stripe |
| Application | Commands/Queries, handlers, validators, DTOs, interfaces | Concrete persistence, controllers |
| Infrastructure | `ApplicationDbContext`, EF configurations, migrations, seeders, external clients | Business rules |
| Api | Endpoint modules, middleware, DI wiring, auth | Business logic |

Endpoints stay thin: `HTTP request → authenticate / validate → Application use case → HTTP response`.

## Database

- **Microsoft SQL Server**, one central holding-company database.
- EF Core code-first with migrations.
- SQL Server **schemas** mirror capability boundaries so the database reads
  clearly in SSMS: `core`, `identity`, `hr`, `onboarding`, `billing`,
  `documents`, `integration`, `intelligence`, `audit`.
- The central DB stores parent-level enterprise data only — never a copy of a
  subsidiary's full operating database.

## Getting started

Prerequisites: **.NET 8 SDK**, **Docker** (for local SQL Server).

```bash
# 1. Start SQL Server
docker compose up -d

# 2. Restore & build
dotnet build

# 3. Run the API (applies migrations + seeds the portfolio in Development)
dotnet run --project src/BoundlessEnterprises

# API:     http://localhost:5080
# Swagger: http://localhost:5080/swagger
# Health:  http://localhost:5080/health
```

The connection string lives in `src/BoundlessEnterprises/appsettings.json`
under `ConnectionStrings:Default` and can be overridden with the
`ConnectionStrings__Default` environment variable.

### Migrations

```bash
dotnet ef migrations add <Name> \
  --project src/BoundlessEnterprises \
  --output-dir Infrastructure/Persistence/Migrations

dotnet ef database update --project src/BoundlessEnterprises
```

### Tests

```bash
dotnet test
```

## Capability modules (roadmap)

Companies · Identity & Access · Employees · Onboarding · Payments · Documents ·
Integrations · Intelligence. Each is a first-class module on both frontend and
backend. Phase 0 ships the **Companies** directory end-to-end plus the shared
foundations; later phases add identity, the public site, and the reusable
engines.
