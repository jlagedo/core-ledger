import { EnvironmentProviders, Provider } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { MicroSentryService } from '@micro-sentry/angular';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { of } from 'rxjs';
import { ENVIRONMENT } from '../config/environment.config';
import { API_URL } from '../config/api.config';
import { MockAuthService } from '../auth/mock-auth.service';

/**
 * Provides common test providers including router, HTTP client, OIDC authentication,
 * environment config, and API URL. Use this in test files to avoid duplication.
 */
export function provideTestDependencies(): (Provider | EnvironmentProviders)[] {
  return [
    provideRouter([]),
    provideLocationMocks(),
    provideHttpClient(),
    {
      provide: OidcSecurityService,
      useValue: {
        isAuthenticated$: of({ isAuthenticated: false }),
        userData$: of({ userData: null }),
        authorize: vi.fn(),
        logoff: vi.fn(),
        checkAuth: vi.fn().mockReturnValue(of({ isAuthenticated: false })),
      }
    },
    {
      provide: MockAuthService,
      useValue: {
        isAuthenticated$: of({ isAuthenticated: true }),
        userData$: of({
          userData: {
            sub: 'test-user',
            name: 'Test User',
            email: 'test@example.com',
            picture: 'https://via.placeholder.com/150',
          }
        }),
        authorize: vi.fn(),
        logoff: vi.fn(),
        checkAuth: vi.fn().mockReturnValue(of({ isAuthenticated: true })),
        getMockBackendUser: vi.fn().mockReturnValue({
          id: '1',
          email: 'test@example.com',
          name: 'Test User',
          role: 'admin',
        }),
      }
    },
    {
      provide: MicroSentryService,
      useValue: {
        withScope: vi.fn(),
        captureMessage: vi.fn(),
        report: vi.fn(),
      }
    },
    {
      provide: ENVIRONMENT,
      useValue: {
        production: false,
        apiUrl: '/api',
        logLevel: 'debug' as const,
        enableSentry: false,
        sentry: {
          dsn: '',
          environment: 'test',
          release: '1.0.0',
        },
        auth: {
          useMock: true,
          mockUser: 'admin',
          authority: 'https://test.auth.com',
          clientId: 'test-client',
          scope: 'openid profile email',
          audience: 'test-api',
        },
        api: {
          useMock: false, // Tests use HttpTestingController, not mock interceptor
          mockDelayMs: 0,
          mockErrorRate: 0,
        },
        toast: {
          defaultDelay: 3000,
          successDelay: 3000,
          errorDelay: 5000,
          serviceUnavailableDelay: 10000,
        },
        pagination: {
          defaultPageSize: 100,
          apiDefaultLimit: 100,
        },
        storage: {
          themeKey: 'theme',
        },
        features: {
          enableAdvancedReporting: true,
        },
      }
    },
    {
      provide: API_URL,
      useValue: '/api'
    }
  ];
}

/**
 * Setup localStorage mock with proper cleanup
 * Call in beforeEach and use the returned utilities in tests
 *
 * @example
 * beforeEach(() => {
 *   const storage = setupLocalStorageMock();
 * });
 *
 * afterEach(() => {
 *   vi.unstubAllGlobals();
 * });
 */
export function setupLocalStorageMock(): {
  store: Map<string, string>;
  clear: () => void;
} {
  const store = new Map<string, string>();

  vi.stubGlobal('localStorage', {
    getItem: (key: string) => store.get(key) ?? null,
    setItem: (key: string, value: string) => store.set(key, value),
    removeItem: (key: string) => store.delete(key),
    clear: () => store.clear(),
    length: store.size,
    key: (index: number) => Array.from(store.keys())[index] ?? null,
  });

  return {
    store,
    clear: () => {
      store.clear();
      vi.unstubAllGlobals();
    },
  };
}
