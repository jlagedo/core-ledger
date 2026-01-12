import { TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { LoggerService } from './logger';
import { ToastService } from './toast-service';
import { MicroSentryService } from '@micro-sentry/angular';
import { ENVIRONMENT } from '../config/environment.config';

describe('LoggerService', () => {
  let service: LoggerService;
  let toastService: ToastService;
  let mockMicroSentryService: { [key: string]: any };
  let mockEnvironment: any;

  beforeEach(() => {
    // Create mock scope function that captures calls
    const mockScope = {
      setTag: vi.fn(),
      setExtra: vi.fn(),
      report: vi.fn(),
      captureMessage: vi.fn(),
    };

    mockMicroSentryService = {
      withScope: vi.fn((fn: (scope: any) => void) => fn(mockScope)),
      captureMessage: vi.fn(),
      report: vi.fn(),
    };

    mockEnvironment = {
      production: false,
      apiUrl: '/api',
      logLevel: 'debug' as const,
      enableSentry: true,
      sentry: {
        dsn: 'test-dsn',
        environment: 'test',
        release: '1.0.0',
      },
      auth: {
        authority: 'https://test.auth.com',
        clientId: 'test-client',
        scope: 'openid profile email',
        audience: 'test-api',
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
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: MicroSentryService, useValue: mockMicroSentryService },
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });

    service = TestBed.inject(LoggerService);
    toastService = TestBed.inject(ToastService);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('logging methods', () => {
    it('should log debug messages with context', () => {
      const consoleSpy = vi.spyOn(console, 'debug');
      service.debug('Test debug message', { foo: 'bar' });
      expect(consoleSpy).toHaveBeenCalledWith(expect.any(String), expect.objectContaining({ foo: 'bar' }));
      consoleSpy.mockRestore();
    });

    it('should log info messages', () => {
      const consoleSpy = vi.spyOn(console, 'info');
      service.info('Test info message');
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    it('should log warning messages', () => {
      const consoleSpy = vi.spyOn(console, 'warn');
      service.warn('Test warning message');
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    it('should log error messages', () => {
      const consoleSpy = vi.spyOn(console, 'error');
      service.error('Test error message');
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('HTTP error logging', () => {
    it('should log HTTP errors and show toast when requested', () => {
      const toastSpy = vi.spyOn(toastService, 'error');
      const error = {
        status: 500,
        error: {
          message: 'Internal Server Error',
          correlationId: '123',
        },
      };

      service.logHttpError('fetchData', error, 'Failed to load data');

      expect(toastSpy).toHaveBeenCalledWith('Failed to load data');
      toastSpy.mockRestore();
    });

    it('should log HTTP errors without toast when disabled', () => {
      const toastSpy = vi.spyOn(toastService, 'error');
      const error = {
        status: 404,
        error: { message: 'Not Found' },
      };

      service.logHttpError('fetchData', error, undefined, false);

      expect(toastSpy).not.toHaveBeenCalled();
      toastSpy.mockRestore();
    });
  });

  describe('Sentry integration', () => {
    it('should send errors to Sentry', () => {
      service.error('Test error', { foo: 'bar' }, 'TestContext');
      expect(mockMicroSentryService['withScope']).toHaveBeenCalled();
    });

    it('should send warnings to Sentry', () => {
      service.warn('Test warning', { foo: 'bar' }, 'TestContext');
      expect(mockMicroSentryService['withScope']).toHaveBeenCalled();
    });

    it('should not send info logs to Sentry', () => {
      service.info('Test info', { foo: 'bar' }, 'TestContext');
      expect(mockMicroSentryService['withScope']).not.toHaveBeenCalled();
    });

    it('should not send debug logs to Sentry', () => {
      service.debug('Test debug', { foo: 'bar' }, 'TestContext');
      expect(mockMicroSentryService['withScope']).not.toHaveBeenCalled();
    });

    it('should send HTTP errors to Sentry with context', () => {
      const error = {
        status: 500,
        error: {
          errorCode: 'ERR_500',
          message: 'Internal Server Error',
          correlationId: '123-abc',
          traceId: '456-def',
        },
      };

      service.logHttpError('fetchData', error, 'Failed to load data');
      expect(mockMicroSentryService['withScope']).toHaveBeenCalled();
    });

    it('should not send errors to Sentry when disabled', () => {
      TestBed.resetTestingModule();
      const disabledEnvironment = { ...mockEnvironment, enableSentry: false };

      TestBed.configureTestingModule({
        providers: [
          { provide: MicroSentryService, useValue: mockMicroSentryService },
          { provide: ENVIRONMENT, useValue: disabledEnvironment },
        ],
      });

      const disabledService = TestBed.inject(LoggerService);
      vi.clearAllMocks();

      disabledService.error('Test error', { foo: 'bar' });
      expect(mockMicroSentryService['withScope']).not.toHaveBeenCalled();
    });
  });
});
