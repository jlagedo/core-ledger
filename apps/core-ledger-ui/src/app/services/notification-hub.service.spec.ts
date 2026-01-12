import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { NotificationHubService } from './notification-hub.service';
import { ToastService } from './toast-service';
import { LoggerService } from './logger';
import { AuthService } from '../auth/auth-service';
import { MockAuthService } from '../auth/mock-auth.service';
import { ENVIRONMENT } from '../config/environment.config';

describe('NotificationHubService', () => {
  let service: NotificationHubService;
  let mockIsLoggedIn: ReturnType<typeof signal<boolean>>;

  const mockEnvironment = {
    production: false,
    logLevel: 'debug' as const,
    signalr: {
      useMock: true, // Use mock to avoid real SignalR connection in tests
      hubUrl: '/api/hub/notifications',
      reconnectDelays: [0, 1000],
      maxReconnectAttempts: 3,
    },
    toast: { defaultDelay: 5000 },
  };

  beforeEach(() => {
    mockIsLoggedIn = signal(false);

    TestBed.configureTestingModule({
      providers: [
        NotificationHubService,
        {
          provide: ToastService,
          useValue: {
            success: vi.fn(),
            warning: vi.fn(),
            error: vi.fn(),
            info: vi.fn(),
          },
        },
        {
          provide: LoggerService,
          useValue: {
            debug: vi.fn(),
            info: vi.fn(),
            warn: vi.fn(),
            error: vi.fn(),
          },
        },
        {
          provide: AuthService,
          useValue: {
            isLoggedIn: mockIsLoggedIn.asReadonly(),
          },
        },
        {
          provide: OidcSecurityService,
          useValue: {
            getAccessToken: vi.fn().mockReturnValue(of('mock-token')),
          },
        },
        {
          provide: MockAuthService,
          useValue: {
            getAccessToken: vi.fn().mockReturnValue('mock-token'),
          },
        },
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });

    service = TestBed.inject(NotificationHubService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should start disconnected', () => {
    expect(service.connectionState()).toBe('disconnected');
    expect(service.isConnected()).toBe(false);
  });

  it('should expose read-only signals', () => {
    expect(service.connectionState).toBeDefined();
    expect(service.lastError).toBeDefined();
    expect(service.reconnectAttempts).toBeDefined();
    expect(service.isConnected).toBeDefined();
    expect(service.isReconnecting).toBeDefined();
  });

  it('should have isConnected as false when disconnected', () => {
    expect(service.isConnected()).toBe(false);
  });

  it('should have isReconnecting as false when disconnected', () => {
    expect(service.isReconnecting()).toBe(false);
  });

  it('should have null lastError initially', () => {
    expect(service.lastError()).toBeNull();
  });

  it('should have 0 reconnect attempts initially', () => {
    expect(service.reconnectAttempts()).toBe(0);
  });

  it('should skip connection in mock mode', async () => {
    // Service should not attempt real connection when useMock is true
    await service.connect();
    // Connection state remains disconnected in mock mode (no real connection)
    expect(service.connectionState()).toBe('disconnected');
  });
});
