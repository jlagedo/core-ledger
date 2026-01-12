import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { MockNotificationHubService } from './mock-notification-hub.service';
import { ToastService } from './toast-service';
import { LoggerService } from './logger';
import { AuthService } from '../auth/auth-service';
import { ENVIRONMENT } from '../config/environment.config';

describe('MockNotificationHubService', () => {
  let service: MockNotificationHubService;
  let toastServiceMock: {
    success: ReturnType<typeof vi.fn>;
    warning: ReturnType<typeof vi.fn>;
    error: ReturnType<typeof vi.fn>;
    info: ReturnType<typeof vi.fn>;
  };
  let mockIsLoggedIn: ReturnType<typeof signal<boolean>>;

  const mockEnvironment = {
    production: false,
    logLevel: 'debug' as const,
    signalr: {
      useMock: true,
      hubUrl: '/api/hub/notifications',
      reconnectDelays: [0, 1000],
      maxReconnectAttempts: 3,
    },
    toast: { defaultDelay: 5000 },
  };

  beforeEach(() => {
    mockIsLoggedIn = signal(false);
    toastServiceMock = {
      success: vi.fn(),
      warning: vi.fn(),
      error: vi.fn(),
      info: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        MockNotificationHubService,
        {
          provide: ToastService,
          useValue: toastServiceMock,
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
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });

    service = TestBed.inject(MockNotificationHubService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should start disconnected', () => {
    expect(service.connectionState()).toBe('disconnected');
    expect(service.isConnected()).toBe(false);
  });

  it('should have read-only signals exposed', () => {
    expect(service.connectionState).toBeDefined();
    expect(service.lastError).toBeDefined();
    expect(service.reconnectAttempts).toBeDefined();
    expect(service.isConnected).toBeDefined();
    expect(service.isReconnecting).toBeDefined();
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

  it('should connect and set connected state', async () => {
    await service.connect();

    expect(service.connectionState()).toBe('connected');
    expect(service.isConnected()).toBe(true);
  });

  it('should disconnect and set disconnected state', async () => {
    await service.connect();
    await service.disconnect();

    expect(service.connectionState()).toBe('disconnected');
    expect(service.isConnected()).toBe(false);
  });

  it('should not send notification when disconnected', () => {
    service.sendTestNotification({ message: 'Should not show', type: 'success' });

    expect(toastServiceMock.success).not.toHaveBeenCalled();
  });

  it('should send success notification when connected', async () => {
    await service.connect();

    service.sendTestNotification({ message: 'Test success', type: 'success' });

    expect(toastServiceMock.success).toHaveBeenCalledWith('Test success');
  });

  it('should send warning notification when connected', async () => {
    await service.connect();

    service.sendTestNotification({ message: 'Test warning', type: 'warning' });

    expect(toastServiceMock.warning).toHaveBeenCalledWith('Test warning');
  });

  it('should send error notification when connected', async () => {
    await service.connect();

    service.sendTestNotification({ message: 'Test error', type: 'error' });

    expect(toastServiceMock.error).toHaveBeenCalledWith('Test error');
  });

  it('should send info notification when connected', async () => {
    await service.connect();

    service.sendTestNotification({ message: 'Test info', type: 'info' });

    expect(toastServiceMock.info).toHaveBeenCalledWith('Test info');
  });

  it('should set failed state on simulateConnectionFailure', async () => {
    await service.connect();

    service.simulateConnectionFailure('Test error');

    expect(service.connectionState()).toBe('failed');
    expect(service.lastError()).toBe('Test error');
  });

  it('should reset state on disconnect after failure', async () => {
    await service.connect();
    service.simulateConnectionFailure('Error');
    await service.disconnect();

    expect(service.connectionState()).toBe('disconnected');
    expect(service.lastError()).toBeNull();
    expect(service.reconnectAttempts()).toBe(0);
  });
});
