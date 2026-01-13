# Calendario API Integration Status

## Summary

**Status:** ✅ Calendario API endpoints are fully available in the generated client, but the Angular service is still using the old HttpClient pattern.

## Current Implementation

### CalendarioService (`apps/core-ledger-ui/src/app/services/calendario.ts`)

**Status:** ❌ Using HttpClient directly (not using the generated API client)

**Current pattern:**
```typescript
@Injectable({ providedIn: 'root' })
export class CalendarioService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  getCalendarios(...): Observable<PaginatedResponse<Calendario>> {
    return this.http.get<PaginatedResponse<Calendario>>(`${this.apiUrl}/v1/calendario?...`);
  }

  getCalendarioById(id: number): Observable<Calendario> {
    return this.http.get<Calendario>(`${this.apiUrl}/v1/calendario/${id}`);
  }

  createCalendario(calendario: CreateCalendario): Observable<Calendario> {
    return this.http.post<Calendario>(`${this.apiUrl}/v1/calendario`, calendario);
  }

  updateCalendario(id: number, calendario: UpdateCalendario): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/v1/calendario/${id}`, calendario);
  }

  deleteCalendario(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/v1/calendario/${id}`);
  }
}
```

### CalendarioForm (`apps/core-ledger-ui/src/app/features/cadastro/calendario/calendario-form/calendario-form.ts`)

**Status:** ❌ Uses CalendarioService (which uses HttpClient)

**Current usage:**
```typescript
loadCalendario(id: number): void {
  this.calendarioService.getCalendarioById(id).subscribe({ ... });
}

onSubmit(): void {
  if (id) {
    this.calendarioService.updateCalendario(id, dto).subscribe({ ... });
  } else {
    this.calendarioService.createCalendario(dto).subscribe({ ... });
  }
}
```

### CalendarioStore (`apps/core-ledger-ui/src/app/features/cadastro/calendario/calendario-store.ts`)

**Status:** ℹ️ Does not make API calls directly (only manages state and filters)

## Available Generated API Endpoints

The Kiota-generated client includes full support for calendario endpoints:

### GET List (with query parameters)
- **Endpoint:** `/api/v1/calendario`
- **Query Parameters:**
  - `dataFim?: string`
  - `dataInicio?: string`
  - `diaUtil?: boolean`
  - `limit?: number`
  - `offset?: number`
  - `praca?: number`
  - `search?: string`
  - `sortBy?: string`
  - `sortDirection?: string`
  - `tipoDia?: number`

### GET by ID
- **Endpoint:** `/api/v1/calendario/{id}`

### POST Create
- **Endpoint:** `/api/v1/calendario`
- **Request Body:** `CreateCalendarioDto`
  ```typescript
  interface CreateCalendarioDto {
    data?: DateOnly | null;
    descricao?: string | null;
    praca?: number | null;
    tipoDia?: number | null;
  }
  ```

### PUT Update
- **Endpoint:** `/api/v1/calendario/{id}`
- **Request Body:** `UpdateCalendarioDto`
  ```typescript
  interface UpdateCalendarioDto {
    descricao?: string | null;
    tipoDia?: number | null;
  }
  ```

### DELETE
- **Endpoint:** `/api/v1/calendario/{id}`

### Additional Endpoints
- `calcularDMais` - Calculate D+ business days
- `diaUtil` - Check if date is business day
- `health` - Health check endpoint
- `importar` - Import calendar data
- `proximoDiaUtil` - Get next business day

## Migration Recommendation

### Option 1: Migrate CalendarioService to Use API Client

Update `CalendarioService` to use the generated API client:

```typescript
import { inject, Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';
import { ApiClientService } from '../api/api-client.service';
import { type CreateCalendarioDto, type UpdateCalendarioDto } from '@core-ledger/api-client';
import { Calendario, PaginatedResponse } from '../models/calendario.model';

@Injectable({ providedIn: 'root' })
export class CalendarioService {
  private readonly apiClient = inject(ApiClientService);

  readonly tipoDiaOptions = [ /* ... */ ];
  readonly pracaOptions = [ /* ... */ ];

  getCalendarios(
    limit: number = 100,
    offset: number = 0,
    sortBy?: string,
    sortDirection: 'asc' | 'desc' = 'desc',
    filters?: Record<string, string>
  ): Observable<PaginatedResponse<Calendario>> {
    const queryParams = {
      limit,
      offset,
      sortBy,
      sortDirection,
      ...filters,
    };

    return from(
      this.apiClient.calendario.get({ queryParameters: queryParams })
    );
  }

  getCalendarioById(id: number): Observable<Calendario> {
    return from(this.apiClient.calendario.byId(id).get());
  }

  createCalendario(calendario: CreateCalendarioDto): Observable<Calendario> {
    return from(this.apiClient.calendario.post(calendario));
  }

  updateCalendario(id: number, calendario: UpdateCalendarioDto): Observable<void> {
    return from(this.apiClient.calendario.byId(id).put(calendario));
  }

  deleteCalendario(id: number): Observable<void> {
    return from(this.apiClient.calendario.byId(id).delete());
  }
}
```

### Option 2: Use API Client Directly in Components

For simpler scenarios, inject `ApiClientService` directly in components:

```typescript
export class CalendarioForm {
  private readonly apiClient = inject(ApiClientService);

  async loadCalendario(id: number) {
    try {
      const calendario = await this.apiClient.calendario.byId(id).get();
      // Use calendario data
    } catch (error) {
      this.toastService.error('Failed to load calendario');
    }
  }

  async onSubmit() {
    const dto = this.buildDto();

    try {
      if (this.calendarioId()) {
        await this.apiClient.calendario.byId(id).put(dto);
        this.toastService.success('Updated successfully');
      } else {
        await this.apiClient.calendario.post(dto);
        this.toastService.success('Created successfully');
      }
      this.router.navigate(['/cadastro/calendario']);
    } catch (error) {
      this.toastService.error('Operation failed');
    }
  }
}
```

## Benefits of Migration

1. **Type Safety** - Full TypeScript types for all requests/responses
2. **Auto-Generated** - Always in sync with .NET API
3. **Consistency** - Same pattern as other migrated services (FundoWizardService)
4. **IntelliSense** - Better IDE support with autocomplete
5. **Mock API Compatible** - Works with existing mock system

## Known Issues

⚠️ **Response Type Issue:** The generated endpoints return `ArrayBuffer` instead of typed objects. This may be an OpenAPI schema issue that needs to be fixed in the .NET API.

Expected:
```typescript
get(): Promise<CalendarioDto | undefined>
```

Current:
```typescript
get(): Promise<ArrayBuffer | undefined>
```

**Recommendation:** Verify the .NET API's OpenAPI spec generation for calendario endpoints to ensure proper response types are specified.

## Action Items

- [ ] Fix OpenAPI response type generation in .NET API (if needed)
- [ ] Regenerate API client: `npm run api:generate-client`
- [ ] Migrate CalendarioService to use ApiClientService
- [ ] Update CalendarioForm to use migrated service
- [ ] Test calendario CRUD operations with generated client
- [ ] Update mock API to handle calendario endpoints (if needed)
- [ ] Update documentation with new patterns

## Related Files

- **Service:** `apps/core-ledger-ui/src/app/services/calendario.ts`
- **Form:** `apps/core-ledger-ui/src/app/features/cadastro/calendario/calendario-form/calendario-form.ts`
- **Store:** `apps/core-ledger-ui/src/app/features/cadastro/calendario/calendario-store.ts`
- **Models:** `apps/core-ledger-ui/src/app/models/calendario.model.ts`
- **Generated Client:** `libs/api-client/generated/api/v1/calendario/`
- **API Client Service:** `apps/core-ledger-ui/src/app/api/api-client.service.ts`

## See Also

- [Angular API Client Usage Guide](./angular-api-client-usage.md)
- [API Client Generation Guide](./api-client-generation.md)
- Example migration: `apps/core-ledger-ui/src/app/services/fundo-wizard.service.ts`
