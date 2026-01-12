
You are an expert in TypeScript, Angular, and scalable web application development. You write functional, maintainable, performant, and accessible code following Angular and TypeScript best practices.

## Project Context

Core Ledger UI is a fund accounting ABOR (Accounting Book of Records) application built with Angular 21. It provides comprehensive financial management features including fund management, portfolio tracking, securities management, transactions, NAV calculation, chart of accounts, journal entries, posting periods, and financial reporting.

## Development Commands

- `npm start` - Start dev server with **mock authentication** (default, no Auth0 needed for local development)
- `npm run start:auth` - Start with real Auth0 authentication
- `npm test` - Run Vitest tests (not Jasmine/Karma)
- `npm run watch` - Watch mode with auto-rebuild
- Development server: http://localhost:4200/ (proxies `/api/**` to backend: https://localhost:7109)

**Note:** The proxy configuration in `src/proxy.conf.json` automatically forwards all `/api/**` requests to the backend. Always use `/api` as the base path, never hardcode backend URLs.

## TypeScript Best Practices

- Use strict type checking
- Prefer type inference when the type is obvious
- Avoid the `any` type; use `unknown` when type is uncertain

## Angular Best Practices

- Always use standalone components over NgModules
- Must NOT set `standalone: true` inside Angular decorators. It's the default in Angular v20+.
- Use signals for state management
- Implement lazy loading for feature routes
- Do NOT use the `@HostBinding` and `@HostListener` decorators. Put host bindings inside the `host` object of the `@Component` or `@Directive` decorator instead
- Use `NgOptimizedImage` for all static images.
  - `NgOptimizedImage` does not work for inline base64 images.

## Accessibility Requirements

- It MUST pass all AXE checks.
- It MUST follow all WCAG AA minimums, including focus management, color contrast, and ARIA attributes.

## Components

- Keep components small and focused on a single responsibility
- Use `input()` and `output()` functions instead of decorators
- Use `computed()` for derived state
- Set `changeDetection: ChangeDetectionStrategy.OnPush` in `@Component` decorator
- Prefer inline templates for small components
- Prefer Reactive forms instead of Template-driven ones
- Do NOT use `ngClass`, use `class` bindings instead
- Do NOT use `ngStyle`, use `style` bindings instead
- When using external templates/styles, use paths relative to the component TS file.
- Always use Bootstrap 5 utility classes (e.g., `text-primary`, `btn-primary`, `bg-secondary`) - **NEVER** inline styles with custom colors

## State Management

- Use signals for local component state: `signal()`, `computed()`, `toSignal()`
- Services expose read-only signals via `.asReadonly()` (e.g., `ThemeService`, `ToastService`)
- Use `@ngrx/signals` for feature stores (see `src/app/shared/stores/paginated-search-store.ts`)
- Do NOT use `mutate` on signals, use `update` or `set` instead
- Keep state transformations pure and predictable

## Routing Architecture

- **Two-level routing structure**:
  1. Top-level routes in `src/app/app.routes.ts` use `loadChildren` for feature modules
  2. Feature routes (e.g., `funds.routes.ts`) define child routes using exported constants (e.g., `FUNDS_ROUTES`)
- All routes use `loadComponent` for components or `loadChildren` for feature modules
- Routes include `breadcrumb` in route data for automatic breadcrumb generation
- Routes protected with `authGuard` (except `/login`)

## Authentication

- Uses Auth0 OIDC via `angular-auth-oidc-client`
- **Mock authentication available** for development: set `environment.auth.useMock = true` (default in `environment.local-noauth.ts`)
- **Two authentication modes** configured in `app.config.ts`:
  - Mock mode: uses `MockAuthService` and `mockAuthInterceptor`
  - Real Auth0: uses `OidcSecurityService` and `authInterceptor()`
- `AuthService` uses `toSignal()` to convert OIDC observables to signals
- `UserService` fetches user data from backend on login (via reactive `effect()`)
- Mock users: admin, fund-manager, trader, analyst (see `src/app/auth/mock-users.ts`)
- All routes protected with `authGuard` (except `/login`)

## Services Pattern

```typescript
@Injectable({ providedIn: 'root' })
export class MyService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL); // Use API_URL token, not hardcoded URLs
}
```

- Always use `inject()` function instead of constructor injection
- Use `providedIn: 'root'` for singleton services
- API calls return Observables with typed interfaces from `src/app/models/`
- All models use kebab-case naming (e.g., `account-type.model.ts`)

## Mock API System (REQUIRED)

**IMPORTANT**: All API implementations MUST include corresponding mock data to support offline development.

### Requirements

When implementing any new API endpoint or entity:

1. **Create Mock Data File** in `src/app/api/mock-data/`:
   - Follow naming convention: `<entity-name>.mock.ts`
   - Export as `MOCK_<ENTITY_NAME>` constant
   - Include realistic data with edge cases (special characters, nulls, long text, etc.)
   - Add JSDoc comment with `@internal` tag

