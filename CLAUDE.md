# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Constraints

- **Never start/serve applications** - Do not run `nx serve`, `npm start`, `npm run start:*`, `dotnet run`, or any commands that start dev servers. The user manages application lifecycle manually.

## Repository Overview

This is a **monorepo for Core Ledger**, a fund accounting ABOR (Accounting Book of Records) system for the Brazilian investment fund market. It implements Brazilian regulatory requirements (CVM 175, ANBIMA, etc.).

The monorepo uses **Nx** for workspace management with an integrated dependency structure.

### Project Structure

```
core-ledger/
├── apps/
│   ├── core-ledger-ui/        # Angular 21 frontend
│   ├── core-ledger-api/       # .NET 10 Web API
│   ├── core-ledger-worker/    # .NET 10 Background Worker
│   └── core-ledger-e2e/       # Playwright E2E tests
├── libs/
│   ├── api-client/            # Generated TypeScript API client (Kiota)
│   └── core-ledger-dotnet/    # Shared .NET libraries
│       ├── CoreLedger.Domain/
│       ├── CoreLedger.Application/
│       ├── CoreLedger.Infrastructure/
│       ├── CoreLedger.UnitTests/
│       └── CoreLedger.IntegrationTests/
├── tools/
│   └── etl/                   # Meltano ETL (standalone)
├── docs/                      # Centralized documentation
│   ├── specs/api/             # API specifications
│   ├── specs/ui/              # UI specifications
│   ├── testing/               # E2E test documentation
│   ├── etl/                   # ETL pipeline documentation
│   └── compliance/            # Compliance documentation
├── CoreLedger.sln             # Root solution for IDE support
├── nx.json                    # Nx workspace configuration
└── package.json               # Root package.json
```

