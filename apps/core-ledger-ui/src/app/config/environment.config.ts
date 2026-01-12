import { InjectionToken } from '@angular/core';
import { environment } from '../../environments/environment';

export type LogLevel = 'debug' | 'info' | 'warn' | 'error';

export interface Environment {
  production: boolean;
  apiUrl: string;
  logLevel: LogLevel;
  enableSentry: boolean;

  // Sentry Configuration
  sentry: {
    dsn: string;
    environment: string;
    release: string;
  };

  // Auth0 / OIDC Configuration
  auth: {
    authority: string;
    clientId: string;
    scope: string;
    audience: string;
  };

  // API Mock Configuration
  api?: {
    useMock: boolean;
    mockDelayMs?: number;
    mockErrorRate?: number;
  };

  // Toast Notification Delays (milliseconds)
  toast: {
    defaultDelay: number;
    successDelay: number;
    errorDelay: number;
    serviceUnavailableDelay: number;
  };

  // Pagination Defaults
  pagination: {
    defaultPageSize: number;
    apiDefaultLimit: number;
  };

  // Storage Keys
  storage: {
    theme: string;
    sidenavState: string;
  };

  // Feature Flags
  features: {
    sidenavDefaultCollapsed: boolean;
  };

  // SignalR / Notification Hub Configuration
  signalr?: {
    useMock: boolean;
    hubUrl: string;
    reconnectDelays: number[];
    maxReconnectAttempts: number;
  };
}

export const ENVIRONMENT = new InjectionToken<Environment>('Environment configuration', {
  providedIn: 'root',
  factory: () => environment,
});
