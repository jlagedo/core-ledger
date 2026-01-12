import { Injectable, inject, signal, computed, effect, DestroyRef } from '@angular/core';
import { ToastService } from './toast-service';
import { LoggerService } from './logger';
import { AuthService } from '../auth/auth-service';
import { ENVIRONMENT } from '../config/environment.config';
import { HubConnectionState, NotificationMessage } from '../models/notification.model';
import { environment } from '../../environments/environment';

/**
 * Serviço mock de hub de notificação para desenvolvimento e testes.
 * Simula o comportamento do hub SignalR sem exigir uma conexão real.
 *
 * IMPORTANTE: Apenas para desenvolvimento. Lançará erro se usado em produção.
 *
 * Uso:
 * - "Conecta" automaticamente quando usuário faz login
 * - Fornece métodos para disparar notificações de teste
 * - Simula estados de conexão para testes de UI
 *
 * @remarks
 * Intentionally NOT using `providedIn: 'root'` because this service is
 * conditionally provided based on environment.signalr.useMock flag.
 * See `src/app/app.config.ts` for conditional provider setup.
 *
 * @internal This service is replaced with real NotificationHubService in production.
 */
@Injectable()
export class MockNotificationHubService {
  private readonly env = inject(ENVIRONMENT);
  private readonly toastService = inject(ToastService);
  private readonly logger = inject(LoggerService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  // Sinais privados graváveis
  private readonly _connectionState = signal<HubConnectionState>('disconnected');
  private readonly _lastError = signal<string | null>(null);
  private readonly _reconnectAttempts = signal<number>(0);

  // Sinais públicos somente leitura
  readonly connectionState = this._connectionState.asReadonly();
  readonly lastError = this._lastError.asReadonly();
  readonly reconnectAttempts = this._reconnectAttempts.asReadonly();

  // Sinais computados
  readonly isConnected = computed(() => this._connectionState() === 'connected');
  readonly isReconnecting = computed(() => this._connectionState() === 'reconnecting');

  constructor() {
    // Verificação de segurança de produção
    if (environment.production) {
      throw new Error('CRÍTICO: Hub de notificação mock não pode ser ativado em produção!');
    }

    this.logger.debug('MockNotificationHubService inicializado', undefined, 'MockNotificationHubService');

    // Efeito reativo: simular conectar/desconectar baseado no estado de autenticação
    effect(() => {
      const isLoggedIn = this.authService.isLoggedIn();

      if (isLoggedIn && !this.isConnected()) {
        this.connect();
      } else if (!isLoggedIn && this.isConnected()) {
        this.disconnect();
      }
    });

    // Limpeza ao destruir
    this.destroyRef.onDestroy(() => {
      this.disconnect();
    });
  }

  /**
   * Simula estabelecer conexão com o hub SignalR.
   */
  async connect(): Promise<void> {
    this._connectionState.set('connecting');
    this.logger.debug('Mock: Conectando ao hub de notificações', undefined, 'MockNotificationHubService');

    // Simular atraso de conexão
    await this.delay(200);

    this._connectionState.set('connected');
    this._lastError.set(null);
    this._reconnectAttempts.set(0);

    this.logger.info('Mock: Conectado ao hub de notificações', undefined, 'MockNotificationHubService');

    // Mostrar notificação de boas-vindas após breve atraso
    setTimeout(() => {
      if (this.isConnected()) {
        this.sendTestNotification({
          message: 'Notificações em tempo real ativadas (Modo Mock)',
          type: 'info',
        });
      }
    }, 500);
  }

  /**
   * Simula desconectar do hub SignalR.
   */
  async disconnect(): Promise<void> {
    this._connectionState.set('disconnected');
    this._lastError.set(null);
    this._reconnectAttempts.set(0);

    this.logger.info('Mock: Desconectado do hub de notificações', undefined, 'MockNotificationHubService');
  }

  /**
   * Envia uma notificação de teste através do hub mock.
   * Útil para testar a manipulação de notificações sem um backend real.
   *
   * @param notification A notificação a enviar
   */
  sendTestNotification(notification: NotificationMessage): void {
    if (!this.isConnected()) {
      this.logger.warn(
        'Mock: Não é possível enviar notificação - não conectado',
        undefined,
        'MockNotificationHubService'
      );
      return;
    }

    this.logger.debug('Mock: Enviando notificação de teste', { notification }, 'MockNotificationHubService');

    // Exibir notificação via ToastService
    switch (notification.type) {
      case 'success':
        this.toastService.success(notification.message);
        break;
      case 'warning':
        this.toastService.warning(notification.message);
        break;
      case 'error':
        this.toastService.error(notification.message);
        break;
      case 'info':
      default:
        this.toastService.info(notification.message);
        break;
    }
  }

  /**
   * Simula um cenário de reconexão para testes.
   */
  async simulateReconnect(): Promise<void> {
    this._connectionState.set('reconnecting');
    this._reconnectAttempts.update((n) => n + 1);

    this.logger.debug('Mock: Simulando reconexão', undefined, 'MockNotificationHubService');

    await this.delay(1000);

    this._connectionState.set('connected');
    this._reconnectAttempts.set(0);

    this.logger.info('Mock: Reconectado', undefined, 'MockNotificationHubService');
  }

  /**
   * Simula uma falha de conexão para testes.
   */
  simulateConnectionFailure(errorMessage: string): void {
    this._connectionState.set('failed');
    this._lastError.set(errorMessage);

    this.logger.error(
      'Mock: Falha de conexão simulada',
      { error: errorMessage },
      'MockNotificationHubService'
    );
  }

  /**
   * Método utilitário para criar um atraso.
   */
  private delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
