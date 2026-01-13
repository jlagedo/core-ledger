# API Client Generation with Kiota

This guide explains how to generate TypeScript DTOs and API clients from the Core Ledger .NET API using Microsoft Kiota.

## Overview

The Core Ledger monorepo uses [Microsoft Kiota](https://learn.microsoft.com/en-us/openapi/kiota/overview) to automatically generate type-safe TypeScript clients from the OpenAPI specification exposed by the .NET API.

### Benefits

- **Type Safety**: Full TypeScript types for all API requests and responses
- **Auto-Generated**: No manual DTO writing or API client maintenance
- **Always in Sync**: Regenerate after API changes to stay current
- **IntelliSense**: Full IDE support with autocomplete and type checking

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
           │ kiota generate
           ▼
┌─────────────────────────────┐
│  TypeScript Client          │
│  libs/api-client/           │
│    generated/               │
│      - DTOs                 │
│      - API Methods          │
│      - Request Builders     │
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

### 1. Kiota CLI Tool

Install Kiota globally:

```bash
dotnet tool install --global Microsoft.OpenApi.Kiota
```

Verify installation:

```bash
kiota --version
```

### 2. Kiota Runtime Packages

Already installed in the monorepo:

- `@microsoft/kiota-abstractions`
- `@microsoft/kiota-http-fetchlibrary`
- `@microsoft/kiota-serialization-json`
- `@microsoft/kiota-serialization-form`
- `@microsoft/kiota-serialization-text`
- `@microsoft/kiota-serialization-multipart`

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

Generate the TypeScript client using Kiota:

```bash
# Using npm script
npm run api:generate-client

# Or using Nx directly
nx run api-client:generate
```

This generates code in `libs/api-client/generated/` including:

- **DTOs**: TypeScript interfaces for all API models
- **API Client**: `CoreLedgerApiClient` class with all endpoints
- **Request Builders**: Fluent API for building requests

#### Step 4: Verify Generation

Check the generated files:

```bash
ls -la libs/api-client/generated/
```

You should see:
- `coreLedgerApiClient.ts` - Main client class
- `models/` - Generated DTOs
- Various endpoint folders (funds, accounts, etc.)

## Using the Generated Client

### 1. Import in Angular

```typescript
import { CoreLedgerApiClient } from '@core-ledger/api-client';
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';
```

### 2. Create Client Instance

```typescript
// In a service or component
const adapter = new FetchRequestAdapter();
adapter.baseUrl = 'https://localhost:7109';

const client = new CoreLedgerApiClient(adapter);
```

### 3. Make API Calls

#### Get All Funds
```typescript
const funds = await client.api.v1.funds.get();
console.log(funds);
```

#### Get Fund by ID
```typescript
const fundId = '123e4567-e89b-12d3-a456-426614174000';
const fund = await client.api.v1.funds.byId(fundId).get();
console.log(fund.name);
```

#### Create New Fund
```typescript
import { CreateFundRequest } from '@core-ledger/api-client';

const newFund: CreateFundRequest = {
  name: 'My Investment Fund',
  cnpj: '12345678000190',
  // ... other properties
};

const created = await client.api.v1.funds.post(newFund);
console.log('Created fund:', created.id);
```

#### Update Fund
```typescript
const fundId = '123e4567-e89b-12d3-a456-426614174000';

await client.api.v1.funds.byId(fundId).put({
  name: 'Updated Fund Name',
  // ... other properties
});
```

#### Delete Fund
```typescript
const fundId = '123e4567-e89b-12d3-a456-426614174000';
await client.api.v1.funds.byId(fundId).delete();
```

### 4. With Authentication

For authenticated requests, add bearer token:

```typescript
import { AnonymousAuthenticationProvider } from '@microsoft/kiota-abstractions';
import { HttpClient } from '@microsoft/kiota-http-fetchlibrary';

const authProvider = new AnonymousAuthenticationProvider();

const httpClient = HttpClient.create({
  middleware: [
    {
      intercept: async (url, init, next) => {
        const token = await getAccessToken(); // Your auth logic
        init.headers = {
          ...init.headers,
          Authorization: `Bearer ${token}`,
        };
        return next(url, init);
      },
    },
  ],
});

const adapter = new FetchRequestAdapter(
  authProvider,
  undefined,
  undefined,
  httpClient
);
adapter.baseUrl = 'https://localhost:7109';

const client = new CoreLedgerApiClient(adapter);
```

## Angular Integration Example

### Create API Service

```typescript
// apps/core-ledger-ui/src/app/services/api.service.ts
import { Injectable } from '@angular/core';
import { CoreLedgerApiClient } from '@core-ledger/api-client';
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly client: CoreLedgerApiClient;

  constructor() {
    const adapter = new FetchRequestAdapter();
    adapter.baseUrl = environment.apiUrl;
    this.client = new CoreLedgerApiClient(adapter);
  }

  get funds() {
    return this.client.api.v1.funds;
  }

  get accounts() {
    return this.client.api.v1.accounts;
  }

  get transactions() {
    return this.client.api.v1.transactions;
  }

  // Add more endpoint accessors as needed
}
```

### Use in Components

```typescript
// apps/core-ledger-ui/src/app/pages/funds/funds.component.ts
import { Component, inject, signal } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-funds',
  template: `
    <h1>Funds</h1>
    @if (loading()) {
      <p>Loading...</p>
    } @else {
      <ul>
        @for (fund of funds(); track fund.id) {
          <li>{{ fund.name }}</li>
        }
      </ul>
    }
  `,
})
export class FundsComponent {
  private readonly api = inject(ApiService);

  funds = signal<any[]>([]);
  loading = signal(true);

  async ngOnInit() {
    try {
      const result = await this.api.funds.get();
      this.funds.set(result.items ?? []);
    } catch (error) {
      console.error('Failed to load funds:', error);
    } finally {
      this.loading.set(false);
    }
  }
}
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
      "command": "kiota generate -l typescript -o generated -d ../../apps/core-ledger-api/openapi.json -c CoreLedgerApiClient -n CoreLedger.ApiClient --clean-output",
      "dependsOn": [
        {
          "target": "export-openapi",
          "projects": ["core-ledger-api"]
        }
      ]
    }
  }
}
```

### Nx Task Dependencies

When you run `nx run api-client:generate`, Nx automatically:

1. Runs `nx run core-ledger-api:export-openapi` first (if needed)
2. Then runs the Kiota generation

This ensures the OpenAPI spec is always up-to-date before generating.

## CI/CD Integration

### In GitHub Actions / CI Pipeline

```yaml
- name: Start API for OpenAPI generation
  run: |
    npm run docker:up
    npm run start:api &
    # Wait for API to be ready
    sleep 10

- name: Generate API client
  run: npm run api:generate-client

- name: Build UI with generated client
  run: nx build core-ledger-ui
```

## Troubleshooting

### API Not Running

**Error**: `curl: (7) Failed to connect`

**Solution**: Start the API first:
```bash
nx serve core-ledger-api
```

### Kiota Not Found

**Error**: `kiota: command not found`

**Solution**: Install Kiota globally:
```bash
dotnet tool install --global Microsoft.OpenApi.Kiota
```

Add to PATH (zsh):
```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

### Generation Errors

**Error**: Various Kiota generation errors

**Solution**:
1. Ensure OpenAPI spec is valid by visiting https://localhost:7109/swagger
2. Delete `libs/api-client/generated/` and regenerate
3. Check Kiota version: `kiota --version` (should be 1.29.0+)

### Type Errors After Generation

**Error**: TypeScript compilation errors

**Solution**:
1. Run `npm install` to ensure all Kiota packages are installed
2. Check `tsconfig.base.json` has the correct path mapping
3. Restart your IDE/TypeScript server

## Best Practices

### 1. Regenerate After API Changes

Always regenerate the client after modifying the API:

```bash
# After changing API endpoints or DTOs
npm run api:generate-client
```

### 2. Don't Edit Generated Code

Never modify files in `libs/api-client/generated/`. They will be overwritten on next generation.

### 3. Add Custom Logic in Wrappers

Create wrapper services for custom logic:

```typescript
// libs/api-client/src/funds.service.ts
import { CoreLedgerApiClient } from '../generated';

export class FundsService {
  constructor(private client: CoreLedgerApiClient) {}

  async getActiveFunds() {
    const funds = await this.client.api.v1.funds.get();
    return funds.items?.filter(f => f.status === 'Active') ?? [];
  }
}
```

### 4. Version Control

Commit the generated code to version control for these reasons:
- Teammates can build without running the API
- CI/CD can build without starting the API
- Provides a clear diff when API changes

Add to `.gitignore` only if you prefer generation on-demand.

## Advanced Configuration

### Custom Kiota Options

Edit `libs/api-client/project.json` to customize generation:

```json
{
  "targets": {
    "generate": {
      "command": "kiota generate -l typescript -o generated -d ../../apps/core-ledger-api/openapi.json -c CoreLedgerApiClient -n CoreLedger.ApiClient --clean-output --exclude-backward-compatible"
    }
  }
}
```

Common options:
- `-l typescript` - Target language
- `-o generated` - Output directory
- `-d path/to/spec.json` - OpenAPI spec path
- `-c CoreLedgerApiClient` - Client class name
- `-n CoreLedger.ApiClient` - Namespace
- `--clean-output` - Delete output dir before generation
- `--exclude-backward-compatible` - Use latest patterns only

### Multiple API Versions

To support multiple API versions:

```bash
# Generate for v1
kiota generate -l typescript -o generated/v1 -d openapi-v1.json

# Generate for v2
kiota generate -l typescript -o generated/v2 -d openapi-v2.json
```

## References

- [Kiota Documentation](https://learn.microsoft.com/en-us/openapi/kiota/overview)
- [Kiota TypeScript Samples](https://github.com/microsoft/kiota-samples/tree/main/get-started/quickstart/typescript)
- [OpenAPI Specification](https://swagger.io/specification/)
- [Core Ledger API README](../libs/api-client/README.md)

## Summary

1. **Install Kiota**: `dotnet tool install --global Microsoft.OpenApi.Kiota`
2. **Start API**: `nx serve core-ledger-api`
3. **Export Spec**: `npm run api:export-spec`
4. **Generate Client**: `npm run api:generate-client`
5. **Use in Angular**: Import from `@core-ledger/api-client`

The generated client provides full type safety and IntelliSense for all API operations!
