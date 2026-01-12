# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Core Ledger UI is a fund accounting ABOR (Accounting Book of Records) application built with Angular 21. It provides comprehensive financial management features including fund management, portfolio tracking, securities management, transactions, NAV calculation, chart of accounts, journal entries, posting periods, and financial reporting.

The application uses modern Angular patterns with standalone components, signals for state management, OnPush change detection, and Auth0 OIDC authentication.

## Development Commands

### Development Server

The application supports multiple development modes:

**Default Mode (No Authentication):**
```bash
ng serve
# or
npm start
```
Uses `local-noauth` configuration with mock authentication and mock API. Ideal for offline development.

**Authenticated Mode (Auth0):**
```bash
npm run start:auth
```
Uses `local-auth` configuration with real Auth0 OIDC authentication and mock API. Requires Auth0 credentials in environment file.

**Watch Modes:**
```bash
npm run watch          # No authentication with auto-rebuild
npm run watch:auth     # Auth0 authentication with auto-rebuild
```

**Mock Token Generation:**
```bash
npm run generate-tokens
```
Generates JWT tokens for mock authentication development.

Application runs at http://localhost:4200/

### Testing
```bash
ng test      # Run all tests with Vitest
npm test     # Same as above
```

### Building
```bash
ng build                              # Development build
ng build --configuration production   # Production build (outputs to dist/)
npm run watch                         # Watch mode (auto-rebuild on changes)
```

### Code Generation
```bash
ng generate component features/my-feature/my-component
ng generate service services/my-service
ng generate directive directives/my-directive
```

## Architecture

### Routing & Lazy Loading

The application uses a two-level routing structure:
1. **Top-level routes** in `src/app/app.routes.ts` define feature modules with lazy loading
2. **Feature-level routes** (e.g., `funds.routes.ts`) define child routes within features

All routes use `loadComponent` for individual components or `loadChildren` for feature modules with child routes. This ensures optimal bundle splitting and performance.

Route data includes `breadcrumb` property used by `BreadCrumbService` to generate navigation breadcrumbs automatically.

**Authentication Guards:**
Routes are protected using a custom `authGuard` function (see `src/app/auth/auth.guard.ts`). This guard supports both real OIDC authentication and mock authentication modes. The `/login` route is public; all other routes require authentication.

### Authentication

The application uses **Auth0 OIDC** authentication via `angular-auth-oidc-client`:

- **AuthService** (`src/app/auth/auth-service.ts`): Main authentication service
  - Uses `toSignal()` to convert OIDC observables to signals
  - Exposes `isLoggedIn` and `userProfile` computed signals
  - Uses `effect()` to reactively fetch user data from backend on login
  - Provides `login()` and `logout()` methods
- **Auth Configuration**: `src/app/auth/auth.config.ts` contains OIDC settings
- **Auth Guard**: `src/app/auth/auth.guard.ts` for custom route protection
- **UserService** (`src/app/services/user-service.ts`): Manages user data from backend database

### State Management

The application uses Angular signals for state management (@ngrx/signals):

**Service-Level State (Signal Pattern):**
- Services expose read-only signals via `.asReadonly()`
- Private signals prefixed with `_` (e.g., `_currentTheme`)
- Use `signal()`, `computed()`, and `toSignal()` for reactive state
- Example: `ThemeService` (src/app/services/theme-service.ts), `ToastService` (src/app/services/toast-service.ts)

**Store Pattern (@ngrx/signals):**
- Feature stores use `signalStore` from `@ngrx/signals`
- Reusable store factories for common patterns (e.g., `createPaginatedSearchStore`)
- Example: `src/app/shared/stores/paginated-search-store.ts` - creates stores with search/sort/pagination state and session storage persistence

### API Integration

API calls are proxied through `src/proxy.conf.json` which forwards `/api/**` to `https://localhost:7109`.

Services follow this pattern:
- Inject `API_URL` token from `src/app/config/api.config.ts` (default: `/api`)
- Inject `HttpClient` using `inject()` function
- Return Observables for async operations
- Use typed interfaces from `src/app/models/` for request/response
- Example: see `FundService` (src/app/services/fund.ts)

### Services

Global singleton services use `providedIn: 'root'` and the `inject()` function instead of constructor injection:

```typescript
@Injectable({ providedIn: 'root' })
export class MyService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);
}
```

