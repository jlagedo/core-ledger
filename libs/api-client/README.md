# Core Ledger API Client

Auto-generated TypeScript client for the Core Ledger API using [NSwag](https://github.com/RicoSuter/NSwag).

## Overview

This library contains TypeScript types and API client code generated from the Core Ledger OpenAPI specification. It provides type-safe access to all API endpoints with native Angular HttpClient integration.

## Generation Workflow

### Prerequisites

1. API must be running: `nx serve core-ledger-api`
2. NSwag CLI (installed via npm): `npx nswag`

### Generate Client

```bash
# Option 1: Generate client (will export OpenAPI spec and generate)
nx run api-client:generate

# Option 2: Manual steps
# Step 1: Export OpenAPI spec from running API
nx run core-ledger-api:export-openapi

# Step 2: Generate TypeScript client
nx run api-client:generate
```

## Usage

### Angular Provider Setup

In your `app.config.ts`:

```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { API_BASE_URL } from '@core-ledger/api-client';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideHttpClient(withInterceptorsFromDi()),
    { provide: API_BASE_URL, useValue: 'https://localhost:7109' },
  ],
};
```

### Using in Services

```typescript
import { inject, Injectable } from '@angular/core';
import { FundosClient, FundoDto } from '@core-ledger/api-client';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class FundService {
  private readonly fundosClient = inject(FundosClient);

  getAll(): Observable<FundoDto[]> {
    return this.fundosClient.getAll();
  }

  getById(id: string): Observable<FundoDto> {
    return this.fundosClient.getById(id);
  }

  create(dto: CreateFundoDto): Observable<FundoDto> {
    return this.fundosClient.create(dto);
  }
}
```

### Using in Components

```typescript
import { Component, inject, signal } from '@angular/core';
import { FundosClient, FundoDto } from '@core-ledger/api-client';

@Component({
  selector: 'app-fund-list',
  template: `
    @for (fund of funds(); track fund.id) {
      <div>{{ fund.nome }}</div>
    }
  `,
})
export class FundListComponent {
  private readonly fundosClient = inject(FundosClient);
  funds = signal<FundoDto[]>([]);

  ngOnInit() {
    this.fundosClient.getAll().subscribe((data) => this.funds.set(data));
  }
}
```

## Available Clients

The following clients are auto-generated:

- `FundosClient` - Fund management
- `CalendárioClient` - Calendar/business days
- `IndexadoresClient` - Financial indices
- `HistoricosIndexadoresClient` - Index history
- `UsersClient` - User management
- `AccountsClient` - Chart of accounts
- `SecuritiesClient` - Financial instruments
- `TransactionsClient` - Transaction management
- `ANBIMA_ClassificationsClient` - Fund classifications

## Structure

```
libs/api-client/
├── generated/          # Auto-generated client code (do not edit)
│   └── api-clients.ts  # All clients and DTOs
├── src/
│   └── index.ts        # Public exports
├── package.json        # Dependencies
├── project.json        # Nx configuration
├── tsconfig.json       # TypeScript configuration
└── README.md           # This file
```

## NSwag Configuration

Configuration file: `apps/core-ledger-api/nswag.json`

Key settings:
- **Template**: Angular (HttpClient with RxJS Observables)
- **Type Style**: Interface (not classes)
- **Client Mode**: Multiple clients from first tag and operation ID
- **Output**: Single file with all clients and DTOs

## Notes

- The `generated/` directory is auto-generated and should not be edited manually
- Always regenerate the client after API changes
- Uses Angular HttpClient with RxJS Observables
- All generated types are strongly typed based on the OpenAPI spec
- For complete documentation, see `docs/api-client-generation.md`
