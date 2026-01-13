# Core Ledger API Client

Auto-generated TypeScript client for the Core Ledger API using [Microsoft Kiota](https://learn.microsoft.com/en-us/openapi/kiota/overview).

## Overview

This library contains TypeScript types and API client code generated from the Core Ledger OpenAPI specification. It provides type-safe access to all API endpoints.

## Generation Workflow

### Prerequisites

1. API must be running: `nx serve core-ledger-api`
2. Kiota CLI installed globally: `dotnet tool install --global Microsoft.OpenApi.Kiota`

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

### Basic Setup

```typescript
import { CoreLedgerApiClient } from '@core-ledger/api-client';
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';

// Create adapter with base URL
const adapter = new FetchRequestAdapter();
adapter.baseUrl = 'https://localhost:7109';

// Create client
const client = new CoreLedgerApiClient(adapter);
```

### With Authentication

```typescript
import { CoreLedgerApiClient } from '@core-ledger/api-client';
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';
import { AnonymousAuthenticationProvider } from '@microsoft/kiota-abstractions';
import { HttpClient } from '@microsoft/kiota-http-fetchlibrary';

// Create auth provider with bearer token
const authProvider = new AnonymousAuthenticationProvider();
const httpClient = HttpClient.create({
  middleware: [
    {
      intercept: async (url, init, next) => {
        // Add authorization header
        const token = 'your-jwt-token';
        init.headers = {
          ...init.headers,
          Authorization: `Bearer ${token}`,
        };
        return next(url, init);
      },
    },
  ],
});

const adapter = new FetchRequestAdapter(authProvider, undefined, undefined, httpClient);
adapter.baseUrl = 'https://localhost:7109';

const client = new CoreLedgerApiClient(adapter);
```

### Example Requests

```typescript
// Get all funds
const funds = await client.api.v1.funds.get();

// Get fund by ID
const fund = await client.api.v1.funds.byId(fundId).get();

// Create new fund
const newFund = await client.api.v1.funds.post({
  name: 'My Fund',
  // ... other properties
});

// Update fund
await client.api.v1.funds.byId(fundId).put({
  name: 'Updated Name',
  // ... other properties
});

// Delete fund
await client.api.v1.funds.byId(fundId).delete();
```

## Dependencies

This library requires the following Kiota runtime packages:

- `@microsoft/kiota-abstractions` - Core abstractions
- `@microsoft/kiota-http-fetchlibrary` - HTTP client using fetch
- `@microsoft/kiota-serialization-json` - JSON serialization
- `@microsoft/kiota-serialization-form` - Form serialization
- `@microsoft/kiota-serialization-text` - Text serialization
- `@microsoft/kiota-serialization-multipart` - Multipart serialization

## Structure

```
libs/api-client/
├── generated/          # Auto-generated client code (do not edit)
├── src/
│   └── index.ts       # Public exports
├── package.json       # Dependencies
├── project.json       # Nx configuration
├── tsconfig.json      # TypeScript configuration
└── README.md          # This file
```

## Integration with Angular

To use in Angular components:

```typescript
import { inject } from '@angular/core';
import { CoreLedgerApiClient } from '@core-ledger/api-client';

export class MyComponent {
  private readonly apiClient = inject(CoreLedgerApiClient);

  async loadFunds() {
    const funds = await this.apiClient.api.v1.funds.get();
    // ... use funds
  }
}
```

## Notes

- The `generated/` directory is auto-generated and should not be edited manually
- Always regenerate the client after API changes
- The client uses the fetch API for HTTP requests
- All generated types are strongly typed based on the OpenAPI spec
