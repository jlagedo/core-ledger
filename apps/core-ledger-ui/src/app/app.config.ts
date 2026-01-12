import { ApplicationConfig, inject, isDevMode, provideAppInitializer, provideBrowserGlobalErrorListeners, } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor, OidcSecurityService, provideAuth } from 'angular-auth-oidc-client';
import { firstValueFrom } from 'rxjs';
import { provideMicroSentry } from '@micro-sentry/angular';

// AG Grid module registration
import { ModuleRegistry } from 'ag-grid-community';
import { AllCommunityModule } from 'ag-grid-community';

// Register AG Grid modules globally
ModuleRegistry.registerModules([AllCommunityModule]);

import { routes } from './app.routes';
import { authConfig } from './auth/auth.config';
import { environment } from '../environments/environment';
import { mockAuthInterceptor } from './auth/mock-auth.interceptor';
import { MockAuthService } from './auth/mock-auth.service';
import { mockApiInterceptor } from './api/mock-api.interceptor';
import { MockApiService } from './api/mock-api.service';
import { NotificationHubService } from './services/notification-hub.service';
import { MockNotificationHubService } from './services/mock-notification-hub.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // Conditional HTTP interceptors based on mock flags
    provideHttpClient(
      withInterceptors([
        // Mock API interceptor must come first to intercept API requests before auth
        ...(environment.api?.useMock ? [mockApiInterceptor] : []),
        environment.auth.useMock ? mockAuthInterceptor : authInterceptor()
      ])
    ),
    // Conditionally provide MockApiService only when useMock is true
    ...(environment.api?.useMock ? [MockApiService] : []),
    // Conditionally provide MockAuthService only when useMock is true
    ...(environment.auth.useMock ? [MockAuthService] : []),
    // Conditionally provide MockNotificationHubService only when signalr.useMock is true
    ...(environment.signalr?.useMock ? [MockNotificationHubService] : []),
    // Always provide OIDC (needed for types even in mock mode)
    provideAuth(authConfig),
    // Conditional auth initializer
    provideAppInitializer(() => {
      if (environment.auth.useMock) {
        const mockAuthService = inject(MockAuthService);
        return firstValueFrom(mockAuthService.checkAuth());
      } else {
        const oidcSecurityService = inject(OidcSecurityService);
        return firstValueFrom(oidcSecurityService.checkAuth());
      }
    }),
    // Initialize notification hub service (connects after auth via effect())
    provideAppInitializer(() => {
      if (environment.signalr?.useMock) {
        inject(MockNotificationHubService);
      } else {
        inject(NotificationHubService);
      }
      return Promise.resolve();
    }),
    // Configure Sentry from environment
    provideMicroSentry({
      dsn: environment.sentry.dsn,
      environment: environment.sentry.environment,
      release: environment.sentry.release,
    })
  ],
};
