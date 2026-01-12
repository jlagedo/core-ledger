# Core Ledger

A comprehensive **fund accounting ABOR (Accounting Book of Records)** system for the Brazilian investment fund market. Implements Brazilian regulatory requirements (CVM 175, ANBIMA, etc.).

## Overview

Core Ledger is an enterprise-grade monorepo containing:

- **Web Application** - Angular 21 frontend with modern signals-based architecture
- **REST API** - .NET 10 backend with Clean Architecture
- **Background Worker** - .NET 10 service for async processing via RabbitMQ
- **ETL Pipeline** - Meltano-based data integration for B3 financial instruments
- **E2E Tests** - Playwright test suite with Brazilian financial test data

## Project Structure

```
core-ledger/
├── apps/
│   ├── core-ledger-ui/        # Angular 21 frontend
│   ├── core-ledger-api/       # .NET 10 Web API
│   ├── core-ledger-worker/    # .NET 10 Background Worker
│   └── core-ledger-e2e/       # Playwright E2E tests
├── libs/
│   └── core-ledger-dotnet/    # Shared .NET libraries
│       ├── CoreLedger.Domain/
│       ├── CoreLedger.Application/
│       ├── CoreLedger.Infrastructure/
│       ├── CoreLedger.UnitTests/
│       └── CoreLedger.IntegrationTests/
├── tools/
│   └── etl/                   # Meltano ETL pipeline
├── docs/                      # Documentation
│   ├── specs/                 # Feature specifications
│   ├── testing/               # Testing documentation
│   ├── etl/                   # ETL documentation
│   └── compliance/            # Compliance documentation
├── CoreLedger.sln             # Root .NET solution
├── nx.json                    # Nx workspace configuration
└── package.json               # Root package.json
```

## Quick Start

### Prerequisites

- **Node.js** >= 20.9.0
- **npm** >= 11.6.2
- **.NET SDK** 10.0
- **Docker** (for infrastructure services)

### 1. Install Dependencies

```bash
npm install
dotnet restore CoreLedger.sln
```

### 2. Start Infrastructure

```bash
npm run docker:up
# Starts: PostgreSQL (5432), Redis (6379), RabbitMQ (5672/15672)
```

### 3. Run the Applications

```bash
# Terminal 1: Start the API
nx serve core-ledger-api

# Terminal 2: Start the UI
nx serve core-ledger-ui

# Open http://localhost:4200 (login: admin / any password)
```

## Development Commands

### Nx Commands (Recommended)

```bash
# Serve applications
nx serve core-ledger-ui              # Angular dev server (http://localhost:4200)
nx serve core-ledger-ui -c local-auth # With Auth0 authentication
nx serve core-ledger-api             # .NET API (https://localhost:7109)
nx serve core-ledger-worker          # .NET Worker

# Build
nx build core-ledger-ui              # Build Angular
nx build core-ledger-api             # Build .NET API
nx run-many -t build                 # Build all projects

# Test
nx test core-ledger-ui               # Angular unit tests (Vitest)
nx test core-ledger-dotnet           # .NET unit tests
nx e2e core-ledger-e2e               # Playwright E2E tests

# Utilities
nx graph                             # Visualize dependency graph
nx affected -t test                  # Test only affected projects
```

### npm Scripts

```bash
npm start                    # Start Angular UI (mock auth)
npm run start:auth           # Start Angular UI (Auth0)
npm run start:api            # Start .NET API
npm run build                # Build all projects
npm run test                 # Test all projects
npm run e2e                  # Run E2E tests
npm run docker:up            # Start infrastructure
npm run docker:down          # Stop infrastructure
```

### .NET Commands

```bash
dotnet build CoreLedger.sln
dotnet test CoreLedger.sln
nx run core-ledger-api:migrate       # Run EF Core migrations
```

## Technology Stack

| Component | Technology |
|-----------|------------|
| Frontend | Angular 21, Bootstrap 5, NgRx Signals |
| Backend | .NET 10, Entity Framework Core, MediatR |
| Database | PostgreSQL 18 |
| Cache | Redis |
| Queue | RabbitMQ |
| ETL | Meltano, DBT, Python |
| Testing | Vitest (UI), xUnit (.NET), Playwright (E2E) |
| Build | Nx, Angular CLI, .NET CLI |

## Architecture

### Dependency Graph

```
core-ledger-e2e → core-ledger-ui
                → core-ledger-api → core-ledger-dotnet

core-ledger-worker → core-ledger-dotnet
```

### .NET Clean Architecture

- **Domain** - Pure business logic, no external dependencies
- **Application** - Use cases with MediatR (CQRS), validators, DTOs
- **Infrastructure** - EF Core, PostgreSQL, RabbitMQ integrations
- **API/Worker** - Entry points consuming shared libraries

### Database Schemas

```
cadastros  → Funds, assets, investors
carteira   → Operations, positions
passivo    → Investor movements
cota       → NAV, shares, closing
pricing    → Prices and indexes
audit      → Logs and history
```

## Documentation

Detailed documentation is available in the `/docs` directory:

- **[Specifications](docs/specs/)** - Feature specifications for API and UI
- **[Testing](docs/testing/)** - E2E test coverage and Brazilian test data
- **[ETL](docs/etl/)** - B3 instruments pipeline documentation
- **[Compliance](docs/compliance/)** - Angular compliance review guides

### Claude Code Instructions

Each project has a `CLAUDE.md` file with AI-assisted development guidance:

- [`/CLAUDE.md`](CLAUDE.md) - Monorepo overview
- [`/apps/core-ledger-ui/CLAUDE.md`](apps/core-ledger-ui/CLAUDE.md) - Angular UI patterns
- [`/tools/etl/CLAUDE.md`](tools/etl/CLAUDE.md) - Meltano ETL guidance

## ETL Pipeline

Process B3 financial instruments data:

```bash
cd tools/etl
meltano install
export TARGET_POSTGRES_PASSWORD=postgres
bash extract/preprocess_b3_instruments.sh && meltano run b3_instruments_pipeline
```

See [ETL Documentation](docs/etl/README.md) for details.

## Contributing

1. Create a feature branch from `main`
2. Make changes following project code standards
3. Run tests: `npm run test && dotnet test CoreLedger.sln`
4. Submit a pull request

## License

Proprietary - All rights reserved