| Project | Technology | Description |
|---------|------------|-------------|
| `core-ledger-ui` | Angular 21, Bootstrap 5 | Web frontend with signals |
| `core-ledger-api` | .NET 10 | REST API (Clean Architecture) |
| `core-ledger-worker` | .NET 10 | RabbitMQ background worker |
| `api-client` | TypeScript, NSwag | Generated API client with DTOs |
| `core-ledger-dotnet` | .NET 10 | Shared Domain/Application/Infrastructure |
| `core-ledger-e2e` | Playwright | E2E tests (baseURL: http://localhost:5071) |
| `tools/etl` | Meltano, DBT, Python | ETL for B3 financial instruments |

## Quick Commands

### Nx Commands (Preferred)
```bash
# Serve
nx serve core-ledger-ui              # Angular dev server (http://localhost:4200)
nx serve core-ledger-ui -c local-auth # With Auth0 authentication
nx serve core-ledger-api             # .NET API with hot reload
nx serve core-ledger-worker          # .NET Worker with hot reload

# Build
nx build core-ledger-ui              # Build Angular
nx build core-ledger-api             # Build .NET API
nx build core-ledger-worker          # Build .NET Worker
nx build core-ledger-dotnet          # Build shared .NET libraries
nx run-many -t build                 # Build all

# Test
nx test core-ledger-ui               # Angular Vitest tests
nx test core-ledger-dotnet           # .NET unit tests
nx run core-ledger-dotnet:test:integration  # .NET integration tests
nx e2e core-ledger-e2e               # Playwright E2E tests

# Other
nx graph                             # Visualize dependency graph
nx affected -t test                  # Test only affected projects
nx run core-ledger-api:migrate       # Run EF Core migrations

# API Client Generation
nx run core-ledger-api:export-openapi  # Export OpenAPI spec from running API
nx run api-client:generate           # Generate TypeScript client with NSwag
```

### npm Scripts
```bash
npm start                    # Start Angular UI
npm run start:auth           # Start Angular UI with Auth0
npm run start:api            # Start .NET API
npm run start:worker         # Start .NET Worker
npm run build                # Build all projects
npm run test                 # Test all projects
npm run e2e                  # Run E2E tests
npm run db:migrate           # Run database migrations
npm run docker:up            # Start infrastructure services
npm run docker:down          # Stop infrastructure services
npm run api:export-spec      # Export OpenAPI spec from running API
npm run api:generate-client  # Generate TypeScript API client
```

### Direct .NET Commands (from workspace root)
```bash
dotnet restore CoreLedger.sln
dotnet build CoreLedger.sln
dotnet test CoreLedger.sln
```

### ETL (Meltano) - Standalone
```bash
cd tools/etl
meltano install
export TARGET_POSTGRES_PASSWORD=postgres
bash extract/preprocess_b3_instruments.sh && meltano run b3_instruments_pipeline
```

## Development Workflow

1. **Start infrastructure**: `npm run docker:up` (PostgreSQL 5432, Redis 6379, RabbitMQ 5672/15672)
2. **Run API**: `nx serve core-ledger-api` (https://localhost:7109)
3. **Run UI**: `nx serve core-ledger-ui` (http://localhost:4200, proxies to API)

## Architecture

### Nx Workspace Structure
- **apps/** - Deployable applications (Angular UI, .NET API, .NET Worker, E2E tests)
- **libs/** - Shared libraries (shared .NET code)
- **tools/** - Standalone tools (Meltano ETL)

### Dependency Graph
```
core-ledger-e2e → core-ledger-ui → api-client
                → core-ledger-api → core-ledger-dotnet

core-ledger-worker → core-ledger-dotnet

api-client → core-ledger-api (for OpenAPI spec generation)
```

### .NET Architecture (Clean Architecture)
- **Domain** → Pure business logic, no dependencies
- **Application** → Use cases with MediatR (CQRS), validators, DTOs
- **Infrastructure** → EF Core, PostgreSQL, RabbitMQ
- **API/Worker** → Entry points using shared libraries

### Angular Architecture
- Standalone components with OnPush change detection
- `inject()` function instead of constructor injection
- `input()` and `output()` instead of decorators
- Native control flow: `@if`, `@for`, `@switch`
- Mock API required for all endpoints

### Database Schemas (PostgreSQL)
```
cadastros  → Funds, assets, investors
carteira   → Operations, positions
passivo    → Investor movements
cota       → NAV, shares, closing
pricing    → Prices and indexes
audit      → Logs and history
```

## Key Patterns

### Nx Task Caching
Nx caches build outputs. Use `nx reset` to clear cache if needed.

### Project-Specific Instructions
Each project has its own `CLAUDE.md` with detailed architecture and commands:
- `apps/core-ledger-ui/CLAUDE.md` - Angular UI patterns and conventions
- `tools/etl/CLAUDE.md` - Meltano ETL guidance

### API Client Generation (NSwag)
TypeScript DTOs and API clients are auto-generated from the .NET API using NSwag:
1. Start API: `nx serve core-ledger-api`
2. Export spec: `npm run api:export-spec`
3. Generate client: `npm run api:generate-client`
4. Import clients: `import { FundosClient, CalendárioClient } from '@core-ledger/api-client'`

**NSwag Configuration:**
- Config file: `apps/core-ledger-api/nswag.json`
- Generates one client per API controller (tag-based)
- Uses TypeScript interfaces (not classes) for DTOs
- Native Angular HttpClient with RxJS Observables

See `docs/api-client-generation.md` for complete guide.

### Angular MCP Tools with Nx Monorepo

The Angular CLI MCP's `list_projects` tool is designed for standard Angular CLI workspaces (`angular.json`), not Nx monorepos that use `project.json` per project. Use these workarounds:

**Use Nx CLI instead of `list_projects`:**
```bash
npx nx show projects                        # List all projects
npx nx show project core-ledger-ui --json   # Get project details (root, sourceRoot, prefix, targets)
```

**MCP tools work without `workspacePath`:**
| Tool | Usage |
|------|-------|
| `get_best_practices` | Call without arguments → returns generic guide |
| `search_documentation` | Pass `version: 21` manually |
| `find_examples` | Call without arguments → returns generic examples |

**Project metadata from `project.json`:**
- Angular version: `21` (from `package.json`)
- Test framework: Vitest (executor: `@angular/build:unit-test`)
- Style language: SCSS (from `inlineStyleLanguage`)
- Prefix: `app`
- Source root: `apps/core-ledger-ui/src`

### Documentation
All documentation is centralized in `/docs/`:
- `docs/specs/api/` - API specifications (.NET)
- `docs/specs/ui/` - UI specifications (Angular)
- `docs/testing/` - E2E test coverage and Brazilian test data
- `docs/etl/` - B3 instruments ETL pipeline
- `docs/compliance/` - Angular compliance review guides
- `docs/api-client-generation.md` - TypeScript client generation with NSwag