2. **Update Mock Data Index**:
   - Add export to `src/app/api/mock-data/index.ts`
   - Ensure barrel export follows alphabetical order

3. **Update MockApiService**:
   - Add Map storage for the new entity in `src/app/api/mock-api.service.ts`
   - Update `getDataMapForUrl()` method with URL pattern
   - Update `getNextIdForUrl()` for auto-increment IDs
   - Update `reset()` method to include new entity

4. **Create Production Stub**:
   - Add corresponding export to `src/app/api/mock-data.production.ts`
   - Use `createProductionProxy()` helper to ensure safety

5. **Write Tests**:
   - Test CRUD operations in `mock-api.service.spec.ts`
   - Test interceptor routing in `mock-api.interceptor.spec.ts`

### Mock Data Best Practices

- **Realistic Data**: Use actual formats, realistic values, and production-like data
- **Edge Cases**: Include special characters, empty strings, nulls, maximum lengths
- **Variety**: Multiple records with different statuses, types, and values
- **Relationships**: Ensure foreign keys match related entities (e.g., fundId references actual fund)
- **Timestamps**: Use ISO 8601 format (`2024-12-01T14:30:00Z`)
- **Comments**: Document unusual or edge-case data with comments

### Example Mock Data File

```typescript
import { Entity } from '../../models/entity.model';

/**
 * Mock entity data for local development and testing.
 * Includes edge cases: special characters, nulls, various statuses.
 * @internal
 */
export const MOCK_ENTITIES: Entity[] = [
  {
    id: 1,
    name: 'Standard Entity',
    status: EntityStatus.Active,
    createdAt: '2020-01-15T10:00:00Z',
    updatedAt: '2024-12-01T14:30:00Z',
  },
  {
    id: 2,
    name: 'Entity with Special Chars: €$£¥ & "Quotes"',
    status: EntityStatus.Active,
    createdAt: '2023-06-15T12:00:00Z',
    updatedAt: '2024-06-15T12:00:00Z',
  },
  {
    id: 3,
    name: 'Very Long Name That Tests UI Constraints And Demonstrates How System Handles Extended Text',
    status: EntityStatus.Inactive,
    createdAt: '2024-01-01T00:00:00Z',
  },
];
```

### Mock API Configuration

Environment files control mock API behavior:

```typescript
api: {
  useMock: true,          // Enable mock API (false in production)
  mockDelayMs: 300,       // Network latency simulation
  mockErrorRate: 0,       // Random error rate (0-1, 0.1 = 10%)
}
```

### Testing with Mock Data

```typescript
// In component tests
TestBed.configureTestingModule({
  providers: [
    provideHttpClient(withInterceptors([mockApiInterceptor])),
    MockApiService,
    { provide: ENVIRONMENT, useValue: { api: { useMock: true } } },
  ],
});
```

**See `documentation/mock-api.md` for complete documentation.**

## Feature Module Structure

Features in `src/app/features/<feature-name>/` follow this pattern:

```
features/<feature>/
├── <feature>.routes.ts       # Exports <FEATURE>_ROUTES constant
├── <feature>-store.ts         # Feature-specific @ngrx/signals store
├── <feature>-list.ts          # List view (uses paginated-search-store)
├── <feature>-details.ts       # Detail view
└── <feature>-form/
    └── <feature>-form.ts      # Form component
```

## Configuration & Environment

- Environment files in `src/environments/` define configuration per deployment target
- Use `ENVIRONMENT` injection token to access environment config (never import directly)
- Use `API_URL` injection token for backend URL (from `src/app/config/api.config.ts`)
- Environment includes: API URL, log level, Sentry config, Auth0 config, toast delays, pagination defaults, storage keys, and feature flags
- Mock authentication flag (`environment.auth.useMock`) switches between mock and real Auth0

## Testing

- Uses Vitest (not Jasmine/Karma)
- Test files: `*.spec.ts`
- Mock utilities: `src/app/testing/test-helpers.ts`, `InMemoryStorageService`
- Use `provideTestDependencies()` helper to provide common test providers (router, HTTP, OIDC)

## Templates

- Keep templates simple and avoid complex logic
- Use native control flow (`@if`, `@for`, `@switch`) instead of `*ngIf`, `*ngFor`, `*ngSwitch`
- Use the async pipe to handle observables
- Do not assume globals like (`new Date()`) are available.
- Do not write arrow functions in templates (they are not supported).

## Logging & Error Handling

- Use `LoggerService` for structured logging with context (debug/info/warn/error)
- Automatic Sentry integration for error tracking
- Use `ToastService` for user-facing notifications (success/error/warning/info)
- Toast delays configured per type in environment config
