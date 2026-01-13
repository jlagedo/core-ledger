# API Client Generation with NSwag

This guide explains how to generate TypeScript DTOs and API clients from the Core Ledger .NET API using NSwag.

## Overview

The Core Ledger monorepo uses [NSwag](https://github.com/RicoSuter/NSwag) to automatically generate type-safe TypeScript clients from the OpenAPI specification exposed by the .NET API.

### Benefits

- **Type Safety**: Full TypeScript types for all API requests and responses
- **Auto-Generated**: No manual DTO writing or API client maintenance
- **Always in Sync**: Regenerate after API changes to stay current
- **Angular Native**: Uses Angular's HttpClient with RxJS Observables
- **Multiple Clients**: One client per API controller (tag-based)
- **Interfaces Only**: Lightweight TypeScript interfaces for DTOs (no classes)

## Architecture

```
┌─────────────────────┐
│  .NET API (Running) │
│  Port: 7109         │
└──────────┬──────────┘
           │
           │ GET /swagger/v1/swagger.json
           ▼
┌─────────────────────────────┐
│  OpenAPI Specification      │
│  apps/core-ledger-api/      │
│    openapi.json             │
└──────────┬──────────────────┘
           │
           │ nswag run nswag.json
           ▼
┌─────────────────────────────┐
│  TypeScript Clients         │
│  libs/api-client/           │
│    generated/               │
│      api-clients.ts         │
│      - Client per Tag       │
│      - DTO Interfaces       │
└─────────────────────────────┘
           │
           │ import
           ▼
┌─────────────────────────────┐
│  Angular UI                 │
│  apps/core-ledger-ui/       │
└─────────────────────────────┘
```

## Prerequisites

### 1. NSwag CLI Tool

Install NSwag globally:

```bash
npm install -g nswag
```

Verify installation:

```bash
nswag version
```

### 2. NSwag.MSBuild Package

Already configured in the .NET API project for automated generation during Release builds.

## Generation Workflow

### Quick Start

```bash
# 1. Start infrastructure
npm run docker:up

# 2. Start the API (in a separate terminal)
npm run start:api

# 3. Wait for API to be ready (watch for "Core Ledger API started successfully")

# 4. Export OpenAPI spec
npm run api:export-spec

# 5. Generate TypeScript client
npm run api:generate-client
```

### Step-by-Step Explanation

#### Step 1: Start the API

The API must be running to export the OpenAPI specification:

```bash
nx serve core-ledger-api
```

Wait for the log message:
```
Core Ledger API is now listening on:
  → https://localhost:7109
```

#### Step 2: Export OpenAPI Specification

Export the OpenAPI spec from the running API:

```bash
# Using npm script
npm run api:export-spec

# Or using Nx directly
nx run core-ledger-api:export-openapi
```

This creates `apps/core-ledger-api/openapi.json` containing the complete API specification.

#### Step 3: Generate TypeScript Client

Generate the TypeScript client using NSwag:

```bash
# Using npm script
npm run api:generate-client

# Or using Nx directly
nx run api-client:generate
```

This generates code in `libs/api-client/generated/api-clients.ts` including:

- **Clients**: One client class per API controller (FundosClient, CalendárioClient, etc.)
- **DTOs**: TypeScript interfaces for all API models
- **Enums**: TypeScript enums for API enumerations

#### Step 4: Verify Generation

Check the generated files:

```bash
ls -la libs/api-client/generated/
```

You should see:
- `api-clients.ts` - All clients, DTOs, and enums in a single file

## NSwag Configuration

### Configuration File

NSwag is configured via `apps/core-ledger-api/nswag.json`:

```json
{
  "runtime": "Net90",
  "documentGenerator": {
    "fromDocument": {
      "json": "openapi.json"
    }
  },
  "codeGenerators": {
    "openApiToTypeScriptClient": {
      "className": "{controller}Client",
      "template": "Angular",
      "typeScriptVersion": 5.0,
      "rxJsVersion": 7.0,
      "httpClass": "HttpClient",
      "injectionTokenType": "InjectionToken",
      "useSingletonProvider": true,
      "generateClientClasses": true,
      "generateClientInterfaces": true,
      "operationGenerationMode": "MultipleClientsFromFirstTagAndOperationId",
      "typeStyle": "Interface",
      "enumStyle": "Enum",
      "dateTimeType": "Date",
      "nullValue": "Undefined",
      "generateOptionalParameters": true,
      "markOptionalProperties": true,
      "baseUrlTokenName": "API_BASE_URL",
      "output": "../../libs/api-client/generated/api-clients.ts"
    }
  }
}
```

### Key Configuration Options

| Option | Value | Description |
|--------|-------|-------------|
| `template` | `Angular` | Generates Angular-compatible clients with HttpClient |
| `operationGenerationMode` | `MultipleClientsFromFirstTagAndOperationId` | One client per API controller |
| `typeStyle` | `Interface` | DTOs as interfaces (not classes) |
| `enumStyle` | `Enum` | Generates TypeScript enums |
| `baseUrlTokenName` | `API_BASE_URL` | InjectionToken for base URL |
| `httpClass` | `HttpClient` | Uses Angular HttpClient |
| `rxJsVersion` | `7.0` | RxJS version for Observables |

## Using the Generated Client

### 1. Configure Providers in Angular

In `app.config.ts`:

```typescript
import { API_BASE_URL } from '@core-ledger/api-client';

export const appConfig: ApplicationConfig = {
  providers: [
    // ... other providers
    { provide: API_BASE_URL, useValue: '' }, // Empty = use proxy
  ],
};
```

### 2. Create API Client Service

The `ApiClientService` acts as a facade for all generated clients:

```typescript
// apps/core-ledger-ui/src/app/api/api-client.service.ts
import { Injectable, inject } from '@angular/core';
import {
  FundosClient,
  CalendárioClient,
  IndexadoresClient,
  UsersClient,
  AccountsClient,
  SecuritiesClient,
  TransactionsClient,
} from '@core-ledger/api-client';

@Injectable({ providedIn: 'root' })
export class ApiClientService {
  readonly fundos = inject(FundosClient);
  readonly calendario = inject(CalendárioClient);
  readonly indexadores = inject(IndexadoresClient);
  readonly users = inject(UsersClient);
  readonly accounts = inject(AccountsClient);
  readonly securities = inject(SecuritiesClient);
  readonly transactions = inject(TransactionsClient);
}
```

### 3. Use in Services

```typescript
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiClientService } from '../api/api-client.service';
import { CreateFundoDto } from '@core-ledger/api-client';

@Injectable({ providedIn: 'root' })
export class FundoService {
  private readonly apiClient = inject(ApiClientService);

  getAllFundos(): Observable<Fundo[]> {
    return this.apiClient.fundos.getAllFundos();
  }

  getFundoById(id: number): Observable<Fundo> {
    return this.apiClient.fundos.getFundoById(id);
  }

  createFundo(fundo: CreateFundoDto): Observable<Fundo> {
    return this.apiClient.fundos.createFundo(fundo);
  }

  updateFundo(id: number, fundo: UpdateFundoDto): Observable<void> {
    return this.apiClient.fundos.updateFundo(id, fundo);
  }

  deleteFundo(id: number): Observable<void> {
    return this.apiClient.fundos.deleteFundo(id);
  }
}
```

### 4. Use in Components

```typescript
import { Component, inject, signal } from '@angular/core';
import { FundoService } from '../../services/fundo.service';

@Component({
  selector: 'app-fundos',
  template: `
    <h1>Funds</h1>
    @if (loading()) {
      <p>Loading...</p>
    } @else {
      <ul>
        @for (fund of funds(); track fund.id) {
          <li>{{ fund.nome }}</li>
        }
      </ul>
    }
  `,
})
export class FundosComponent {
  private readonly fundoService = inject(FundoService);

  funds = signal<Fundo[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.fundoService.getAllFundos().subscribe({
      next: (result) => {
        this.funds.set(result);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load funds:', error);
        this.loading.set(false);
      },
    });
  }
}
```

## Generated Client Classes

NSwag generates one client per API controller based on OpenAPI tags:

| Client | API Controller |
|--------|----------------|
| `FundosClient` | Fundos (Funds) |
| `CalendárioClient` | Calendar |
| `IndexadoresClient` | Indexes |
| `HistoricosIndexadoresClient` | Index History |
| `UsersClient` | Users |
| `AccountsClient` | Chart of Accounts |
| `SecuritiesClient` | Securities |
| `TransactionsClient` | Transactions |
| `ANBIMA_ClassificationsClient` | ANBIMA Classifications |

## Type Handling

### Enum Compatibility

NSwag generates enums with `_0`, `_1` naming convention. When using with local Angular enums, use explicit type assertions:

```typescript
import { TipoDia } from '../models/calendario.model';
import { CreateCalendarioDto, TipoDia as NSwagTipoDia } from '@core-ledger/api-client';

// Local enum → NSwag DTO
const dto: CreateCalendarioDto = {
  tipoDia: localTipoDia as unknown as NSwagTipoDia,
};
```

### Null vs Undefined

NSwag uses `undefined` for optional fields. Convert `null` values:

```typescript
const dto = {
  descricao: localModel.descricao ?? undefined, // null → undefined
};
```

## Nx Integration

### Project Configuration

The `api-client` library is configured in `libs/api-client/project.json`:

```json
{
  "name": "api-client",
  "projectType": "library",
  "sourceRoot": "libs/api-client/src",
  "targets": {
    "generate": {
      "executor": "nx:run-commands",
      "options": {
        "commands": ["npx nswag run nswag.json"],
        "cwd": "apps/core-ledger-api",
        "parallel": false
      },
      "dependsOn": [
        {
          "target": "export-openapi",
          "projects": ["core-ledger-api"]
        }
      ],
      "inputs": ["{workspaceRoot}/apps/core-ledger-api/openapi.json"],
      "outputs": ["{projectRoot}/generated"]
    }
  }
}
```

### Nx Task Dependencies

When you run `nx run api-client:generate`, Nx automatically:

1. Runs `nx run core-ledger-api:export-openapi` first (if needed)
2. Then runs the NSwag generation

This ensures the OpenAPI spec is always up-to-date before generating.

### MSBuild Integration (Optional)

NSwag can also run automatically during .NET Release builds via the MSBuild target in `CoreLedger.API.csproj`:

```xml
<Target Name="NSwag" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Release'">
  <Exec WorkingDirectory="$(ProjectDir)/.."
        Command="$(NSwagExe_Net90) run nswag.json /variables:Configuration=$(Configuration)" />
</Target>
```

## Troubleshooting

### API Not Running

**Error**: `curl: (7) Failed to connect`

**Solution**: Start the API first:
```bash
nx serve core-ledger-api
```

### NSwag Not Found

**Error**: `nswag: command not found`

**Solution**: Install NSwag globally:
```bash
npm install -g nswag
```

### Generation Errors

**Error**: Various NSwag generation errors

**Solution**:
1. Ensure OpenAPI spec is valid by visiting https://localhost:7109/swagger
2. Delete `libs/api-client/generated/` and regenerate
3. Check nswag.json configuration

### Type Errors After Generation

**Error**: TypeScript compilation errors

**Solution**:
1. Run `npm install` to ensure all packages are installed
2. Check `tsconfig.base.json` has the correct path mapping
3. Restart your IDE/TypeScript server

### Single Client Generated

**Error**: Only one client generated instead of multiple

**Solution**: Ensure `operationGenerationMode` is set to `MultipleClientsFromFirstTagAndOperationId` in nswag.json.

## Best Practices

### 1. Regenerate After API Changes

Always regenerate the client after modifying the API:

```bash
# After changing API endpoints or DTOs
npm run api:generate-client
```

### 2. Don't Edit Generated Code

Never modify files in `libs/api-client/generated/`. They will be overwritten on next generation.

### 3. Use ApiClientService Facade

Access all clients through the `ApiClientService` facade rather than injecting clients directly:

```typescript
// Good
private readonly apiClient = inject(ApiClientService);
this.apiClient.fundos.getAllFundos();

// Avoid
private readonly fundosClient = inject(FundosClient);
this.fundosClient.getAllFundos();
```

### 4. Handle Type Compatibility

Create mapping utilities for enum and null handling:

```typescript
function toNSwagEnum<T>(value: unknown): T {
  return value as T;
}

function nullToUndefined<T>(value: T | null): T | undefined {
  return value ?? undefined;
}
```

### 5. Version Control

Commit the generated code to version control for these reasons:
- Teammates can build without running the API
- CI/CD can build without starting the API
- Provides a clear diff when API changes

## References

- [NSwag GitHub Repository](https://github.com/RicoSuter/NSwag)
- [NSwag Documentation](https://github.com/RicoSuter/NSwag/wiki)
- [OpenAPI Specification](https://swagger.io/specification/)

## Summary

1. **Install NSwag**: `npm install -g nswag`
2. **Start API**: `nx serve core-ledger-api`
3. **Export Spec**: `npm run api:export-spec`
4. **Generate Client**: `npm run api:generate-client`
5. **Use in Angular**: Import from `@core-ledger/api-client`

The generated clients provide full type safety and native Angular HttpClient integration with RxJS Observables!