Key services in `src/app/services/`:
- **ToastService**: Global toast notifications (success/warning/error/info) using signals
- **BreadCrumbService**: Automatic breadcrumb generation from route data
- **ThemeService**: Dark/light theme management with localStorage persistence
- **UserService**: User data management and API integration
- **MenuService**: Navigation menu state management
- **LoggerService**: Structured logging with context
- **FundService**: Fund entity API integration
- **AccountService**: Chart of accounts API integration
- **SecurityService**: Securities/instruments API integration

### Mock API System (REQUIRED)

**IMPORTANT**: All API implementations MUST include corresponding mock data to support offline development.

#### Requirements for New API Endpoints

When implementing any new API endpoint or entity:

1. **Create Mock Data File** in `src/app/api/mock-data/`:
   - Naming: `<entity-name>.mock.ts`
   - Export as `MOCK_<ENTITY_NAME>` constant
   - Include realistic data with edge cases (special characters, nulls, long text)
   - Add JSDoc with `@internal` tag

2. **Update Mock Data Index**: Add export to `src/app/api/mock-data/index.ts`

3. **Update MockApiService** in `src/app/api/mock-api.service.ts`:
   - Add Map storage for entity
   - Update `getDataMapForUrl()` with URL pattern
   - Update `getNextIdForUrl()` for auto-increment IDs
   - Update `reset()` method

4. **Create Production Stub**: Add export to `src/app/api/mock-data.production.ts` using `createProductionProxy()`

5. **Write Tests**: Test CRUD operations and interceptor routing

#### Mock Data Best Practices

- **Realistic Data**: Use actual formats, production-like values
- **Edge Cases**: Special characters, empty strings, nulls, max lengths
- **Variety**: Multiple records with different statuses/types
- **Relationships**: Foreign keys must reference actual entities
- **Timestamps**: ISO 8601 format (`2024-12-01T14:30:00Z`)
- **Comments**: Document unusual/edge-case data

