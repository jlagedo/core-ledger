/**
 * Production stub for mock notification hub service.
 * This file replaces mock-notification-hub.service.ts in production builds via angular.json fileReplacements.
 *
 * PRODUCTION BUILD - MOCK NOTIFICATION HUB DISABLED
 *
 * All class members throw errors when accessed, preventing accidental use in production.
 */

import { Injectable, signal, computed } from '@angular/core';
import { HubConnectionState, NotificationMessage } from '../models/notification.model';

@Injectable()
export class MockNotificationHubService {
  readonly connectionState = signal<HubConnectionState>('disconnected').asReadonly();
  readonly lastError = signal<string | null>(null).asReadonly();
  readonly reconnectAttempts = signal<number>(0).asReadonly();
  readonly isConnected = computed(() => false);
  readonly isReconnecting = computed(() => false);

  constructor() {
    throw new Error(
      'CRITICAL SECURITY ERROR: MockNotificationHubService is not available in production. ' +
        'This indicates a build configuration error. Please verify that:\n' +
        '1. environment.signalr.useMock is set to false in environment.production.ts\n' +
        '2. Angular build fileReplacements are correctly configured in angular.json\n' +
        '3. The production build was executed with --configuration=production'
    );
  }

  connect(): Promise<void> {
    throw new Error('Mock notification hub is not available in production');
  }

  disconnect(): Promise<void> {
    throw new Error('Mock notification hub is not available in production');
  }

  sendTestNotification(_notification: NotificationMessage): void {
    throw new Error('Mock notification hub is not available in production');
  }

  simulateReconnect(): Promise<void> {
    throw new Error('Mock notification hub is not available in production');
  }

  simulateConnectionFailure(_errorMessage: string): void {
    throw new Error('Mock notification hub is not available in production');
  }
}
