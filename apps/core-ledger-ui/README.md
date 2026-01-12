# Core Ledger UI

A modern, enterprise-grade accounting ledger application built with Angular 21. Core Ledger UI provides comprehensive accounting management features including chart of accounts, journal entries, posting periods, and financial reporting.


## 📋 Table of Contents

https://github.com/user-attachments/assets/adb69d8a-af7b-476a-97c4-af92dab0d48c

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Quick Start](#-quick-start)
- [Installation](#installation)
- [Development](#development)
  - [For Users: Running the Application](#for-users-running-the-application)
  - [For Developers: Development Workflow](#for-developers-development-workflow)
    - [Environment Configurations](#environment-configurations)
    - [Development Server](#development-server)
    - [Code Scaffolding](#code-scaffolding)
    - [Mock Authentication](#mock-authentication-development-only)
- [Project Structure](#-project-structure)
- [Testing](#-testing)
- [Building](#-building)
- [Code Standards](#-code-standards)
- [Additional Resources](#-additional-resources)

## ✨ Features

- **Chart of Accounts Management** - Create, organize, and manage account hierarchies
- **Journal Entries** - Record and track financial transactions with validation
- **Posting Periods** - Control accounting periods for accurate financial reporting
- **Balance Reports** - Generate comprehensive financial statements and reports
- **Dashboard** - Real-time overview of key financial metrics
- **Administration** - User management and system configuration
- **Toast Notifications** - Real-time user feedback for actions (success, error, warning, info)
- **Responsive Design** - Mobile-first UI using Bootstrap 5
- **Modern Angular Architecture** - Standalone components with signals-based state management

## 🛠️ Tech Stack

- **Framework:** [Angular 21](https://angular.dev)
- **State Management:** [@ngrx/signals](https://ngrx.io/guide/signals)
- **UI Framework:** [Bootstrap 5.3](https://getbootstrap.com)
- **Component Library:** [ng-bootstrap 20](https://ng-bootstrap.github.io)
- **Icons:** [Bootstrap Icons 1.13](https://icons.getbootstrap.com)
- **Testing:** [Vitest 4.0](https://vitest.dev)
- **Build Tool:** Angular CLI 21.0.4
- **Package Manager:** npm 11.6.2

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- **Node.js:** >= 18.19.0 or >= 20.9.0
- **npm:** >= 11.6.2
- **Angular CLI:** 21.0.4

```bash
# Install Angular CLI globally
npm install -g @angular/cli@21.0.4
```

## ⚡ Quick Start

Get the app running in 2 minutes:

```bash
# 1. Clone and install
git clone <repository-url>
cd core-ledger-ui
npm install

# 2. Start the development server
npm start

# 3. Open in browser
# http://localhost:4200/

# 4. Login with mock user (default)
# User: admin | Password: any
```

That's it! The app runs with mock authentication by default. No Auth0 setup required.

**Next steps - choose your path:**
- **I want to use the app** → Go to [For Users: Running the Application](#for-users-running-the-application)
- **I want to develop/test** → Go to [For Developers: Development Workflow](#for-developers-development-workflow)
- **I want to contribute** → Go to [Code Standards](#-code-standards)

## 🚀 Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd core-ledger-ui
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm start
```

4. Open your browser and navigate to `http://localhost:4200/`

## 💻 Development

### For Users: Running the Application

Simply use the Quick Start commands above. The app defaults to **mock authentication** so no Auth0 setup is needed.

**Available mock users:**
- **admin** - Full system access (13 permissions)
- **fund-manager** - Fund & ledger management (9 permissions)
- **trader** - Transaction & portfolio (7 permissions)
- **analyst** - Read-only access (7 permissions)

Change the mock user by editing `src/environments/environment.local-noauth.ts`:

```typescript
export const environment = {
  auth: {
    useMock: true,
    mockUser: 'fund-manager'  // Change this to switch users
  }
};
```

Then restart the server (`npm start`) to apply changes.

### For Developers: Development Workflow

This section covers building, testing, and extending the application.

#### Environment Configurations

The application provides multiple development configurations to support different authentication modes:

| Configuration | Auth Mode | Default | Command | Use Case |
|--------------|-----------|---------|---------|----------|
| **local-noauth** | Mock Authentication | ✅ Yes | `npm start` | Local development without Auth0 dependency |
| **local-auth** | Real Auth0 | No | `npm run start:auth` | Testing with real Auth0 integration |
| **development** | Real Auth0 | No | `ng serve --configuration=development` | Standard development mode |
| **production** | Real Auth0 | No | `ng build --configuration=production` | Production builds |

**Default Configuration:** The application now defaults to `local-noauth` for faster local development without requiring Auth0 setup.

##### Development Server

Start a local development server with live reload:

```bash
# Start with mock authentication (default)
npm start
# or
ng serve

# Start with real Auth0 authentication
npm run start:auth
# or
ng serve --configuration=local-auth
```

The application will automatically reload when you change any source files.

##### Code Scaffolding

Generate new components, services, and more:

```bash
# Generate a new component
ng generate component features/my-feature/my-component

# Generate a new service
ng generate service services/my-service

# Generate a new directive
ng generate directive directives/my-directive
```

For a complete list of available schematics:
```bash
ng generate --help
```

##### Mock Authentication

For development and testing without requiring Auth0, the application includes a mock authentication system with JWT token generation.

#### Generate Mock Tokens

The project includes a CLI tool to generate JWT tokens for mock users:

```bash
# Generate tokens for all mock users
npm run generate-tokens

# Generate token for a specific user
npm run generate-tokens admin
npm run generate-tokens fund-manager
npm run generate-tokens trader
npm run generate-tokens analyst
```

#### Architecture

The mock authentication system uses a clean separation of concerns:

- **`src/app/auth/mock-users.ts`** - User configuration (source of truth)
  - Contains user profiles, permissions, and backend user data
  - Only file you manually edit to add/modify users

- **`src/app/auth/mock-tokens.ts`** - Auto-generated JWT tokens
  - Contains JWT tokens for each mock user
  - **DO NOT EDIT MANUALLY** - generated by CLI tool
  - Includes generation timestamp
  - **Replaced with error stub in production builds**

- **`src/app/auth/mock-tokens.production.ts`** - Production safety stub
  - Throws errors if accessed in production builds
  - Automatically used via Angular file replacements

- **`tools/generate-dev-token.ts`** - Token generator CLI
  - Dynamically imports `mock-users.ts` as a TypeScript module
  - Reads user data and permissions directly from the exported object
  - Generates proper JWT tokens with claims and permissions
  - Writes entire `mock-tokens.ts` file atomically

#### Mock User Roles

The system includes four pre-configured user roles:

| Role | Permissions | Description |
|------|-------------|-------------|
| **admin** | Full access (13 permissions) | Complete system access including securities, funds, ledger, portfolio, NAV, and reports |
| **fund-manager** | Fund & ledger management (9 permissions) | Manage funds, accounts, transactions, NAV, and view reports |
| **trader** | Transaction & portfolio (7 permissions) | Execute transactions, manage portfolio, view securities and accounts |
| **analyst** | Read-only (7 permissions) | View-only access to securities, funds, accounts, transactions, portfolio, NAV, and reports |

#### Adding New Mock Users

1. Edit `src/app/auth/mock-users.ts` to add your new user
2. Define the user's `oidcUserData`, `permissions`, and `backendUser`
3. Run `npm run generate-tokens` to generate the JWT token
4. The token is automatically added to `mock-tokens.ts`

#### Enabling Mock Auth

**Quick Start:** The application defaults to mock authentication with the `local-noauth` configuration:

```bash
# Mock auth is enabled by default
npm start
```

**Customizing Mock User:** Edit `src/environments/environment.local-noauth.ts` to change the mock user:

```typescript
// src/environments/environment.local-noauth.ts
export const environment = {
  auth: {
    useMock: true,        // Enable mock auth
    mockUser: 'admin'     // Choose user role: admin, fund-manager, trader, analyst
  }
};
```

**Switching to Real Auth0:** Use the `local-auth` configuration:

```bash
npm run start:auth
```

**⚠️ Important:** Mock authentication is only available in development mode and will throw an error if used in production.

#### Production Safety

The mock authentication system has **multiple security layers** to prevent accidental use in production:

**🔒 Layer 1: Build-Time File Replacement**
- Production builds automatically replace `mock-tokens.ts` with a stub that throws errors
- Configured in `angular.json` file replacements
- Real tokens never included in production bundles

**🔒 Layer 2: Runtime Environment Check**
- `MockAuthService` validates `environment.production` flag
- Throws error immediately if production mode detected
- Prevents service instantiation in production

**🔒 Layer 3: Environment Configuration**
- Production environment files disable mock auth by default
- Requires explicit `useMock: true` flag (never set in production)

**Testing Production Safety:**
```bash
# Build for production
ng build --configuration production

# Verify mock-tokens.production.ts is used (check dist/ bundle)
# Any attempt to use mock auth will throw runtime error
```

This multi-layered approach ensures mock tokens and authentication are **completely safe** and will never reach production builds.

## ⚙️ Environment Configuration

### File Structure

```
src/
├── environments/
│   ├── environment.ts              # Default/local development
│   ├── environment.development.ts  # Development environment
│   └── environment.production.ts   # Production environment
└── app/
    └── config/
        └── environment.config.ts   # Environment injection token
```

### Environment Files

- **environment.ts** - Used when running `ng serve` without a configuration flag (default/local development)
- **environment.development.ts** - Used when building with `--configuration development`
- **environment.production.ts** - Used when building with `--configuration production`

### Using Environment Variables

#### 1. Direct Import (Simple)

```typescript
import {environment} from '../environments/environment';

@Component({...})
export class MyComponent {
  apiUrl = environment.apiUrl;
}
```

#### 2. Injection Token (Recommended for Services)

```typescript
import {inject} from '@angular/core';
import {ENVIRONMENT} from '../config/environment.config';

@Injectable({providedIn: 'root'})
export class MyService {
  private env = inject(ENVIRONMENT);

  doSomething() {
    if (this.env.production) {
      // Production logic
    }
  }
}
```

#### 3. In Providers (app.config.ts)

```typescript
import {environment} from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideMicroSentry({
      dsn: environment.sentry.dsn,
      environment: environment.sentry.environment,
      release: environment.sentry.release,
    })
  ],
};
```

### Running with Different Environments

```bash
# Development (local - no Sentry)
ng serve
# Uses: environment.ts

# Development (with Sentry)
ng serve --configuration development
# Uses: environment.development.ts

# Production Build
ng build --configuration production
# Uses: environment.production.ts

# Development Build
ng build --configuration development
# Uses: environment.development.ts
```

### Environment Schema

```typescript
interface Environment {
  production: boolean;           // Is production environment?
  apiUrl: string;               // API base URL
  logLevel: LogLevel;           // Logging level (debug, info, warn, error)
  enableSentry: boolean;        // Enable/disable Sentry error tracking
  sentry: {
    dsn: string;                // Sentry Data Source Name
    environment: string;        // Environment name for Sentry
    release: string;            // Release version
  };
  auth?: {
    // Auth0 or other auth configuration
  };
}
```

### Best Practices

1. **Never commit secrets** - Use environment variables or secrets management for sensitive data
2. **Keep structure consistent** - All environment files should have the same structure
3. **Use TypeScript types** - Define an interface for your environment configuration
4. **Default to safe values** - Local environment should be the safest (e.g., Sentry disabled)
5. **Document required values** - Comment what each configuration value does

## 📁 Project Structure

```
src/
├── app/
│   ├── config/              # Application configuration
│   ├── directives/          # Custom Angular directives
│   ├── features/            # Feature modules
│   │   ├── administration/     # User & system management
│   │   ├── balances-reports/   # Financial reporting
│   │   ├── chart-of-accounts/  # Account management
│   │   ├── dashboard/          # Main dashboard
│   │   ├── journal-entries/    # Transaction recording
│   │   └── posting-periods/    # Period management
│   ├── layout/              # Layout components (header, footer, toasts)
│   ├── models/              # TypeScript interfaces and types
│   ├── services/            # Shared services
│   │   └── toast-service.ts   # Toast notification service
│   ├── app.config.ts        # Application configuration
│   ├── app.routes.ts        # Routing configuration
│   └── app.ts               # Root component
├── public/                  # Static assets
└── main.ts                  # Application entry point
```

## 🧪 Testing

### Unit Tests

Run unit tests with Vitest:

```bash
ng test
# or
npm test
```

### Test Utilities Quick Reference

**Location:** `src/app/testing/test-helpers.ts`

#### 1. `provideTestDependencies()`

Provides all common test providers (Router, HTTP, Auth, Sentry, etc.). Use in every component or service test:

```typescript
beforeEach(async () => {
  await TestBed.configureTestingModule({
    imports: [MyComponent],
    providers: [provideTestDependencies()],
  }).compileComponents();
});
```

#### 2. `setupLocalStorageMock()`

Creates a mocked localStorage with cleanup. Use when testing services that use localStorage:

```typescript
let storageMock: ReturnType<typeof setupLocalStorageMock>;

beforeEach(() => {
  storageMock = setupLocalStorageMock();
  TestBed.configureTestingModule({...});
});

afterEach(() => {
  storageMock.clear();
});

// In test:
storageMock.store.set('key', 'value');
expect(storageMock.store.get('key')).toBe('value');
```

#### 3. `createMockService<T>()`

Creates a typed mock service object:

```typescript
const mockLogger = createMockService({
  debug: vi.fn(),
  info: vi.fn(),
  error: vi.fn(),
  warn: vi.fn(),
});

TestBed.configureTestingModule({
  providers: [
    { provide: LoggerService, useValue: mockLogger },
  ]
});
```

#### 4. `expectHttpRequest()`

Expects an HTTP request with cleaner syntax than `httpMock.expectOne()`:

```typescript
const req = expectHttpRequest(httpMock, 'GET', '/api/funds');
// OR with regex:
const req = expectHttpRequest(httpMock, 'GET', /\/api\/funds\?/);

expect(req.request.url).toContain('limit=100');
req.flush(mockResponse);
```

#### 5. `FundBuilder`

Builder for creating test Fund objects:

```typescript
const fund = new FundBuilder()
  .withId(1)
  .withName('Test Fund')
  .withDescription('A test fund')
  .withBaseCurrency('USD')
  .build();
```

#### 6. `createMockPaginatedResponse<T>()`

Creates a properly typed paginated response:

```typescript
const funds = [
  new FundBuilder().withId(1).build(),
  new FundBuilder().withId(2).build(),
];

const response = createMockPaginatedResponse(funds, 2, 100, 0);

expect(response.items.length).toBe(2);
expect(response.totalCount).toBe(2);
```

### Common Test Patterns

**Service Test with HTTP:**
```typescript
describe('FundService', () => {
  let service: FundService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_URL, useValue: '/api' }
      ]
    });
    service = TestBed.inject(FundService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should get funds', () => {
    const funds = [new FundBuilder().withId(1).build()];
    const response = createMockPaginatedResponse(funds, 1);

    service.getFunds(100, 0).subscribe(result => {
      expect(result.items.length).toBe(1);
    });

    const req = expectHttpRequest(httpMock, 'GET', /\/api\/funds/);
    req.flush(response);
  });
});
```

### Testing Best Practices

#### ✅ What SHOULD be tested

1. **Business Logic** - Data transformations, calculations, validation rules, state management
2. **Services** - Method behavior, API calls (mocked), error handling, caching, state updates
3. **Component Behavior** - Inputs/outputs, user interactions, conditional rendering, event emission
4. **Pipes** - Pure input → output transformations
5. **Critical Routing Flows** - Guards, redirects, role-based access
6. **Form Logic** - Validation rules, default values, submission behavior, field interactions

**Example:**
```typescript
it('should calculate total with tax', () => {
  const result = service.calculateTotalWithTax(100, 0.08);
  expect(result).toBe(108);
});

it('should emit search term when searching', () => {
  const spy = vi.spyOn(component.search, 'emit');
  component.onSearch('test');
  expect(spy).toHaveBeenCalledWith('test');
});
```

#### ❌ What should NOT be tested

1. **Angular Framework Internals** - DI system, change detection, HttpClient internals, Router internals
2. **HTML Structure or CSS** - Tag names, class names, layout details, DOM structure
3. **Third-Party UI Components** - Angular Material, ng-bootstrap, third-party library UI
4. **Visual Layout or Styling** - Spacing, colors, responsiveness, animations
5. **Implementation Details** - Private methods, internal variables, specific DOM query selectors

**Bad Examples to Avoid:**
```typescript
// ❌ DON'T test that Angular's DI works
it('should inject service', () => {
  expect(TestBed.inject(MyService)).toBeTruthy();
});

// ❌ DON'T test HTML structure
it('should have main tag', () => {
  expect(compiled.querySelector('main')).toBeTruthy();
});

// ❌ DON'T test CSS classes
it('should have btn-primary class', () => {
  expect(element.classList.contains('btn-primary')).toBe(true);
});
```

**Better Alternatives:**
```typescript
// ✅ Test behavior, not structure
it('should submit form when button is clicked', () => {
  const spy = vi.spyOn(component, 'onSubmit');
  const button = fixture.nativeElement.querySelector('[type="submit"]');
  button.click();
  expect(spy).toHaveBeenCalled();
});
```

#### 🔄 Refactoring Common Anti-Patterns

**Remove "should be created" tests:**
```typescript
// ❌ DON'T
it('should be created', () => {
  expect(service).toBeTruthy();
});

// ✅ DO - Focus on actual behavior
describe('getFunds', () => {
  it('should fetch funds with pagination', () => {
    // Test actual behavior
  });
});
```

**Convert DOM structure tests to behavior tests:**
```typescript
// ❌ DON'T
it('should render main layout', () => {
  expect(compiled.querySelector('main')).toBeTruthy();
  expect(compiled.querySelector('router-outlet')).toBeTruthy();
});

// ✅ DO
it('should allow navigation to child routes', () => {
  router.navigate(['/dashboard']);
  expect(router.url).toBe('/dashboard');
});
```

**Convert CSS class tests to state tests:**
```typescript
// ❌ DON'T
it('should have asc class', () => {
  expect(nameElement.classList.contains('asc')).toBe(true);
});

// ✅ DO
it('should emit ascending sort event', () => {
  component.onSort('name');
  expect(component.sortEvent()).toEqual({ column: 'name', direction: 'asc' });
});
```

#### 📝 Testing Checklist

Before writing a test, ask yourself:

- [ ] Am I testing **my business logic** or framework internals?
- [ ] Am I testing **behavior** or implementation details?
- [ ] Would this test break if I refactor the HTML/CSS?
- [ ] Am I testing through the **public API** only?
- [ ] Does this test add value or just inflate coverage?

**If you answered "framework", "implementation", "yes", "no", or "inflate" to any question, reconsider the test.**

### End-to-End Tests

E2E testing framework is not configured by default. Choose and configure one based on your needs (e.g., Playwright, Cypress).

### For Contributors: Contributing to the Project

Want to improve Core Ledger UI? Great! Here's how to contribute:

**Before you start:**
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Familiarize yourself with the Code Standards section below

**Development workflow:**
1. Make your changes following code standards
2. Write tests for new features (see Testing section)
3. Run tests to ensure nothing breaks: `npm test`
4. Build to verify production build works: `ng build --configuration production`

**Before submitting a PR:**
- [ ] Code follows project style guide
- [ ] Tests pass: `npm test`
- [ ] Build succeeds: `ng build`
- [ ] Documentation is updated (if needed)
- [ ] Commit messages are clear and descriptive

For detailed contribution guidelines, see `CONTRIBUTING.md` (if available) or create a feature branch and submit a pull request.

## 🏗️ Building

### Development Build

```bash
ng build
```

### Production Build

```bash
ng build --configuration production
```

Build artifacts will be stored in the `dist/` directory. The production build optimizes the application for performance and speed.

### Watch Mode

Automatically rebuild on file changes:

```bash
# Watch mode with mock authentication (default)
npm run watch

# Watch mode with real Auth0 authentication
npm run watch:auth
```

## 📏 Code Standards

This project follows Angular and TypeScript best practices:

- **Standalone Components** - All components use the standalone API
- **Signals** - Reactive state management using Angular signals
- **TypeScript Strict Mode** - Enabled for type safety
- **SCSS** - Styling with SCSS preprocessor
- **Prettier** - Code formatting (printWidth: 100, singleQuote: true)
- **Bootstrap Utilities** - Use Bootstrap classes, avoid inline styles

### Formatting

Code is formatted with Prettier. Configuration is in `package.json`:

```bash
# Format code (if formatter is set up)
npx prettier --write "src/**/*.{ts,html,scss}"
```

## 📚 Additional Resources

- [Angular Documentation](https://angular.dev)
- [Angular CLI Reference](https://angular.dev/tools/cli)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.3)
- [ng-bootstrap Components](https://ng-bootstrap.github.io)
- [NgRx Signals](https://ngrx.io/guide/signals)