#### Example Mock Data

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
  },
];
```

**See `documentation/mock-api.md` for complete documentation.**

### Mock Authentication System (REQUIRED)

**IMPORTANT**: The application includes a complete mock authentication system to support offline development without Auth0 dependencies.

#### Mock Authentication Components

1. **MockAuthService** (`src/app/auth/mock-auth-service.ts`):
   - Simulates `OidcSecurityService` behavior
   - Provides login/logout functionality with mock users
   - Returns realistic authentication state observables

2. **Mock Users** (`src/app/auth/mock-users.ts`):
   - Predefined users with different roles (admin, fund-manager, trader, analyst)
   - Realistic user profiles with claims
   - Used for role-based access testing

3. **Mock Tokens** (`src/app/auth/mock-tokens.ts`):
   - JWT token generation for mock authentication
   - Includes realistic claims and expiration
   - Production-safe with `mock-tokens.production.ts` stub

4. **Mock Auth Interceptor** (`src/app/auth/mock-auth-interceptor.ts`):
   - Adds Authorization headers to requests
   - Simulates token refresh behavior

#### Environment Switching

The application automatically uses mock or real authentication based on environment configuration:

- **`environment.local-noauth.ts`**: Uses `MockAuthService` (default for `npm start`)
- **`environment.local-auth.ts`**: Uses real Auth0 via `angular-auth-oidc-client` (`npm run start:auth`)
- **`environment.development.ts`**: Configurable
- **`environment.production.ts`**: Always uses real Auth0

#### Production Safety

Mock authentication files are replaced during production builds:
- `mock-tokens.ts` → `mock-tokens.production.ts` (throws error if accessed)
- Prevents accidental use of mock authentication in production

#### Usage in AuthService

The `AuthService` abstracts authentication and works with both real and mock providers:

```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oidcSecurityService = inject(OidcSecurityService);
  // OidcSecurityService is either real or MockAuthService based on environment
}
```

### Layout Components

Core layout components in `src/app/layout/`:
- **sidenav**: Main navigation sidebar with collapsible menu
- **breadcrumb**: Breadcrumb navigation (automatically generated from route data)
- **page-header**: Consistent page header across features
- **user-profile**: User profile display and menu
- **toasts-container**: Global toast notification display

### Shared Utilities

**Storage Abstraction** (`src/app/shared/storage/`):
- `StorageService` interface: Abstraction over browser storage APIs
- `SessionStorageService`: Session storage implementation
- Used for testing and flexibility (allows mock implementations)

**Reusable Stores** (`src/app/shared/stores/`):
- `createPaginatedSearchStore()`: Factory function for creating stores with search, pagination, and sorting
- Includes automatic session storage persistence
- Used by list views (e.g., fund list, account list)

**Testing Utilities** (`src/app/testing/`):
- `InMemoryStorageService`: Mock storage for tests
- `test-helpers.ts`: Shared test utilities and helpers

### Features Structure

Features follow a consistent pattern:
- Each feature module in `src/app/features/<feature-name>/`
- Child routes defined in `<feature-name>.routes.ts`
- Components are standalone (no NgModules)
- Feature exports named constant for routes (e.g., `FUNDS_ROUTES`)

Example feature structure (Funds):
```
features/funds/
├── funds.routes.ts       # Child routes
├── funds.ts              # Feature shell component (if needed)
├── funds-store.ts        # Feature-specific store
├── fund-list.ts          # List view
├── fund-details.ts       # Detail view
├── fund-form/
│   └── fund-form.ts      # Form component
└── share-classes.ts      # Related feature
```

**Available Features:**
- `admin`: Administrative functions
- `administration`: System administration
- `balances-reports`: Account balances and reporting
- `chart-of-accounts`: Chart of accounts management
- `dashboard`: Main dashboard and overview
- `funds`: Fund management and share classes
- `journal-entries`: Journal entry creation and management
- `login`: Authentication and login
- `nav`: Net Asset Value (NAV) calculation
- `portfolio`: Portfolio holdings and positions
- `posting-periods`: Accounting period management
- `pricing-valuation`: Securities pricing and valuation
- `profile`: User profile management
- `reports`: Financial reporting
- `transactions`: Transaction management

### Models

TypeScript interfaces and types in `src/app/models/` (kebab-case naming):
- Domain models: `fund.model.ts`, `account.model.ts`, `security.model.ts`, `user.model.ts`
- Enum-like models: `account-type.model.ts`, `security-type.model.ts`
- Request/response DTOs: `import-b3-response.model.ts`
- Shared types: `menu-item.model.ts`

**Naming Convention:** Use kebab-case for model files (e.g., `account-type.model.ts`, not `account_type.model.ts`)

### Directives

Custom directives in `src/app/directives/` (kebab-case naming):
- **sortable.directive.ts**: Table column sorting functionality
- **only-numbers.directive.ts**: Input restriction to numeric values only

**Naming Convention:** Use kebab-case for directive files (e.g., `only-numbers.directive.ts`)

## Technology Stack

### Core Framework
- **Angular 21.0** with TypeScript 5.9
- **Standalone Components**: No NgModules (default in Angular 21)
- **@angular/build**: Application builder with Vite
- **RxJS 7.8**: Reactive programming

### UI & Styling
- **Bootstrap 5.3.8**: UI component framework
- **@ng-bootstrap/ng-bootstrap 20.0**: Angular Bootstrap components
- **Bootstrap Icons 1.13.1**: Icon library
- **SCSS**: Stylesheet preprocessor

### State Management & Data
- **@ngrx/signals 21.0**: Signal-based state management
- **angular-auth-oidc-client 21.0**: Auth0 OIDC authentication
- **ngx-echarts 21.0 / echarts 6.0**: Interactive charts and visualizations

### Error Monitoring & Logging
- **@micro-sentry/angular 7.2.0**: Lightweight Sentry integration for error tracking
- Configured per environment (development/production)
- See `app.config.ts` for setup with `provideMicroSentry()`

### Development Tools
- **knip 5.78**: Unused code detection
- **source-map-explorer 2.5.3**: Bundle analysis
- **tsx 4.21.0 / ts-node 10.9.2**: TypeScript execution for scripts
- **Vitest 4.0.8**: Unit testing framework
- **@vitest/coverage-v8 4.0.16**: Code coverage
- **jsdom 27.1.0**: DOM implementation for tests
- **@angular/cli 21.0.4**: Angular CLI
- **npm 11.6.2**: Package manager
- **Prettier**: Code formatting (printWidth: 100, singleQuote: true)

### Build Configuration
- **Bundle size budgets**:
  - Initial bundle: 500kB warning, 1MB error
  - Component styles: 4kB warning, 8kB error
- **Development server**: Proxies `/api/**` to `https://localhost:7109`

## Code Standards & Conventions

### Angular & TypeScript Requirements

**DO:**
- Use standalone components (default in Angular 20+, don't set `standalone: true`)
- Use signals for state management (`signal()`, `computed()`, `toSignal()`)
- Use `inject()` function instead of constructor injection
- Use `input()` and `output()` functions instead of decorators
- Set `changeDetection: ChangeDetectionStrategy.OnPush` in components
- Use native control flow: `@if`, `@for`, `@switch` (not `*ngIf`, `*ngFor`, etc.)
- Use `class` and `style` bindings instead of `ngClass`/`ngStyle`
- Use Reactive Forms (not Template-driven)
- Put host bindings in the `host` object of `@Component`/`@Directive` decorator
- Use strict TypeScript (`strict: true` enabled)
- Prefer type inference when obvious; avoid `any` (use `unknown` if type uncertain)

**DON'T:**
- Use `@HostBinding` or `@HostListener` decorators
- Use `mutate` on signals (use `update` or `set` instead)
- Write arrow functions in templates (not supported)
- Assume globals like `new Date()` are available in templates

### Styling

- SCSS is the stylesheet format (configured in `angular.json`)
- Use Bootstrap 5 utility classes where possible
- Avoid inline styles (use rem units if inline styles are absolutely necessary)
- Prettier formatting: printWidth 100, singleQuote true

### Bootstrap 5 Best Practices

The application uses **Bootstrap 5.3.8** imported from source in `styles.scss`. Follow these conventions:

**Core Principles:**
1. **Utility-First**: Prefer Bootstrap utility classes over custom CSS
2. **Responsive Design**: Use responsive breakpoints (sm, md, lg, xl, xxl)
3. **Spacing System**: Use consistent spacing scale (0-5, auto)
4. **Grid System**: Leverage row/col with gutters (g-*)
5. **Theme Colors**: Use semantic colors (primary, secondary, success, info, warning, danger, light, dark)

**Grid & Layout:**
```html
<!-- Use grid gutters instead of manual margins -->
<form class="row g-3" novalidate>
  <div class="col-md-6">...</div>
  <div class="col-md-6">...</div>
</form>
```

**Container Usage Standards:**
The application follows a consistent container strategy based on page type:

1. **List/Index Pages** → `container-lg px-3 px-md-4`
   - Examples: fund-list, chart-of-accounts, transaction-list, security-list
   - Purpose: Balanced max-width on large screens, readable content width

2. **Form Pages** → `container mt-3`
   - Examples: fund-form, account-form, security-form, transaction-form
   - Purpose: Narrower width for focused form entry
   - Forms typically use `col-lg-8` or `col-lg-10` centered cards

3. **Dashboard Pages** → `container-fluid`
   - Examples: dashboard, posting-periods, journal-entries, administration
   - Purpose: Full-width layouts for data visualization and charts

```html
<!-- List view example -->
<main class="container-lg px-3 px-md-4">
  <div class="card">...</div>
</main>

<!-- Form view example -->
<div class="container mt-3">
  <div class="card col-lg-8 mx-auto">...</div>
</div>

<!-- Dashboard example -->
<main class="container-fluid px-3 pb-4">
  <div class="row g-4">...</div>
</main>
```

**Spacing Utilities:**
- Margin: `m-*`, `mt-*`, `mb-*`, `ms-*`, `me-*`, `mx-*`, `my-*`
- Padding: `p-*`, `pt-*`, `pb-*`, `ps-*`, `pe-*`, `px-*`, `py-*`
- Gap: `gap-*`, `row-gap-*`, `column-gap-*`
- Scale: 0 (0rem), 1 (0.25rem), 2 (0.5rem), 3 (1rem), 4 (1.5rem), 5 (3rem), auto

**Button Sizing Standards:**
- **Regular size (default)**: Primary actions (Submit, Create New, Save)
- **Small size (`btn-sm`)**: Secondary actions in headers (Help, Info)
- **Button groups (`btn-group-sm`)**: Table row actions, compact toolbars

**Common Component Patterns:**

*Cards:*

**Standard Card Pattern for List Views:**
The application uses a consistent card styling pattern across list views. Use this exact combination for consistency:

```html
<!-- List view card (data grids, tables) -->
<div class="card bg-body-tertiary border-0 shadow-sm rounded-4 border-top bg-opacity-50">
  <div class="card-body p-3 p-md-4">
    <app-data-grid />
  </div>
</div>
```

**Explanation of classes:**
- `bg-body-tertiary` - Themed background color (adapts to light/dark mode)
- `border-0` - Remove default card border
- `shadow-sm` - Subtle shadow for depth
- `rounded-4` - Larger border radius (0.5rem)
- `border-top` - Top border for visual separation
- `bg-opacity-50` - 50% background opacity for glass effect

**Form Card Pattern:**
Forms use a centered card with header:

```html
<div class="card col-lg-8 mx-auto bg-body-tertiary">
  <div class="card-header">
    <h5 class="card-title">Create Fund</h5>
  </div>
  <div class="card-body">
    <form class="row g-3">...</form>
  </div>
</div>
```

**Dashboard Card Pattern:**
Dashboard cards use responsive grid and equal heights:

```html
<div class="col-xl-4 col-lg-6">
  <div class="card bg-body-tertiary shadow-sm border-0 rounded-4 h-100">
    <div class="card-body p-4">
      <h6 class="text-uppercase text-secondary fw-semibold mb-3 fs-6">Title</h6>
      <!-- Content -->
    </div>
  </div>
</div>
```

*Forms:*
```html
<!-- Form with validation -->
<form class="row g-3" [formGroup]="myForm" (ngSubmit)="onSubmit()">
  <div class="col-md-6">
    <label for="field" class="form-label">Field Label</label>
    <input type="text" class="form-control" [class.is-invalid]="isInvalid('field')" id="field">
    <div class="invalid-feedback">Error message</div>
  </div>
</form>
```

*Buttons:*
```html
<!-- Button variants -->
<button class="btn btn-primary">Primary Action</button>
<button class="btn btn-secondary">Secondary Action</button>
<button class="btn btn-outline-secondary">Tertiary Action</button>

<!-- Button groups for table actions -->
<div class="btn-group btn-group-sm" role="group">
  <button class="btn btn-secondary"><i class="bi bi-pencil"></i></button>
  <button class="btn btn-secondary dropdown-toggle" ngbDropdownToggle></button>
</div>
```

*Alerts:*
```html
<!-- Alert with icon -->
<div class="alert alert-success d-flex align-items-center" role="alert">
  <i class="bi bi-check-circle-fill fs-4 me-2" aria-hidden="true"></i>
  <div><strong>Success!</strong> Operation completed.</div>
</div>
```

**Flexbox Utilities:**
- Display: `d-flex`, `d-inline-flex`, `d-grid`
- Direction: `flex-row`, `flex-column`
- Justify: `justify-content-start`, `justify-content-center`, `justify-content-end`, `justify-content-between`
- Align: `align-items-start`, `align-items-center`, `align-items-end`, `align-items-stretch`
- Grow/Shrink: `flex-grow-1`, `flex-shrink-0`, `flex-fill`

**Responsive Utilities:**
```html
<!-- Hide/show based on breakpoint -->
<div class="d-none d-md-block">Hidden on mobile, visible on tablet+</div>

<!-- Responsive spacing -->
<div class="px-3 px-md-4">Padding adjusts with screen size</div>

<!-- Responsive grid -->
<div class="col-12 col-md-6 col-lg-4">Responsive column</div>
```

**Text Utilities:**
- Color: `text-primary`, `text-secondary`, `text-muted`, `text-danger`, `text-success`
- Alignment: `text-start`, `text-center`, `text-end`
- Transform: `text-uppercase`, `text-lowercase`, `text-capitalize`
- Weight: `fw-bold`, `fw-semibold`, `fw-normal`, `fw-light`
- Size: `fs-1` through `fs-6`, `display-1` through `display-6`

**Avoid:**
- Inline styles (use rem units like `style="max-width: 25rem;"` if absolutely necessary)
- Hardcoded pixel values (prefer Bootstrap's spacing scale)
- Custom CSS for what Bootstrap utilities already provide
- Repetitive class combinations (consider creating semantic classes)

### Accessibility

- All changes must pass AXE checks
- Follow WCAG AA minimums (focus management, color contrast, ARIA attributes)

### File Organization

**Directory Structure:**
```
src/app/
├── auth/                    # Authentication module (OIDC)
├── config/                  # Configuration (API URL, etc.)
├── directives/              # Shared directives (kebab-case.directive.ts)
├── features/                # Feature modules (lazy-loaded)
│   ├── admin/
│   ├── administration/
│   ├── balances-reports/
│   ├── chart-of-accounts/
│   ├── dashboard/
│   ├── funds/
│   ├── journal-entries/
│   ├── login/
│   ├── nav/
│   ├── portfolio/
│   ├── posting-periods/
│   ├── pricing-valuation/
│   ├── profile/
│   ├── reports/
│   └── transactions/
├── layout/                  # Layout components
│   ├── breadcrumb/
│   ├── page-header/
│   ├── sidenav/
│   ├── toasts-container/
│   └── user-profile/
├── models/                  # TypeScript interfaces (kebab-case.model.ts)
├── services/                # Global singleton services
├── shared/                  # Shared utilities
│   ├── storage/            # Storage abstraction layer
│   └── stores/             # Reusable store factories
├── testing/                # Test utilities and mocks
├── app.ts                  # Root component
├── app.routes.ts           # Top-level routes
└── app.html                # Root template
```

**Naming Conventions:**
- Components: PascalCase class, kebab-case files (e.g., `fund-list.ts` exports `FundList`)
- Services: PascalCase class, kebab-case files (e.g., `toast-service.ts` exports `ToastService`)
- Directives: PascalCase class with `Directive` suffix, kebab-case files (e.g., `only-numbers.directive.ts`)
- Models: kebab-case files with `.model.ts` suffix (e.g., `account-type.model.ts`)
- Routes: kebab-case with `.routes.ts` suffix (e.g., `funds.routes.ts` exports `FUNDS_ROUTES`)

**File Co-location:**
- Components, services, directives have co-located `.spec.ts` test files
- Component templates and styles use relative imports (e.g., `./fund-list.html`, `./fund-list.scss`)
- Test files use the same base name with `.spec.ts` extension

## Key Architectural Patterns

### Signal-Based Services Pattern
```typescript
@Injectable({ providedIn: 'root' })
export class ThemeService {
  // Private writable signal
  private readonly _currentTheme = signal<'light' | 'dark'>('dark');

  // Public read-only signal
  readonly currentTheme = this._currentTheme.asReadonly();

  // Computed signal for derived state
  readonly isDarkMode = computed(() => this._currentTheme() === 'dark');
}
```

### Store Factory Pattern
```typescript
// Create reusable store with configuration
export const FundsStore = createPaginatedSearchStore({
  storageKey: 'funds-search',
  initialSort: { column: 'name', direction: 'asc' },
  defaultPageSize: 15,
});
```

### Authentication with Effects
```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly authState = toSignal(this.oidcSecurityService.isAuthenticated$);
  readonly isLoggedIn = computed(() => this.authState()?.isAuthenticated ?? false);

  constructor() {
    // Reactive effect: run side effects when signals change
    effect(() => {
      if (this.isLoggedIn()) {
        // Fetch additional user data
        this.userService.fetchCurrentUser().subscribe(...);
      }
    });
  }
}
```

### Storage Abstraction Pattern
```typescript
// Interface for testing and flexibility
export interface StorageService {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
  clear(): void;
}

// Inject the interface, not the implementation
private readonly storage = inject(SessionStorageService);
```

## Testing

Tests use **Vitest 4.0.8**. Test files use `.spec.ts` extension and are co-located with source files.

**Configuration:**
- TypeScript strict mode enabled
- Vitest configured via Angular CLI (`@angular/build:unit-test`)
- Tests run with `ng test` or `npm test`
- Code coverage with `@vitest/coverage-v8`

**Testing Utilities:**
- `InMemoryStorageService`: Mock storage implementation for tests
- `test-helpers.ts`: Shared test utilities and helpers
- Use jsdom for DOM operations in tests

## TypeScript Configuration

Strict mode enabled with:
- `strict: true`
- `noImplicitOverride: true`
- `noPropertyAccessFromIndexSignature: true`
- `noImplicitReturns: true`
- `noFallthroughCasesInSwitch: true`
- `strictInjectionParameters: true`
- `strictInputAccessModifiers: true`
- `strictTemplates: true`

Target: ES2022
